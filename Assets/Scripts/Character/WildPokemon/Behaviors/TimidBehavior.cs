using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Pokemon/Wild Behavior/Timid", fileName = "TimidBehavior")]
public class TimidBehavior : WildPokemonBehavior
{
    public override WildPersonality Personality => WildPersonality.Timid;


    public override IEnumerator Run(WildPokemonController c)
    {
        while (true)
        {
            Transform player = DetectPlayer(c);
            if (player == null)
            {
                yield return null;
                continue;
            }

            c.EnterFlee(); // reuse metric reset
            yield return Flee(c, player);
        }
    }

    Transform DetectPlayer(WildPokemonController c)
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null)
            return null;

        return Vector3.Distance(c.transform.position, player.transform.position)
            <= c.Profile.detectionRadius ? player.transform : null;
    }

    IEnumerator Flee(WildPokemonController c, Transform player)
    {
        float stuckTimer = 0f;

        while (true)
        {
            // Stop if safe
            float safeDist = c.Profile.safeDistance > 0f ? c.Profile.safeDistance : c.Profile.detectionRadius * 1.5f;

            if (Vector3.Distance(c.transform.position, player.position) >= safeDist)
                yield break;

            Vector3 nextPos = NextStepAwayFrom(c.transform.position, player.position);

            // Leash rules still apply (territorial timid Pokémon exist!)
            if (!c.CanChaseTo(nextPos))
                break;

            yield return c.Move(DirectionFromPositions(c.transform.position, nextPos));

            bool gainingDistance = !c.IsClosingDistanceTo(player);

            stuckTimer = gainingDistance ? 0f : stuckTimer + c.Profile.chaseMoveDelay;

            if (stuckTimer > c.Profile.giveUpTime)
                break;

            yield return new WaitForSeconds(c.Profile.chaseMoveDelay);
        }

        c.ExitReactiveMode();
    }

    Vector2 DirectionFromPositions(Vector3 from, Vector3 to)
    {
        Vector3 delta = to - from;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return new Vector2(Mathf.Sign(delta.x), 0f);
        else
            return new Vector2(0f, Mathf.Sign(delta.y));
    }

    Vector3 NextStepAwayFrom(Vector3 from, Vector3 threat)
    {
        Vector3 delta = from - threat;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return from + new Vector3(Mathf.Sign(delta.x), 0f, 0f);
        else
            return from + new Vector3(0f, Mathf.Sign(delta.y), 0f);
    }
}
