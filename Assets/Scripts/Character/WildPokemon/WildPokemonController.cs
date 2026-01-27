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

    [Header("Reactions")]
    [SerializeField] GameObject alertIndicator; // "!" sprite/anim, disabled by default

    WildPursuitProfile pursuitProfile;

    public Pokemon WildPokemon { get; private set; }
    public bool StateNotIdle => state != NPCState.Idle;
    public float WanderDelay => wanderDelay;
    public float LastAggroTime => lastAggroTime;
    public Territory Territory => territory;
    public WildPursuitProfile Profile => pursuitProfile;
    public VisualAltitudeController VerticalPresence => verticalPresence;

    Vector3 spawnPoint;
    WildPokemonBehavior baselineBehavior;
    TimidBehavior timidBehavior;
    AggressiveBehavior aggressiveBehavior;

    Coroutine baselineBehaviorRoutine;
    Coroutine reactiveRoutine;

    Territory territory;

    float lastAggroTime;
    float fleeAccumulatedDistance;
    float lastFleeEndTime;

    VisualAltitudeController verticalPresence = null;

    public enum WildMode { Neutral, Aggro, Fleeing }
    public Transform CurrentThreat { get; private set; }
    public WildMode CurrentMode { get; private set; } = WildMode.Neutral;
    public float FleeAccumulatedDistance { get => fleeAccumulatedDistance; set => fleeAccumulatedDistance = value; }
    public float LastFleeEndTime { get => lastFleeEndTime; set => lastFleeEndTime = value; }
    public float LastMeasuredPlayerDistance { get; private set; } = float.MaxValue;


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
                character.OffsetY = 0.5f;
            }

        }

        this.spawnPoint = spawnPoint;
        this.territory = territory;
        this.territory?.Register(this);
        character.SetPositionAndSnapToTile(spawnPoint);

        var behavior = WildBehaviorResolver.Resolve(WildPokemon, mapArea, territory);
        pursuitProfile = WildPokemon.Base.DefaultPursuitProfile;

        InitBehaviors(WildPokemon);

        InitializeBehavior(behavior);
    }

    public bool AllowsInteraction()
    {
        return verticalPresence?.AllowsInteraction() ?? true;
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
        verticalPresence.IsMoving = true;

        try
        {
            yield return character.Move(dir, OnMoveOver: null, checkCollisions: true, onProgress: t => verticalPresence.OnMoveProgress(t));
        }
        finally
        {
            verticalPresence.ReanchorBaseHeight();
            verticalPresence.IsMoving = false;
            state = NPCState.Idle;

            // 3️⃣ Same-tile interaction guarantee
            if (CurrentMode == WildMode.Fleeing)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null && Vector3.Distance(transform.position, player.transform.position) < 0.1f)
                {
                    StartCoroutine(Interact(player.transform));
                }
            }
        }
    }

    private void InitBehaviors(Pokemon pokemon)
    {
        var behaviors = pokemon.Base.PossibleBehaviors;

        timidBehavior = behaviors
            .Find(b => b is TimidBehavior) as TimidBehavior;

        aggressiveBehavior = behaviors
            .Find(b => b is AggressiveBehavior) as AggressiveBehavior;
    }

    protected override void Update()
    {
        base.Update();
        verticalPresence?.Tick();

        TryTriggerReactions();
    }

    void TryTriggerReactions()
    {
        if (CurrentMode != WildMode.Neutral)
            return;

        CurrentThreat = DetectPlayer();
        if (CurrentThreat == null)
            return;

        if (timidBehavior != null)
        {
            // Skip fleeing if recently exhausted
            if (Time.time - lastFleeEndTime < Profile.fleeExhaustionCooldown)
            {
                // skip fleeing this frame
                return;
            }

            StartReactiveBehavior(timidBehavior);
            return;
        }

        if (aggressiveBehavior != null)
        {
            StartReactiveBehavior(aggressiveBehavior);
        }
    }

    void InitializeBehavior(WildPokemonBehavior newBehavior)
    {
        baselineBehavior = newBehavior;

        if (baselineBehaviorRoutine != null)
            StopCoroutine(baselineBehaviorRoutine);

        baselineBehaviorRoutine = StartCoroutine(baselineBehavior.Run(this));
    }

    Transform DetectPlayer()
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null)
            return null;

        bool playerDetected = Vector3.Distance(transform.position, player.transform.position) <= Profile.detectionRadius;
        if (playerDetected)
        {
            Territory?.BroadcastThreat(player.transform);
            return player.transform;
        }

        return null;
    }

    public bool CanSeeThreat(Transform threat)
    {
        Vector2 toThreat = threat.position - transform.position;

        if (toThreat.sqrMagnitude > Profile.detectionRadius * Profile.detectionRadius)
            return false;

        if (Profile.hasOmnidirectionalVision)
            return true;

        Vector2 forward = character.Animator.GetFacingDirection(); // normalized grid or move direction
        float angle = Vector2.Angle(forward, toThreat);

        return angle <= Profile.viewAngle * 0.5f;
    }

    public void OnTerritoryThreat(Transform threat)
    {
#if UNITY_EDITOR
        lastThreat = threat;
        lastThreatTime = Time.time;
#endif

        // Forward to current behavior if it cares
        if (baselineBehavior is ITerritoryReactive reactive)
            reactive.OnTerritoryThreat(this, threat);
    }

    public void StartReactiveBehavior(WildPokemonBehavior reactive)
    {
        if (reactiveRoutine != null)
            StopCoroutine(reactiveRoutine);

        reactiveRoutine = StartCoroutine(RunReactive(reactive));
    }

    IEnumerator RunReactive(WildPokemonBehavior reactive)
    {
        yield return reactive.Run(this);
        reactiveRoutine = null;
    }
    public void PlayAlert()
    {
        if (alertIndicator == null)
            return;

        StartCoroutine(PlayAlertRoutine());
    }

    IEnumerator PlayAlertRoutine()
    {
        alertIndicator.SetActive(true);

        // If it's animated, let the animation play
        // If not, this still gives a clean pulse
        float alertDuration = Mathf.Max(0.15f, Profile.timidReactionDelay);
        yield return new WaitForSeconds(alertDuration);

        alertIndicator.SetActive(false);
    }

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
        CurrentThreat = null;
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
        if (baselineBehavior == null) return;

        Handles.Label(
            transform.position + Vector3.up * 1.2f,
            baselineBehavior.GetType().Name
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

public static class WildBehaviorResolver
{
    public static WildPokemonBehavior Resolve(Pokemon pokemon, MapArea mapArea, Territory territory)
    {
        var personality = ResolveBaselineBehavior(pokemon);

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

    static WildPersonality ResolveBaselineBehavior(Pokemon pokemon)
    {
        // What it does when calm
        if (pokemon.Base.HasBehaviorType(WildPersonality.Territorial))
            return WildPersonality.Territorial;

        return WildPersonality.Passive;
    }

}

public enum WildPersonality { Passive, Timid, Aggressive, Territorial }

