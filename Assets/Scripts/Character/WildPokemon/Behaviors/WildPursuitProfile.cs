using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pokemon/WildPursuitProfile")]
public class WildPursuitProfile : ScriptableObject
{
    [Header("Perception")]
    public float detectionRadius = 5f;
    public float viewAngle = 120f;      // degrees
    public bool hasOmnidirectionalVision;
    public float reactionDelay = 0.4f;

    [Header("Leash")]
    public float maxChaseDistanceFromSpawn = 6f; // 0 = ignore spawn-point leash
    public float maxChaseDistanceFromTerritory = 0f; // 0 = ignore territory

    [Header("Persistence")]
    public float giveUpTime = 3f; // seconds without progress
    public float reAggroCooldown = 2f;

    [Header("Timid")]
    public float safeDistance = 5f; // distance at which fleeing stops
    public float maxFleeDistance = 6f; // how far it will flee before exhaustion
    public float fleeExhaustionCooldown = 2f; // time to recover before it can flee again

    [Tooltip("Delay after detection before fleeing starts")]
    public float timidReactionDelay = 0.2f;

    [Tooltip("Time it takes to ramp from slow flee to full flee speed")]
    public float fleeAccelerationTime = 0.75f;

    [Header("Movement")]
    public float chaseMoveDelay = 0.25f;
}

public class Territory
{
    public Vector3 center;
    public float radius;

    readonly HashSet<WildPokemonController> members = new();

    public void Register(WildPokemonController c)
    {
        members.Add(c);
    }

    public void Unregister(WildPokemonController c)
    {
        members.Remove(c);
    }

    public void BroadcastThreat(Transform threat)
    {
        foreach (var m in members)
            m.OnTerritoryThreat(threat);
    }

    public bool Contains(Vector3 pos)
    {
        return Vector3.Distance(center, pos) <= radius;
    }
}

#if UNITY_EDITOR

public class TerritoryDebugDrawer : MonoBehaviour
{
    public Territory territory;
    public Color territoryColor = new Color(1f, 0.6f, 0.1f, 0.25f);

    void OnDrawGizmos()
    {
        if (territory == null)
            return;

        Gizmos.color = territoryColor;
        Gizmos.DrawWireSphere(territory.center, territory.radius);
    }
}
// How to use

// Attach this to an empty GameObject

// Assign the same Territory instance used by the spawner

// Toggle visibility via Gizmos checkbox
#endif
