using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new Pokemon")]
public class PokemonSpecies : ScriptableObject
{
    [SerializeField] private PokemonIdentityData identity = new();
    [SerializeField] private PokemonBattleData battle = new();
    [SerializeField] private PokemonLearnsetData learnset = new();
    [SerializeField] private PokemonEvolutionData evolution = new();
    [SerializeField] private PokemonVisualData visuals = new();
    [SerializeField] private PokemonOverworldData overworld = new();

    public PokemonIdentityData Identity => identity;
    public PokemonBattleData Battle => battle;
    public PokemonLearnsetData Learnset => learnset;
    public PokemonEvolutionData Evolution => evolution;
    public PokemonVisualData Visuals => visuals;
    public PokemonOverworldData Overworld => overworld;
    public string Name => identity.Name;
    public int DexNumber => identity.DexNumber;
    public string Desc => identity.Description;

    public PokemonType Type1 => battle.Type1;
    public PokemonType Type2 => battle.Type2;

    public int MaxHP => battle.MaxHP;
    public int Attack => battle.Attack;
    public int Defense => battle.Defense;
    public int SpAttack => battle.SpAttack;
    public int SpDef => battle.SpDefense;
    public int Speed => battle.Speed;

    public AbilityID AbilityID => battle.AbilityID;
    public AbilityID HiddenAbilityID => battle.HiddenAbilityID;
    public int NormalAbilityPercent => battle.NormalAbilityPercent;

    public int CatchRate => battle.CatchRate;
    public int ExpYield => battle.ExpYield;
    public PokemonGenderRatio GenderRatio => battle.GenderRatio;

    public IReadOnlyList<LearnableMove> LearnableMoves => learnset.LevelUpMoves;
    public IReadOnlyList<MoveBase> LearnableByItems => learnset.ItemMoves;

    public IReadOnlyList<Evolution> Evolutions => evolution.Evolutions;

    public PokemonBattleSprites BattleSprites => visuals.Battle;
    public PokemonOverworldSprites OverworldSprites => visuals.Overworld;

    public List<WildPokemonBehavior> PossibleBehaviors => overworld.PossibleBehaviors;
    public WildPursuitProfile DefaultPursuitProfile => overworld.DefaultPursuitProfile;
    public VerticalPresenceProfile VerticalPresence => overworld.VerticalPresence;

    public bool HasBehaviorType(WildPersonality personality)
    {
        return PossibleBehaviors.Exists(
            behavior => behavior.Personality == personality);
    }

#if UNITY_EDITOR
    public void SetLevelUpMoves(List<LearnableMove> moves)
    {
        learnset.LevelUpMoves.Clear();
        learnset.LevelUpMoves.AddRange(moves);
    }

    public void SetItemMoves(List<MoveBase> moves)
    {
        learnset.ItemMoves.Clear();
        learnset.ItemMoves.AddRange(moves);
    }
#endif
}

[Serializable]
public class LearnableMove
{
    [SerializeField] private MoveBase moveBase;

    [SerializeField] private int levelLearned;

    public MoveBase Base => moveBase;
    public int LevelLearned => levelLearned;

#if UNITY_EDITOR
    public LearnableMove(MoveBase moveBase, int levelLearned)
    {
        this.moveBase = moveBase;
        this.levelLearned = levelLearned;
    }
#endif
}

[Serializable]
public class PokemonLearnsetData
{
    [SerializeField] private List<LearnableMove> levelUpMoves = new();
    [SerializeField] private List<MoveBase> itemMoves = new();

    public List<LearnableMove> LevelUpMoves => levelUpMoves;
    public List<MoveBase> ItemMoves => itemMoves;
}

public enum PokemonType { None, Normal, Fire, Water, Grass, Flying, Fighting, Poison, Electric, Ground, Rock, Psychic, Ice, Bug, Ghost, Steel, Dragon, Dark, Fairy, }

public enum Stat { Attack, Defense, SpAttack, SpDefense, Speed, Accuracy, Evasion, HP }

public enum GrowthRate { Erratic, Fast, MediumFast, MediumSlow, Slow, Fluctuating, }

public class TypeChart
{
    static float[][] chart =
    {
        //Has to be same order as PokemonType class
        //                   Nor   Fir   Wat   Gra   Fly   Fig   Poi   Ele   Gro   Roc   Psy   Ice   Bug   Gho   Ste    Dra   Dar   Fai
        /*NOR*/ new float[] {1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f, 0.0f, 0.5f, 1.0f, 1.0f, 1.0f},
        /*FIR*/ new float[] {1.0f, 0.5f, 0.5f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 2.0f, 2.0f, 1.0f, 2.0f, 0.5f, 1.0f, 1.0f},
        /*WAT*/ new float[] {1.0f, 2.0f, 0.5f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 2.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f},
        /*GRA*/ new float[] {1.0f, 0.5f, 2.0f, 0.5f, 0.5f, 1.0f, 0.5f, 1.0f, 2.0f, 2.0f, 1.0f, 1.0f, 0.5f, 1.0f, 0.5f, 0.5f, 1.0f, 1.0f},
        /*Fly*/ new float[] {1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 2.0f, 1.0f, 0.5f, 1.0f, 0.5f, 1.0f, 1.0f, 2.0f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f},
        /*FIG*/ new float[] {2.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 0.5f, 1.0f, 1.0f, 2.0f, 0.5f, 2.0f, 0.5f, 0.0f, 2.0f, 1.0f, 2.0f, 0.5f},
        /*Poi*/ new float[] {1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 0.5f, 1.0f, 0.5f, 0.5f, 1.0f, 1.0f, 1.0f, 0.5f, 0.0f, 1.0f, 1.0f, 2.0f},
        /*ELE*/ new float[] {1.0f, 1.0f, 2.0f, 0.5f, 2.0f, 1.0f, 1.0f, 0.5f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f},
        /*Gro*/ new float[] {1.0f, 2.0f, 1.0f, 0.5f, 0.0f, 1.0f, 2.0f, 2.0f, 1.0f, 2.0f, 1.0f, 1.0f, 0.5f, 1.0f, 2.0f, 1.0f, 1.0f, 1.0f},
        /*Rck*/ new float[] {1.0f, 2.0f, 1.0f, 1.0f, 2.0f, 0.5f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 2.0f, 2.0f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f},
        /*Psy*/ new float[] {1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 2.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 0.0f, 1.0f},
        /*ICE*/ new float[] {1.0f, 0.5f, 0.5f, 2.0f, 2.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 0.5f, 2.0f, 1.0f, 1.0f},
        /*Bug*/ new float[] {1.0f, 0.5f, 1.0f, 2.0f, 0.5f, 0.5f, 0.5f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 0.5f, 0.5f, 1.0f, 2.0f, 0.5f},
        /*Gho*/ new float[] {0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 0.5f, 1.0f},
        /*Ste*/ new float[] {1.0f, 0.5f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 0.5f, 1.0f, 2.0f, 2.0f, 1.0f, 0.5f, 1.0f, 1.0f, 2.0f},
        /*Dra*/ new float[] {1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 2.0f, 1.0f, 0.0f},
        /*Drk*/ new float[] {1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 2.0f, 1.0f, 1.0f, 0.5f, 0.5f},
        /*Fai*/ new float[] {1.0f, 0.5f, 1.0f, 1.0f, 1.0f, 2.0f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 2.0f, 2.0f, 1.0f},
    };

    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        if (attackType == PokemonType.None || defenseType == PokemonType.None)
            return 1f;

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        float mod = chart[row][col];
        return mod;
    }
}

[Serializable]
public class Evolution
{
    [SerializeField] private PokemonSpecies evolvesInto;
    [SerializeField] private int requiredLevel = -1;
    [SerializeField] private EvolutionItem requiredItem;

    public PokemonSpecies EvolvesInto => evolvesInto;
    public int RequiredLevel => requiredLevel;
    public EvolutionItem RequiredItem => requiredItem;
}

public enum PokemonGenderRatio
{
    OneInTwoFemale, OneInFourFemale, OneInEightFemale, ThreeInFourFemale, SevenInEightFemale, FemaleOnly, MaleOnly, Genderless, Ditto
}

public enum PokemonGender
{
    NotSet, Female, Male, Genderless, Ditto
}

[Serializable]
public class VerticalPresenceProfile
{
    public bool canHover;
    public bool canFly;
    public bool canBurrow;
    public bool usesGroundFooting = true;

    [Tooltip("Visual offset range in Unity units")]
    public Vector2 offsetRange; // e.g. (-0.4, 0.8)

    public NeutralAltitudeMode neutralMode = NeutralAltitudeMode.Continuous;

    // For Continuous
    public float neutralBobSpeed = 2.0f;

    // For PerMoveCycle
    public AnimationCurve perMoveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("Player can interact only if abs(offset) <= this")]
    public float interactionTolerance = 0.2f;

    [Tooltip("Seconds between altitude changes")]
    public Vector2 changeIntervalRange = new(1.5f, 3.5f);

    [Tooltip("Indicates the preferred direction to escape")]
    public DivergencePreference divergencePreference;

    [Header("Fly Away")]
    public bool canFlyAway = false;

    [Tooltip("How close to max altitude counts as 'at ceiling'")]
    public float flyAwayAltitudeTolerance = 0.1f;

    [Tooltip("How long the Pokémon must remain at max altitude to fly away")]
    public float flyAwayHoldTime = 1.5f;

}

public enum NeutralAltitudeMode
{
    Continuous,   // butterflies, birds
    PerMoveCycle  // hopping, bobbing per tile
}

[Serializable]
public class PokemonIdentityData
{
    [SerializeField] private string name;
    [SerializeField] private int dexNumber = -1;

    [TextArea]
    [SerializeField] private string description;

    public string Name => name;
    public int DexNumber => dexNumber;
    public string Description => description;
}

[Serializable]
public class PokemonBattleData
{
    [Header("Types")]
    [SerializeField] private PokemonType type1;
    [SerializeField] private PokemonType type2;

    [Header("Stats")]
    [SerializeField] private PokemonBaseStats baseStats = new();

    [Header("Abilities")]
    [SerializeField] private AbilityID ability;
    [SerializeField] private AbilityID hiddenAbility;
    [SerializeField, Range(0, 100)]
    int normalAbilityPercent = 70;

    [Header("Other")]
    [SerializeField] private int expYield;
    [SerializeField] private int catchRate = 255;
    [SerializeField] private PokemonGenderRatio genderRatio;

    public PokemonType Type1 => type1;
    public PokemonType Type2 => type2;

    public int MaxHP => baseStats.MaxHP;
    public int Attack => baseStats.Attack;
    public int Defense => baseStats.Defense;
    public int SpAttack => baseStats.SpAttack;
    public int SpDefense => baseStats.SpDefense;
    public int Speed => baseStats.Speed;

    public AbilityID AbilityID => ability;
    public AbilityID HiddenAbilityID => hiddenAbility;
    public int NormalAbilityPercent => normalAbilityPercent;

    public int ExpYield => expYield;
    public int CatchRate => catchRate;
    public PokemonGenderRatio GenderRatio => genderRatio;
}

[Serializable]
public class PokemonBaseStats
{
    [SerializeField] private int maxHP;
    [SerializeField] private int attack;
    [SerializeField] private int defense;
    [SerializeField] private int spAttack;
    [SerializeField] private int spDefense;
    [SerializeField] private int speed;

    public int MaxHP => maxHP;
    public int Attack => attack;
    public int Defense => defense;
    public int SpAttack => spAttack;
    public int SpDefense => spDefense;
    public int Speed => speed;
}

[Serializable]
public class PokemonEvolutionData
{
    [SerializeField] private List<Evolution> evolutions = new();

    public List<Evolution> Evolutions => evolutions;
}

[Serializable]
public class PokemonVisualData
{
    [Header("Battle")]
    [SerializeField] private PokemonBattleSprites battle = new();

    [Header("Overworld")]
    [SerializeField] private PokemonOverworldSprites overworld = new();

    public PokemonBattleSprites Battle => battle;
    public PokemonOverworldSprites Overworld => overworld;
}

[Serializable]
public class PokemonBattleSprites
{
    [Header("Normal")]
    [SerializeField] private Sprite front;
    [SerializeField] private Sprite back;
    [SerializeField] private Sprite frontFemale;
    [SerializeField] private Sprite backFemale;

    [Header("Shiny")]
    [SerializeField] private Sprite frontShiny;
    [SerializeField] private Sprite backShiny;
    [SerializeField] private Sprite frontFemaleShiny;
    [SerializeField] private Sprite backFemaleShiny;

    public Sprite Front => front;
    public Sprite Back => back;
    public Sprite FrontFemale => frontFemale;
    public Sprite BackFemale => backFemale;

    public Sprite FrontShiny => frontShiny;
    public Sprite BackShiny => backShiny;
    public Sprite FrontFemaleShiny => frontFemaleShiny;
    public Sprite BackFemaleShiny => backFemaleShiny;
}

[Serializable]
public class PokemonOverworldSprites
{
    [Header("Normal")]
    [SerializeField] private List<Sprite> walkDown = new();
    [SerializeField] private List<Sprite> walkUp = new();
    [SerializeField] private List<Sprite> walkLeft = new();
    [SerializeField] private List<Sprite> walkRight = new();

    [Header("Shiny")]
    [SerializeField] private List<Sprite> walkDownShiny = new();
    [SerializeField] private List<Sprite> walkUpShiny = new();
    [SerializeField] private List<Sprite> walkLeftShiny = new();
    [SerializeField] private List<Sprite> walkRightShiny = new();

    public List<Sprite> WalkDown => walkDown;
    public List<Sprite> WalkUp => walkUp;
    public List<Sprite> WalkLeft => walkLeft;
    public List<Sprite> WalkRight => walkRight;

    public List<Sprite> WalkDownShiny => walkDownShiny;
    public List<Sprite> WalkUpShiny => walkUpShiny;
    public List<Sprite> WalkLeftShiny => walkLeftShiny;
    public List<Sprite> WalkRightShiny => walkRightShiny;
}

[Serializable]
public class PokemonOverworldData
{
    [Header("Behavior")]
    [SerializeField] private List<WildPokemonBehavior> possibleBehaviors = new();
    [SerializeField] private WildPursuitProfile defaultPursuitProfile;

    [Header("Vertical Presence")]
    [SerializeField] private VerticalPresenceProfile verticalPresence;

    public List<WildPokemonBehavior> PossibleBehaviors => possibleBehaviors;
    public WildPursuitProfile DefaultPursuitProfile => defaultPursuitProfile;
    public VerticalPresenceProfile VerticalPresence => verticalPresence;
}