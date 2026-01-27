using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Pokemon/Wild Behavior/Timid", fileName = "TimidBehavior")]
public class TimidBehavior : WildPokemonBehavior
{
    public override WildPersonality Personality => WildPersonality.Timid;

    public override IEnumerator Run(WildPokemonController c)
    {
        Transform player = c.CurrentThreat;
        if (player == null)
            yield break;

        if (!c.CanSeeThreat(player))
            yield break;

        // Play alert (!) or similar indicator
        c.PlayAlert();

        // Small lift for visual anticipation
        c.VerticalPresence?.StartTransition(
            c.VerticalPresence.GetDivergenceOffset() * 0.5f,
            0.15f
        );

        // 1️⃣ Hesitation phase
        float timer = 0f;
        while (timer < c.Profile.timidReactionDelay)
        {
            if (c.CurrentThreat == null)
                yield break; // Player already interacted

            timer += Time.deltaTime;
            yield return null;
        }

        // Check exhaustion: skip flee if recently maxed out
        if (c.FleeAccumulatedDistance >= c.Profile.maxFleeDistance &&
            Time.time - c.LastFleeEndTime < c.Profile.fleeExhaustionCooldown)
        {
            yield break;
        }

        // Begin fleeing
        c.EnterFlee();
        yield return Flee(c, player);

        // Record flee exhaustion if max distance reached
        c.LastFleeEndTime = Time.time;

        // Reset to neutral
        c.ExitReactiveMode();
    }

    IEnumerator Flee(WildPokemonController c, Transform player)
    {
        float stuckTimer = 0f;
        float fleeStartTime = Time.time;

        // Reset accumulated distance for this flee
        c.FleeAccumulatedDistance = 0f;

        while (true)
        {
            // Stop if safe distance reached
            float safeDist = c.Profile.safeDistance > 0f
                ? c.Profile.safeDistance
                : c.Profile.detectionRadius * 1.5f;

            if (Vector3.Distance(c.transform.position, player.position) >= safeDist)
                break;

            // Stop if exhausted
            if (c.FleeAccumulatedDistance >= c.Profile.maxFleeDistance)
                break;

            Vector3 nextPos = NextStepAwayFrom(c.transform.position, player.position);
            if (!c.CanChaseTo(nextPos))
                break;

            // 2️⃣ Flee acceleration ramp
            float t = Mathf.Clamp01(
                (Time.time - fleeStartTime) / c.Profile.fleeAccelerationTime
            );

            float moveDelay = Mathf.Lerp(
                c.Profile.chaseMoveDelay * 1.5f, // slow start
                c.Profile.chaseMoveDelay,        // full speed
                t
            );

            // Move one step
            yield return c.Move(DirectionFromPositions(c.transform.position, nextPos));
            c.VerticalPresence?.StartTransition(
                c.VerticalPresence.GetDivergenceOffset(),
                0.25f
            );

            // Track flee distance
            c.FleeAccumulatedDistance += Vector3.Distance(c.transform.position, nextPos);

            // Check if making progress
            bool gainingDistance = !c.IsClosingDistanceTo(player);
            stuckTimer = gainingDistance ? 0f : stuckTimer + moveDelay;

            // Give up if stuck
            if (stuckTimer > c.Profile.giveUpTime)
                break;

            // 3️⃣ Same-tile guarantee: allow at least one attempt to interact
            if (Vector3.Distance(c.transform.position, player.position) < 0.5f)
            {
                yield return c.Interact(player);
                yield break;
            }

            yield return new WaitForSeconds(moveDelay);
        }
    }

    Vector2 DirectionFromPositions(Vector3 from, Vector3 to)
    {
        Vector3 delta = to - from;
        return Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
            ? new Vector2(Mathf.Sign(delta.x), 0f)
            : new Vector2(0f, Mathf.Sign(delta.y));
    }

    Vector3 NextStepAwayFrom(Vector3 from, Vector3 threat)
    {
        Vector3 delta = from - threat;
        return Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
            ? from + new Vector3(Mathf.Sign(delta.x), 0f, 0f)
            : from + new Vector3(0f, Mathf.Sign(delta.y), 0f);
    }

}
