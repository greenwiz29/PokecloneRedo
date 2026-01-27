using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Pokemon/Wild Behavior/Passive", fileName = "PassiveBehavior")]
public class PassiveBehavior : WildPokemonBehavior
{
    public override WildPersonality Personality {get;} = WildPersonality.Passive;
    public override IEnumerator Run(WildPokemonController c)
    {
        while (true)
        {
            if (c.CurrentMode != WildPokemonController.WildMode.Neutral)
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
