using UnityEngine;

[CreateAssetMenu(menuName = "Battles/Modifiers/Forced Evolution Trigger")]
public class ForcedEvolutionTriggerModifier : BattleModifier
{
    [SerializeField] Evolution evolution;
    [SerializeField] string dialog;

    bool triggered = false;

    public override void OnAfterDamage(
        BattleSystem bs,
        BattleUnit attacker,
        BattleUnit defender,
        Move move,
        DamageDetails details
    )
    {
        if (triggered) return;
        if (!defender.IsPlayerUnit) return;

        triggered = true;

        bs.EnqueueEvent(
            new EvolutionRequestEvent(
                new EvolutionRequest
                {
                    Unit = defender,
                    ForcedEvolution = evolution,
                    Reason = dialog
                }
            )
        );
    }
}
