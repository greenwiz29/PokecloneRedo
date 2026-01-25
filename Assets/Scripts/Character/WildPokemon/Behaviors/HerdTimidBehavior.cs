using UnityEngine;

public class HerdTimidBehavior : TimidBehavior, ITerritoryReactive
{
    public void OnTerritoryThreat(WildPokemonController c, Transform threat)
    {
        // Immediately flee when a neighbor detects danger
        c.SwitchBehavior(this);
        c.EnterFlee();
    }
}
