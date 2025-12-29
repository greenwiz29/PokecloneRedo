using UnityEngine;

[CreateAssetMenu(menuName = "Battles/Modifiers/Phase Shift")]
public class PhaseShiftModifier : BattleModifier
{
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

        if (!defender.IsPlayerUnit &&
            defender.Pokemon.HP <= defender.Pokemon.MaxHP / 2)
        {
            triggered = true;
            bs.EnqueueEvent(new DialogBattleEvent("The Gym Leader gets serious!"));
        }
    }
}
