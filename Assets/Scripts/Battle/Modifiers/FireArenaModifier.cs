using UnityEngine;

[CreateAssetMenu(menuName = "Battles/Modifiers/Fire Gym Arena")]
public class FireGymArenaModifier : BattleModifier
{
    public override void OnBattleStart(BattleSystem bs, BattleContext ctx)
    {
        bs.EnqueueEvent(
            new DialogBattleEvent("The arena radiates intense heat!")
        );
    }

    public override void OnBeforeDamage(
        BattleSystem bs,
        BattleUnit attacker,
        BattleUnit defender,
        Move move,
        ref float damageMultiplier,
        BattleContext ctx
    )
    {
        if (move.Base.Type == PokemonType.Water)
            damageMultiplier *= 0.7f;
        if (move.Base.Type == PokemonType.Fire)
            damageMultiplier *= 1.2f;
    }
}
