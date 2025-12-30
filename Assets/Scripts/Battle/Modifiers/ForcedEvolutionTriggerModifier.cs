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

    public override void OnAfterDamage(
        BattleSystem bs,
        BattleUnit attacker,
        BattleUnit defender,
        Move move,
        DamageDetails details,
        BattleContext ctx
    )
    {
        if (trigger.triggerOnce && ctx.ModifierState.HasTriggered(this))
            return;

        if (!trigger.Check(defender))
            return;

        ctx.ModifierState.MarkTriggered(this);

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
