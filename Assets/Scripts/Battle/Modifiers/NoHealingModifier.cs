using UnityEngine;

[CreateAssetMenu(menuName = "Battles/Modifiers/No Healing")]
public class NoHealingModifier : BattleModifier
{
    public override void OnBeforeDamage(
        BattleSystem bs,
        BattleUnit attacker,
        BattleUnit defender,
        Move move,
        ref float damageMultiplier
    )
    {
        if (move.Base.DrainingPercentage > 0)
        {
            damageMultiplier = 0f;
        }
    }
}
