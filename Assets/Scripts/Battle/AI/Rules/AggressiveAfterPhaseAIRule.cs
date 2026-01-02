using UnityEngine;

[CreateAssetMenu(menuName = "Battle/AI Rules/Aggressive After Phase")]
public class AggressiveAfterPhaseRule : AIRule
{
    [SerializeField] string phaseModifierId;
    [SerializeField] int bonus = 30;

    public override int ScoreAction(
        BattleSystem battle,
        BattleUnit enemy,
        BattleAction action)
    {
        if (!battle.HasModifier(phaseModifierId))
            return 0;

        if (action.SelectedMove?.Base.Power > 0)
            return bonus;

        return 0;
    }
}
