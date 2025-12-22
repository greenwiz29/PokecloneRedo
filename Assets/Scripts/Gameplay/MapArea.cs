using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<PokemonEncounterRecord> wildPoolGrass, wildPoolInWater;
    [SerializeField] WeatherConditionID weather = WeatherConditionID.none;

    [SerializeField]
    [HideInInspector]
    int totalChance;

    [SerializeField]
    [HideInInspector]
    int totalChanceWater;

    public WeatherConditionID Weather => weather;

    void OnValidate()
    {
        CalculateTotalChance();
    }

    void Start()
    {
        CalculateTotalChance();
    }

    void CalculateTotalChance()
    {
        totalChance = 0;
        foreach (var record in wildPoolGrass)
        {
            record.ChanceLowerInclusive = totalChance;
            totalChance += record.encounterRate;
            record.ChanceUpperExclusive = totalChance;
        }

        totalChanceWater = 0;
        if (wildPoolInWater.Count == 0)
            totalChanceWater = 100;
        else
            foreach (var record in wildPoolInWater)
            {
                record.ChanceLowerInclusive = totalChanceWater;
                totalChanceWater += record.encounterRate;
                record.ChanceUpperExclusive = totalChanceWater;
            }
    }

    public Pokemon GetRandomWildPokemon(BattleTrigger trigger)
    {
        var wildPool = trigger == BattleTrigger.Water ? wildPoolInWater : wildPoolGrass;
        int roll = UnityEngine.Random.Range(0, totalChance + 1);
        var record = wildPool.First(p => roll >= p.ChanceLowerInclusive && roll < p.ChanceUpperExclusive);
        var levelRange = record.levelRange;
        var level = levelRange.y == 0 ? levelRange.x : UnityEngine.Random.Range(levelRange.x, levelRange.y + 1);

        return new Pokemon(record.pokemon, level);
    }
}

[Serializable]
public class PokemonEncounterRecord
{
    public PokemonBase pokemon;
    /// <summary>
    /// Represents a range of valid levels for the encountered Pokemon, both ends inclusive.
    /// <para>If the second value is 0, the level will always be the first value.</para>
    /// </summary>
    public Vector2Int levelRange;
    [Range(1, 100)]
    public int encounterRate;

    public int ChanceLowerInclusive { get; set; }
    public int ChanceUpperExclusive { get; set; }
}
