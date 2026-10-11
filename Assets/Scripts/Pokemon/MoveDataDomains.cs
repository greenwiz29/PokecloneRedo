
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MoveIdentityData
{
    [SerializeField] private string id = "";
    [SerializeField] private string displayName = "";

    [TextArea]
    [SerializeField] private string description = "";

    public string Id => id;
    public string Name => displayName;
    public string Description => description;
}

[Serializable]
public class MoveBattleData
{
    [SerializeField] private PokemonType type;
    [SerializeField] private int power;
    [SerializeField] private int accuracy;
    [SerializeField] private int pp;
    [SerializeField] private int priority;
    [SerializeField] private bool alwaysHits;
    [SerializeField] private MoveType moveType;
    [SerializeField] private MoveTarget target;
    [SerializeField] private List<MoveFlag> flags = new();

    public PokemonType Type => type;
    public int Power => power;
    public int Accuracy => accuracy;
    public int PP => pp;
    public int Priority => priority;
    public bool AlwaysHits => alwaysHits;
    public MoveType MoveType => moveType;
    public MoveTarget Target => target;
    public IReadOnlyList<MoveFlag> Flags => flags;

    public bool HasFlag(MoveFlag flag)
    {
        return flags != null && flags.Contains(flag);
    }
}

[Serializable]
public class MoveEffectsData
{
    [SerializeField] private MoveEffects primary = new();
    [SerializeField] private List<SecondaryEffects> secondary = new();

    public MoveEffects Primary => primary;
    public List<SecondaryEffects> Secondary => secondary;
}

[Serializable]
public class MoveSpecialMechanicsData
{
    [SerializeField] private bool isMultiHitMove;
    [SerializeField] private Vector2Int hitRange = new(2, 0);

    [SerializeField] private RecoilMoveEffect recoil = new();
    [SerializeField] private int drainingPercentage;
    [SerializeField] private CritBehavior critBehavior;
    [SerializeField] private OneHitKoMoveEffect oneHitKoMoveEffect = new();

    public bool IsMultiHitMove => isMultiHitMove;
    public RecoilMoveEffect Recoil => recoil;
    public int DrainingPercentage => drainingPercentage;
    public CritBehavior CriticalBehavior => critBehavior;
    public OneHitKoMoveEffect OneHitKo => oneHitKoMoveEffect;

    public int GetHitTimes()
    {
        if (!isMultiHitMove)
            return 1;

        if (hitRange.y == 0)
            return hitRange.x;

        return UnityEngine.Random.Range(hitRange.x, hitRange.y + 1);
    }
}
