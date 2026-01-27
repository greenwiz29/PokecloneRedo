using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Pokemon/Wild Behavior/Aggressive", fileName = "AggressiveBehavior")]
public class AggressiveBehavior : WildPokemonBehavior
{
    public override WildPersonality Personality => WildPersonality.Aggressive;

    public override IEnumerator Run(WildPokemonController c)
    {
        if (!CanAggro(c))
            yield break;

        Transform player = c.CurrentThreat;
        if (player == null)
            yield break;

        if (!c.CanSeeThreat(player))
            yield break;

        c.EnterAggro();
        yield return Chase(c, player);
    }

    bool CanAggro(WildPokemonController c)
    {
        return Time.time - c.LastAggroTime >= c.Profile.reAggroCooldown;
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
            c.VerticalPresence?.StartTransition(0f, 0.25f);

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

        c.ExitReactiveMode();
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
