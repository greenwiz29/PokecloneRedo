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

    [SerializeField] float critChance = 6.25f;

    [SerializeField] bool alwaysHits;

    [SerializeField] MoveType moveType;

    [SerializeField] MoveEffects effects;

    [SerializeField] List<SecondaryEffects> secondaryEffects;

    [SerializeField] MoveTarget target;

    [SerializeField] bool isMultiHitMove = false;
    [SerializeField] Vector2Int hitRange = new Vector2Int(2, 0);
    //Erina's tutorial
    [SerializeField] RecoilMoveEffect recoil = new RecoilMoveEffect();


    public string Name => name;
    public string Desc => description;
    public PokemonType Type => type;
    public int Power => power;
    public int Accuracy => accuracy;
    public bool AlwaysHits => alwaysHits;
    public int PP => pp;
    public int Priority => priority;
    public float CritChance => critChance;
    public MoveType MoveType => moveType;
    public MoveTarget Target => target;
    public MoveEffects Effects => effects;
    public List<SecondaryEffects> SecondaryEffects => secondaryEffects;
    public bool IsMultiHitMove => isMultiHitMove;

    //Erina's tutorial
    public RecoilMoveEffect Recoil => recoil;

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
}

[System.Serializable]
public class SecondaryEffects : MoveEffects
{
    [SerializeField] int chance;

    [SerializeField] MoveTarget target;

    public int Chance => chance;
    public MoveTarget Target => target;
}

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

//Erina's tutorial
[System.Serializable]
public class RecoilMoveEffect
{
    public RecoilType recoilType;
    public int recoilDamagePercent = 0;
}

//Erina's tutorial
public enum RecoilType
{
    none, RecoilByMaxHP, RecoilByCurrentHP, RecoilByDamage
}