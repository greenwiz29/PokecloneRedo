using System.Collections;
using UnityEngine;

public class PassiveBehavior : IWildPokemonBehavior
{
    public WildPersonality Personality {get;} = WildPersonality.Passive;
    public IEnumerator Run(WildPokemonController c)
    {
        while (true)
        {
            if (c.StateNotIdle)
            {
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(c.WanderDelay);

            Vector2 dir = RandomDirection();
            if (c.CanMove(dir))
                yield return c.Move(dir);
        }
    }

    Vector2 RandomDirection()
    {
        Vector2[] dirs =
        {
            Vector2.up, Vector2.down,
            Vector2.left, Vector2.right
        };
        return dirs[UnityEngine.Random.Range(0, dirs.Length)];
    }
}
