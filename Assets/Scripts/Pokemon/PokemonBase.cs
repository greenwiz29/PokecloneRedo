using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new Pokemon")]
public class PokemonBase : ScriptableObject
{
    [SerializeField] new string name;

    [TextArea]
    [SerializeField] string description;

    [Header("Sprites")]
    [Header("Normal")]
    [SerializeField] Sprite frontSprite, backSprite;
    [SerializeField] List<Sprite> walkDownSprites, walkUpSprites, walkLeftSprites, walkRightSprites;

    [Header("Shiny")]
    [SerializeField] Sprite frontSpriteShiny, backSpriteShiny;
    [SerializeField] List<Sprite> walkDownSpritesShiny, walkUpSpritesShiny, walkLeftSpritesShiny, walkRightSpritesShiny;


    [Header("Stats")]
    [SerializeField] PokemonType type1, type2;

    // Base Stats
    [SerializeField] int maxHP, attack, defense, spAttack, spDefense, speed;

    // Hidden Stats
    [SerializeField] int expYield;
    [SerializeField] int catchRate = 255;

    [SerializeField] List<LearnableMove> learnableMoves;
    [SerializeField] List<MoveBase> learnableByItems;

    [SerializeField] List<Evolution> evolutions;
    GrowthRate growthRate;

    public string Name => name;
    public string Desc => description;
    public Sprite FrontSprite => frontSprite;
    public Sprite BackSprite => backSprite;
    public PokemonType Type1 => type1;
    public PokemonType Type2 => type2;
    public int CatchRate => catchRate;
    public int MaxHP => maxHP;
    public int Attack => attack;
    public int Defense => defense;
    public int SpAttack => spAttack;
    public int SpDef => spDefense;
    public int Speed => speed;
    public int ExpYield => expYield;
    public GrowthRate GrowthRate { get => growthRate; set => growthRate = value; }
    public List<LearnableMove> LearnableMoves => learnableMoves;
    public List<MoveBase> LearnableByItems => learnableByItems;
    public List<Evolution> Evolutions => evolutions;

    public int CalculateBaseExpForLevel(int level)
    {
        int exp;
        int n = level; // for ease of typing
        switch (growthRate)
        {
            case GrowthRate.Erratic:
                if (n < 50)
                {
                    exp = n * n * n * (100 - n) / 50;
                }
                else if (n < 68)
                {
                    exp = n * n * n * (150 - n) / 100;
                }
                else if (n < 98)
                {
                    exp = n * n * n * Mathf.FloorToInt((1911 - 10 * n) / 3) / 500;
                }
                else
                {
                    exp = n * n * n * (160 - n) / 100;
                }
                break;
            case GrowthRate.Fast:
                exp = 4 * n * n * n / 5;
                break;
            case GrowthRate.MediumFast:
                exp = n * n * n;
                break;
            case GrowthRate.MediumSlow:
                exp = (int)(1.2f * n * n * n - 15 * n * n + 100 * n - 140);
                break;
            case GrowthRate.Slow:
                exp = 5 * n * n * n / 4;
                break;
            case GrowthRate.Fluctuating:
                if (n < 15)
                {
                    exp = (n * n * n * Mathf.FloorToInt((n + 1) / 3) + 24) / 50;
                }
                else if (n < 36)
                {
                    exp = n * n * n * (n + 14) / 50;
                }
                else
                {
                    exp = n * n * n * (Mathf.FloorToInt(n / 2) + 32) / 50;
                }
                break;
            default:
                exp = 100 * n;
                break;
        }
        return exp;
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

public enum PokemonType
{
    None,
    Normal,
    Fire,
    Water,
    Grass,
    Flying,
    Fighting,
    Poison,
    Electric,
    Ground,
    Rock,
    Psychic,
    Ice,
    Bug,
    Ghost,
    Steel,
    Dragon,
    Dark,
    Fairy,
}

public enum Stat
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed,
    Accuracy,
    Evasion,
}

public enum GrowthRate
{
    Erratic,
    Fast,
    MediumFast,
    MediumSlow,
    Slow,
    Fluctuating,
}

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