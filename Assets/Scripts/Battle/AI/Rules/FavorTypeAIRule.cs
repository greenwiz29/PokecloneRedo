using UnityEngine;

[CreateAssetMenu(menuName = "Battle/AI Rules/Favor Type")]
public class FavorTypeAIRule : AIRule
{
    [SerializeField] PokemonType favoredType;
    [SerializeField] int bonus = 20;

    public override int ScoreAction(
        BattleSystem bs,
        BattleUnit unit,
        BattleAction action
    )
    {
        if (action.Type == BattleActionType.Move &&
            action.SelectedMove?.Base.Type == favoredType)
        {
            return bonus;
        }

        return 0;
    }
}
