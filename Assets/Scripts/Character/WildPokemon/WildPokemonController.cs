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

    WildPursuitProfile pursuitProfile;

    public Pokemon WildPokemon { get; private set; }
    public bool StateNotIdle => state != NPCState.Idle;
    public float WanderDelay => wanderDelay;
    public float LastAggroTime => lastAggroTime;
    public Territory Territory => territory;
    public WildPursuitProfile Profile => pursuitProfile;

    Vector3 spawnPoint;
    IWildPokemonBehavior behavior;
    Coroutine behaviorRoutine;
    Territory territory;

    float lastAggroTime;

    public enum WildMode { Neutral, Aggro, Fleeing }

    public WildMode CurrentMode { get; private set; } = WildMode.Neutral;


    protected override void Awake()
    {
        base.Awake();
        spawnPoint = transform.position;
    }

    protected override IEnumerator OnInteract(Transform initiator)
    {
        var player = initiator.GetComponent<PlayerController>();

        // No facing required
        GameController.I.StartOverworldPokemonBattle(this);
        yield break;
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

        this.spawnPoint = spawnPoint;
        this.territory = territory;
        this.territory?.Register(this);

        var behavior = WildBehaviorResolver.Resolve(WildPokemon, mapArea, territory);
        pursuitProfile = WildPokemon.Base.DefaultPursuitProfile;

        InitializeBehavior(behavior);
    }

    public void InitializeBehavior(IWildPokemonBehavior newBehavior)
    {
        behavior = newBehavior;

        if (behaviorRoutine != null)
            StopCoroutine(behaviorRoutine);

        behaviorRoutine = StartCoroutine(behavior.Run(this));
    }

    public void SwitchBehavior(IWildPokemonBehavior newBehavior)
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

    public void RecordAggroStart()
    {
        ResetChaseMetrics();
    }

    public void RecordAggroEnd()
    {
        lastAggroTime = Time.time;
        ResetChaseMetrics();
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
    public static IWildPokemonBehavior Resolve(
        Pokemon pokemon,
        MapArea mapArea,
        Territory territory
    )
    {
        var personality = ResolvePersonality(pokemon, mapArea);

        return personality switch
        {
            WildPersonality.Timid => new TimidBehavior(),
            WildPersonality.Aggressive => new AggressiveBehavior(),
            _ => new PassiveBehavior()
        };
    }

    static WildPersonality ResolvePersonality(Pokemon pokemon, MapArea mapArea)
    {
        // Placeholder logic
        // if (pokemon.Base.HasBehaviorType(WildPersonality.Timid))
        //     return WildPersonality.Timid;

        // if (mapArea.IsDangerous)
        //     return WildPersonality.Aggressive;

        return WildPersonality.Passive;
    }
}

