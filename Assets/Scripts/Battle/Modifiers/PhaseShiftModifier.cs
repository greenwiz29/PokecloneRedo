using UnityEngine;

[CreateAssetMenu(menuName = "Battles/Modifiers/Phase Shift")]
public class PhaseShiftModifier : BattleModifier
{
    [Header("Trigger")]
    [SerializeField] BattleTriggerCondition trigger;

    [Header("Effects")]
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
            new DialogBattleEvent(dialog)
        );
    }
}
