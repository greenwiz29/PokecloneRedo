using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new Pokemon")]
public class PokemonBase : ScriptableObject
{
    [SerializeField] new string name;
    [SerializeField] int dexNumber;

    [TextArea]
    [SerializeField] string description;

    [Header("Sprites")]
    [Header("Normal")]
    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;
    [SerializeField] Sprite frontSpriteFemale;
    [SerializeField] Sprite backSpriteFemale;
    [Header("Overworld")]
    [SerializeField] List<Sprite> walkDownSprites;
    [SerializeField] List<Sprite> walkUpSprites, walkLeftSprites, walkRightSprites;

    [Header("Shiny")]
    [SerializeField] Sprite frontSpriteShiny;
    [SerializeField] Sprite backSpriteShiny;
    [SerializeField] Sprite frontSpriteFemaleShiny;
    [SerializeField] Sprite backSpriteFemaleShiny;
    [Header("Overworld")]
    [SerializeField] List<Sprite> walkDownSpritesShiny;
    [SerializeField] List<Sprite> walkUpSpritesShiny, walkLeftSpritesShiny, walkRightSpritesShiny;

    [Header("Abilities")]
    [SerializeField] AbilityID ability;
    [SerializeField] AbilityID hiddenAbility;
    [SerializeField] int normalAbilityPercent = 70;

    [Header("Stats")]
    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    // Base Stats
    [Header("Visible Stats")]
    [SerializeField] int maxHP;
    [SerializeField] int attack, defense, spAttack, spDefense, speed;

    // Hidden Stats
    [Header("Hidden Stats")]
    [SerializeField] int expYield;
    [SerializeField] int catchRate = 255;
    [SerializeField] PokemonGenderRatio genderRatio;

    [Header("Moves")]
    [SerializeField] List<LearnableMove> learnableMoves;
    [SerializeField] List<MoveBase> learnableByItems;

    [Header("Evolutions")]
    [SerializeField] List<Evolution> evolutions;

    [Header("Overworld Behavior")]
    [SerializeField] List<WildPokemonBehavior> possibleBehaviors;
    [SerializeField] WildPursuitProfile defaultPursuitProfile;
    // [Tooltip("Describes the flying/burrowing behavior of the species. Set to null for species that can do neither.")]
    [SerializeField] VerticalPresenceProfile verticalPresence = null;

    public string Name => name;
    public string Desc => description;
    public PokemonGenderRatio GenderRatio { get { return genderRatio; } }
    public List<WildPokemonBehavior> PossibleBehaviors => possibleBehaviors;
    public WildPursuitProfile DefaultPursuitProfile => defaultPursuitProfile;
    public VerticalPresenceProfile VerticalPresence => verticalPresence;
    public Sprite FrontSprite => frontSprite;
    public Sprite BackSprite => backSprite;
    public Sprite FrontSpriteFemale => frontSpriteFemale;
    public Sprite BackSpriteFemale => backSpriteFemale;
    public List<Sprite> WalkDownAnim => walkDownSprites;
    public List<Sprite> WalkUpAnim => walkUpSprites;
    public List<Sprite> WalkLeftAnim => walkLeftSprites;
    public List<Sprite> WalkRightAnim => walkRightSprites;
    public Sprite FrontSpriteShiny => frontSpriteShiny;
    public Sprite BackSpriteShiny => backSpriteShiny;
    public Sprite FrontSpriteFemaleShiny => frontSpriteFemaleShiny;
    public Sprite BackSpriteFemaleShiny => backSpriteFemaleShiny;
    public List<Sprite> WalkDownAnimShiny => walkDownSpritesShiny;
    public List<Sprite> WalkUpAnimShiny => walkUpSpritesShiny;
    public List<Sprite> WalkLeftAnimShiny => walkLeftSpritesShiny;
    public List<Sprite> WalkRightAnimShiny => walkRightSpritesShiny;
    public PokemonType Type1 => type1;
    public PokemonType Type2 => type2;
    public AbilityID AbilityID => ability;
    public AbilityID HiddenAbilityID => hiddenAbility;
    public int NormalAbilityPercent => normalAbilityPercent;
    public int CatchRate => catchRate;
    public int MaxHP => maxHP;
    public int Attack => attack;
    public int Defense => defense;
    public int SpAttack => spAttack;
    public int SpDef => spDefense;
    public int Speed => speed;
    public int ExpYield => expYield;
    public List<LearnableMove> LearnableMoves => learnableMoves;
    public List<MoveBase> LearnableByItems => learnableByItems;
    public List<Evolution> Evolutions => evolutions;

    public bool HasBehaviorType(WildPersonality wildPersonality)
    {
        return possibleBehaviors.Any(b => b.Personality == wildPersonality);
    }
}

[Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;

    [SerializeField] int levelLearned;

    public MoveBase Base => moveBase;
    public int LevelLearned => levelLearned;
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
    [SerializeField] PokemonBase evolvesInto;
    [SerializeField] int requiredLevel = -1;
    [SerializeField] EvolutionItem requiredItem;

    public PokemonBase EvolvesInto => evolvesInto;
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
