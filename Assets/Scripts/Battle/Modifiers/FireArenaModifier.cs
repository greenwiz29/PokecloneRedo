using UnityEngine;

[CreateAssetMenu(menuName = "Battles/Modifiers/Fire Arena")]
public class FireArenaModifier : BattleModifier
{
    public override void OnBattleStart(BattleSystem bs)
    {
        bs.DialogBox.TypeDialog("The arena radiates intense heat!");
    }

    public override void OnBeforeDamage(
        BattleSystem bs,
        BattleUnit attacker,
        BattleUnit defender,
        Move move,
        ref float damageMultiplier
    )
    {
        if (move.Base.Type == PokemonType.Water)
            damageMultiplier *= 0.7f;

        if (move.Base.Type == PokemonType.Fire)
            damageMultiplier *= 1.2f;
    }
}
