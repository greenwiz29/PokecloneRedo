using UnityEngine;

[CreateAssetMenu(menuName = "Battle/AI Rules/Favor Terrain Move")]
public class FavorTerrainMoveAIRule : AIRule
{
    [SerializeField] string terrainModifierId;
    [SerializeField] PokemonType boostedType;
    [SerializeField] int bonus = 40;

    public override int ScoreAction(
        BattleSystem battle,
        BattleUnit enemy,
        BattleAction action)
    {
        if (!battle.HasModifier(terrainModifierId))
            return 0;

        if (action.SelectedMove?.Base.Type == boostedType)
            return bonus;

        return 0;
    }
}
