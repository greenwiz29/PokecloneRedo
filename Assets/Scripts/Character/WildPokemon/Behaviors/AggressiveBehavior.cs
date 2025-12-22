using System.Collections;
using UnityEngine;

public class AggressiveBehavior : IWildPokemonBehavior
{
    public WildPersonality Personality => WildPersonality.Aggressive;

    public IEnumerator Run(WildPokemonController c)
    {
        while (true)
        {
            if (!CanAggro(c))
            {
                yield return null;
                continue;
            }

            Transform player = DetectPlayer(c);
            if (player != null)
            {
                c.RecordAggroStart();
                yield return Chase(c, player);
            }
            else
            {
                yield return null;
            }
        }
    }

    bool CanAggro(WildPokemonController c)
    {
        return Time.time - c.LastAggroTime >= c.Profile.reAggroCooldown;
    }

    Transform DetectPlayer(WildPokemonController c)
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null)
            return null;

        bool playerDetected = Vector3.Distance(c.transform.position, player.transform.position) <= c.Profile.detectionRadius;
        if (playerDetected)
        {
            c.Territory?.BroadcastThreat(player.transform);
            return player.transform;
        }

        return null;
    }

    IEnumerator Chase(WildPokemonController c, Transform player)
    {
        float giveUpTimer = 0f;

        while (true)
        {
            if (c.StateNotIdle)
                yield break;

            Vector3 nextPos = NextStepTowards(c.transform.position, player.position);

            if (!c.CanChaseTo(nextPos))
                break;

            yield return c.Move(DirectionFromPositions(c.transform.position, nextPos));

            giveUpTimer = PlayerDistanceDecreasing(c, player)
                ? 0f
                : giveUpTimer + c.Profile.chaseMoveDelay;

            if (giveUpTimer > c.Profile.giveUpTime)
                break;

            if (Vector3.Distance(c.transform.position, player.position) < 0.5f)
            {
                yield return c.Interact(player);
                yield break;
            }

            yield return new WaitForSeconds(c.Profile.chaseMoveDelay);
        }

        c.RecordAggroEnd();
    }

    Vector3 NextStepTowards(Vector3 from, Vector3 to)
    {
        Vector3 delta = to - from;

        // Prefer axis with larger distance (Manhattan pursuit)
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            return from + new Vector3(Mathf.Sign(delta.x), 0f, 0f);
        }
        else
        {
            return from + new Vector3(0f, Mathf.Sign(delta.y), 0f);
        }
    }

    Vector2 DirectionFromPositions(Vector3 from, Vector3 to)
    {
        Vector3 delta = to - from;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return new Vector2(Mathf.Sign(delta.x), 0f);
        else
            return new Vector2(0f, Mathf.Sign(delta.y));
    }

    bool PlayerDistanceDecreasing(WildPokemonController c, Transform player)
    {
        return c.IsClosingDistanceTo(player);
    }
}
