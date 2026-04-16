using UnityEngine;

[CreateAssetMenu(menuName = "Pokemon/Wild Behavior/Guard", fileName = "GuardBehavior")]
public class GuardBehavior : PassiveBehavior, ITerritoryReactive
{
    [SerializeField] AggressiveBehavior aggressiveBehavior;

    public void OnTerritoryThreat(WildPokemonController c, Transform threat)
    {
        if (c.CurrentMode != WildPokemonController.WildMode.Neutral || c.IsReacting)
            return;

        if (!c.CanSeeThreat(threat))
            return;

        c.StartReactiveBehavior(aggressiveBehavior);
    }
}
