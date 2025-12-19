using System.Collections.Generic;
using UnityEngine;

public enum MoveType { Physical, Special, Status, }

[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] new string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] PokemonType type;

    [SerializeField] int power, accuracy, pp, priority;

    [SerializeField] bool alwaysHits;

    [SerializeField] MoveType moveType;

    [SerializeField] MoveEffects effects;

    [SerializeField] List<SecondaryEffects> secondaryEffects;

    [SerializeField] MoveTarget target;
    [SerializeField] List<MoveFlag> flags;

    [SerializeField] bool isMultiHitMove = false;
    [SerializeField] Vector2Int hitRange = new Vector2Int(2, 0);
    //Erina's tutorials
    [SerializeField] RecoilMoveEffect recoil = new RecoilMoveEffect();
    [SerializeField] int drainingPercentage = 0;
    [SerializeField] CritBehavior critBehavior;
    [SerializeField] OneHitKoMoveEffect oneHitKoMoveEffect = new OneHitKoMoveEffect();

    public string Name => name;
    public string Desc => description;
    public PokemonType Type => type;
    public int Power => power;
    public int Accuracy => accuracy;
    public bool AlwaysHits => alwaysHits;
    public int PP => pp;
    public int Priority => priority;
    public MoveType MoveType => moveType;
    public MoveTarget Target => target;
    public MoveEffects Effects => effects;
    public List<SecondaryEffects> SecondaryEffects => secondaryEffects;
    public bool IsMultiHitMove => isMultiHitMove;

    //Erina's tutorials
    public RecoilMoveEffect Recoil => recoil;
    public int DrainingPercentage => drainingPercentage;
    public CritBehavior CriticalBehavior => critBehavior;
    public OneHitKoMoveEffect OneHitKo => oneHitKoMoveEffect;

    public int GetHitTimes()
    {
        if (isMultiHitMove)
        {
            if (hitRange.y == 0)
                return hitRange.x;
            else
            {
                return Random.Range(hitRange.x, hitRange.y + 1);
            }
        }
        else
            return 1;
    }

    public bool HasFlag(MoveFlag flag)
    {
        return flags.Contains(flag);
    }
}

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
