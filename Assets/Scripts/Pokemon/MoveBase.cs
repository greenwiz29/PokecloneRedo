using System.Collections.Generic;
using UnityEngine;

public enum MoveType { Physical, Special, Status }

[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create new move")]
public class MoveBase : ScriptableObject
{
    // New serialized structure.
    [SerializeField] private MoveIdentityData identityData = new();
    [SerializeField] private MoveBattleData battleData = new();
    [SerializeField] private MoveEffectsData effectsData = new();
    [SerializeField] private MoveSpecialMechanicsData specialMechanicsData = new();

    public string Id => identityData.Id;
    public string Name => identityData.Name;
    public string Desc => identityData.Description;

    public PokemonType Type => battleData.Type;
    public int Power => battleData.Power;
    public int Accuracy => battleData.Accuracy;
    public bool AlwaysHits => battleData.AlwaysHits;
    public int PP => battleData.PP;
    public int Priority => battleData.Priority;
    public MoveType MoveType => battleData.MoveType;
    public MoveTarget Target => battleData.Target;

    public MoveEffects Effects => effectsData.Primary;
    public List<SecondaryEffects> SecondaryEffects => effectsData.Secondary;

    public bool IsMultiHitMove => specialMechanicsData.IsMultiHitMove;
    public RecoilMoveEffect Recoil => specialMechanicsData.Recoil;
    public int DrainingPercentage => specialMechanicsData.DrainingPercentage;
    public CritBehavior CriticalBehavior => specialMechanicsData.CriticalBehavior;
    public OneHitKoMoveEffect OneHitKo => specialMechanicsData.OneHitKo;

    public int GetHitTimes()
    {
        return specialMechanicsData.GetHitTimes();
    }

    public bool HasFlag(MoveFlag flag)
    {
        return battleData.HasFlag(flag);
    }
}

// Keep the existing SecondaryEffects, EffectSource, MoveEffects,
// StatBoost, MoveTarget, MoveFlag, RecoilMoveEffect, RecoilType,
// CritBehavior, and OneHitKoMoveEffect declarations below this class
// unchanged.

[System.Serializable]
public class SecondaryEffects : MoveEffects
{
    [SerializeField] int chance;

    [SerializeField] MoveTarget target;

    public int Chance => chance;
    public MoveTarget Target => target;
}

public enum EffectSource { Move, Ability, Item }

[System.Serializable]
public class MoveEffects
{
    [SerializeField] List<StatBoost> boosts;

    [SerializeField] StatusConditionID status;

    [SerializeField] StatusConditionID volatileStatus;
    [Header("Weather")]
    [SerializeField] WeatherConditionID weather;
    [SerializeField] SN<int> weatherDuration = null;

    public List<StatBoost> Boosts => boosts;
    public StatusConditionID Status => status;
    public StatusConditionID VolatileStatus => volatileStatus;
    public WeatherConditionID Weather => weather;
    public int? WeatherDuration => weatherDuration;
}

[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
}

public enum MoveTarget { Foe, Self, Area, Ally, }

public enum MoveFlag { Contact, Punch, Bite, Sound, }

//Erina's tutorials
[System.Serializable]
public class RecoilMoveEffect
{
    public RecoilType recoilType;
    public int recoilDamagePercent = 0;
}

public enum RecoilType { none, RecoilByMaxHP, RecoilByCurrentHP, RecoilByDamage }

public enum CritBehavior { none, HighCritRatio, AlwaysCrits, NeverCrits }

[System.Serializable]
public class OneHitKoMoveEffect
{
    // Bool to turn move into 1-hit KO move
    public bool isOneHitKnockOut;
    // This is an exception that makes the base Accuracy 20 instead of 30 if the pokémons type isn't the same as the moves type.
    public bool lowerOddsException;
    // This can be used to make a target immume if it has a certain type
    public PokemonType immunityType;
}
