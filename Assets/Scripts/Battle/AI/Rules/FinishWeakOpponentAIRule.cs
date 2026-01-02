using UnityEngine;

[CreateAssetMenu(menuName = "Battle/AI Rules/Finish Weak Opponent")]
public class FinishWeakOpponentAIRule : AIRule
{
    [SerializeField] float enemyHpThreshold = 0.25f;
    [SerializeField] int bonus = 30;

    public override int ScoreAction(
        BattleSystem state,
        BattleUnit unit,
        BattleAction action
    )
    {
        if (action.Type != BattleActionType.Move || action.SelectedMove?.Base.Power <= 0)
            return 0;

        if (action.Target.Pokemon.GetNormalizedHp() <= enemyHpThreshold)
            return bonus;

        return 0;
    }
}
