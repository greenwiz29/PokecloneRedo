using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


public class WildPokemonController : OverworldEntity
{
    [Header("Movement")]
    [SerializeField] float wanderDelay = 1.5f;
    [SerializeField] float maxDistanceFromSpawner = 5f;
    [SerializeField] SpriteRenderer sprite;

    WildPursuitProfile pursuitProfile;

    public Pokemon WildPokemon { get; private set; }
    public bool StateNotIdle => state != NPCState.Idle;
    public float WanderDelay => wanderDelay;
    public float LastAggroTime => lastAggroTime;
    public Territory Territory => territory;
    public WildPursuitProfile Profile => pursuitProfile;

    Vector3 spawnPoint;
    WildPokemonBehavior behavior;
    Coroutine behaviorRoutine;
    Territory territory;

    float lastAggroTime;

    public enum WildMode { Neutral, Aggro, Fleeing }

    public WildMode CurrentMode { get; private set; } = WildMode.Neutral;
    VisualAltitudeController verticalPresence = null;


    protected override void Awake()
    {
        base.Awake();
        spawnPoint = transform.position;
    }

    protected override IEnumerator OnInteract(Transform initiator)
    {
        if (!AllowsInteraction())
            yield break;

        var player = initiator.GetComponent<PlayerController>();
        if (player == null)
            yield break;

        GameController.I.StartOverworldPokemonBattle(this);
    }

    public bool AllowsInteraction()
    {
        return verticalPresence?.AllowsInteraction() ?? true;
    }

    public void SetMode(WildMode mode)
    {
        CurrentMode = mode;
    }

    public bool CanMove(Vector2 dir)
    {
        var target = transform.position + (Vector3)dir;
        return Vector3.Distance(spawnPoint, target) <= maxDistanceFromSpawner;
    }

    public bool CanChaseTo(Vector3 pos)
    {
        // 1. Spawn leash
        if (Profile.maxChaseDistanceFromSpawn > 0f)
        {
            float distFromSpawn = Vector3.Distance(pos, spawnPoint);
            if (distFromSpawn > Profile.maxChaseDistanceFromSpawn)
                return false;
        }

        // 2. Territory leash (optional)
        if (territory != null && Profile.maxChaseDistanceFromTerritory > 0f)
        {
            float distFromTerritory = Vector3.Distance(pos, territory.center);
            if (distFromTerritory > Profile.maxChaseDistanceFromTerritory)
                return false;
        }

        return true;
    }

    public IEnumerator Move(Vector2 dir)
    {
        state = NPCState.Walking;
        yield return character.Move(dir);
        state = NPCState.Idle;
    }

    public void Init(MapArea mapArea, Vector3 spawnPoint, BattleTrigger trigger = BattleTrigger.LongGrass, Territory territory = null)
    {
        var wild = mapArea.GetRandomWildPokemon(trigger);
        if (wild == null)
        {
            Debug.LogError("No valid wild Pokémon generated");
            Destroy(gameObject);
            return;
        }
        WildPokemon = new Pokemon(wild.Base, wild.Level);

        // Set character sprites
        character.Animator.SetWalkingDownSprites(WildPokemon.WalkDownAnim);
        character.Animator.SetWalkingUpSprites(WildPokemon.WalkUpAnim);
        character.Animator.SetWalkingLeftSprites(WildPokemon.WalkLeftAnim);
        character.Animator.SetWalkingRightSprites(WildPokemon.WalkRightAnim);

        var profile = WildPokemon.Base.VerticalPresence;
        if (profile != null)
        {
            verticalPresence = new VisualAltitudeController(this, sprite, profile);
            if (!profile.usesGroundFooting)
            {
                character.OffsetY = 0f;
            }
            else
            {
                character.OffsetY = 0.3f;
            }

        }

        this.spawnPoint = spawnPoint;
        this.territory = territory;
        this.territory?.Register(this);
        character.SetPositionAndSnapToTile(spawnPoint);

        var behavior = WildBehaviorResolver.Resolve(WildPokemon, mapArea, territory);
        pursuitProfile = WildPokemon.Base.DefaultPursuitProfile;

        InitializeBehavior(behavior);
    }

    protected override void Update()
    {
        base.Update();
        verticalPresence?.Tick();
    }

    public void InitializeBehavior(WildPokemonBehavior newBehavior)
    {
        behavior = newBehavior;

        if (behaviorRoutine != null)
            StopCoroutine(behaviorRoutine);

        behaviorRoutine = StartCoroutine(behavior.Run(this));
    }

    public void SwitchBehavior(WildPokemonBehavior newBehavior)
    {
        if (behavior == newBehavior)
            return;

        InitializeBehavior(newBehavior);
    }

    public void OnTerritoryThreat(Transform threat)
    {
#if UNITY_EDITOR
        lastThreat = threat;
        lastThreatTime = Time.time;
#endif

        // Forward to current behavior if it cares
        if (behavior is ITerritoryReactive reactive)
            reactive.OnTerritoryThreat(this, threat);
    }

    public float LastMeasuredPlayerDistance { get; private set; } = float.MaxValue;

    public bool IsClosingDistanceTo(Transform target)
    {
        float current = Vector3.Distance(transform.position, target.position);

        bool closing = current < LastMeasuredPlayerDistance - 0.01f;
        LastMeasuredPlayerDistance = current;

        return closing;
    }

    public void ResetChaseMetrics()
    {
        LastMeasuredPlayerDistance = float.MaxValue;
    }

    public void EnterAggro()
    {
        ResetChaseMetrics();
        CurrentMode = WildMode.Aggro;
        verticalPresence?.SetInteractionPlaneBias(InteractionPlaneBias.Converge);
    }

    public void EnterFlee()
    {
        ResetChaseMetrics();
        CurrentMode = WildMode.Fleeing;
        verticalPresence?.SetInteractionPlaneBias(InteractionPlaneBias.Diverge);
    }

    public void ExitReactiveMode()
    {
        lastAggroTime = Time.time;
        ResetChaseMetrics();
        CurrentMode = WildMode.Neutral;
        verticalPresence?.SetInteractionPlaneBias(InteractionPlaneBias.Neutral);
    }

    public void OnBattleOver()
    {
        Destroy(gameObject); // Removed only AFTER battle
    }

    void OnDestroy()
    {
        territory?.Unregister(this);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (Profile != null && Profile.maxChaseDistanceFromSpawn > 0f)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spawnPoint, Profile.maxChaseDistanceFromSpawn);
        }
    }

    void OnDrawGizmos()
    {
        DrawBehaviorLabel();
        DrawStateRing();
        DrawModeRing();
        DrawTerritorySignal();
    }

    Transform lastThreat;
    float lastThreatTime;

    void DrawBehaviorLabel()
    {
        if (behavior == null) return;

        Handles.Label(
            transform.position + Vector3.up * 1.2f,
            behavior.GetType().Name
        );
    }

    void DrawStateRing()
    {
        Color ringColor = state switch
        {
            NPCState.Idle => Color.green,
            NPCState.Walking => Color.yellow,
            _ => Color.white
        };

        Gizmos.color = ringColor;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }

    void DrawModeRing()
    {
        Color modeColor = CurrentMode switch
        {
            WildMode.Aggro => Color.red,
            WildMode.Fleeing => Color.blue,
            _ => Color.clear
        };

        if (modeColor.a <= 0f)
            return;

        Gizmos.color = modeColor;
        Gizmos.DrawWireSphere(transform.position, 0.6f);
    }

    void DrawTerritorySignal()
    {
        if (lastThreat == null) return;
        if (Time.time - lastThreatTime > 1f) return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, lastThreat.position);
    }
#endif

}

public enum WildPersonality { Passive, Timid, Aggressive, Territorial }

public static class WildBehaviorResolver
{
    public static WildPokemonBehavior Resolve(Pokemon pokemon, MapArea mapArea, Territory territory)
    {
        var personality = ResolvePersonality(pokemon, mapArea);

        var behaviors = pokemon.Base.PossibleBehaviors;

        var behavior = behaviors
            .Find(b => b.Personality == personality);

        if (behavior == null)
        {
            Debug.LogWarning(
                $"No behavior for {personality} on {pokemon.Base.name}, falling back to Passive"
            );
            behavior = behaviors
                .Find(b => b.Personality == WildPersonality.Passive);
        }

        return behavior;
    }

    static WildPersonality ResolvePersonality(Pokemon pokemon, MapArea mapArea)
    {
        // Placeholder logic
        if (pokemon.Base.HasBehaviorType(WildPersonality.Timid))
            return WildPersonality.Timid;

        // if (mapArea.IsDangerous)
        //     return WildPersonality.Aggressive;

        return WildPersonality.Passive;
    }
}

