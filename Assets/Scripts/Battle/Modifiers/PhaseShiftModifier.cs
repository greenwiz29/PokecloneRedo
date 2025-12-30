using UnityEngine;

[CreateAssetMenu(menuName = "Battles/Modifiers/Phase Shift")]
public class PhaseShiftModifier : BattleModifier
{
    [Header("Trigger")]
    [SerializeField] BattleTriggerCondition trigger;

    [Header("Effects")]
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
            new DialogBattleEvent(dialog)
        );
    }
}
