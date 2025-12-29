using UnityEngine;

[CreateAssetMenu(menuName = "Battles/Trainer Battle Profile")]
public class TrainerBattleProfile : ScriptableObject
{
    [Header("Core Rules")]
    public int unitCount = 1;

    public bool allowItems = false;
    public bool allowSwitching = true;
    public bool allowRunning = false;

    [Header("Battlefield")]
    public WeatherConditionID forcedWeather;

    [Header("Modifiers")]
    public BattleModifier[] modifiers;

    [Header("Presentation")]
    public string introDialogOverride;
    public string defeatDialogOverride;
}

public abstract class BattleModifier : ScriptableObject
{
    // Called once when battle starts
    public virtual void OnBattleStart(BattleSystem bs) { }

    // Called at start of each turn (before actions resolve)
    public virtual void OnTurnStart(BattleSystem bs) { }

    // Called before damage is applied
    public virtual void OnBeforeDamage(
        BattleSystem bs,
        BattleUnit attacker,
        BattleUnit defender,
        Move move,
        ref float damageMultiplier
    )
    { }

    // Called after damage is applied
    public virtual void OnAfterDamage(
        BattleSystem bs,
        BattleUnit attacker,
        BattleUnit defender,
        Move move,
        DamageDetails details
    )
    { }

    // Called at end of turn (after weather, status)
    public virtual void OnTurnEnd(BattleSystem bs) { }

    // Called once when battle ends
    public virtual void OnBattleEnd(BattleSystem bs, bool playerWon) { }
}
