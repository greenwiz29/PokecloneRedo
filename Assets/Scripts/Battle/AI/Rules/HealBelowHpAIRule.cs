using UnityEngine;

[CreateAssetMenu(menuName = "Battle/AI Rules/Heal Below HP")]
public class HealBelowHpAIRule : AIRule
{
    [Range(0f, 1f)]
    [SerializeField] float hpThreshold = 0.3f;

    [SerializeField] int bonus = 100;

    public override int ScoreAction(
        BattleSystem bs,
        BattleUnit unit,
        BattleAction action
    )
    {
        if (action.Type == BattleActionType.Item &&
            action.SelectedItem is RecoveryItem &&
            unit.Pokemon.GetNormalizedHp() <= hpThreshold)
        {
            return bonus;
        }

        return 0;
    }
}
