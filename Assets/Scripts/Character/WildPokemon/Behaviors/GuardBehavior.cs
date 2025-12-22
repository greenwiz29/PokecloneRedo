using UnityEngine;

public class GuardBehavior : PassiveBehavior, ITerritoryReactive
{
    public void OnTerritoryThreat(WildPokemonController c, Transform threat)
    {
        c.SwitchBehavior(new AggressiveBehavior());
    }
}
