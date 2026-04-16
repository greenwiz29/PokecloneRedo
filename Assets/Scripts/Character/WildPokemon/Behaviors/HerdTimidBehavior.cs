using UnityEngine;

[CreateAssetMenu(menuName = "Pokemon/Wild Behavior/Herd Timid", fileName = "HerdTimidBehavior")]
public class HerdTimidBehavior : TimidBehavior, ITerritoryReactive
{
    public void OnTerritoryThreat(WildPokemonController c, Transform threat)
    {
        if (c.CurrentMode != WildPokemonController.WildMode.Neutral || c.IsReacting)
            return;

        if (!c.CanSeeThreat(threat))
            return;

        c.StartReactiveBehavior(this);
    }
}
