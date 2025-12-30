using UnityEngine;

[CreateAssetMenu(menuName = "Battles/Modifiers/Forced Evolution Trigger")]
public class ForcedEvolutionTriggerModifier : BattleModifier
{
    [Header("Trigger")]
    [SerializeField] BattleTriggerCondition trigger;

    [Header("Evolution")]
    [SerializeField] Evolution forcedEvolution;

    [TextArea]
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
        if (triggered && trigger.triggerOnce)
            return;

        if (!trigger.Check(defender))
            return;

        triggered = true;

        bs.EnqueueEvent(
            new EvolutionRequestEvent(
                new EvolutionRequest
                {
                    Unit = defender,
                    ForcedEvolution = forcedEvolution,
                    Reason = dialog
                }
            )
        );
    }
}
