using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<PokemonEncounterRecord> wildPoolGrass, wildPoolInWater;

    [SerializeField]
    [HideInInspector]
    int totalChance;

    [SerializeField]
    [HideInInspector]
    int totalChanceWater;

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
            record.ChanceLower = totalChance;
            totalChance += record.encounterRate;
            record.ChanceUpper = totalChance;
        }

        totalChanceWater = 0;
        if (wildPoolInWater.Count == 0)
            totalChanceWater = 100;
        else
            foreach (var record in wildPoolInWater)
            {
                record.ChanceLower = totalChanceWater;
                totalChanceWater += record.encounterRate;
                record.ChanceUpper = totalChanceWater;
            }
    }

    public Pokemon GetRandomWildPokemon(BattleTrigger trigger)
    {
        var wildPool = trigger == BattleTrigger.Water ? wildPoolInWater : wildPoolGrass;
        int roll = UnityEngine.Random.Range(0, totalChance + 1);
        var record = wildPool.First(p => roll >= p.ChanceLower && roll < p.ChanceUpper);
        var levelRange = record.levelRange;
        var level = levelRange.y == 0 ? levelRange.x : UnityEngine.Random.Range(levelRange.x, levelRange.y + 1);

        return new Pokemon(record.pokemon, level);
    }
}

[Serializable]
public class PokemonEncounterRecord
{
    public PokemonBase pokemon;
    public Vector2Int levelRange;
    public int encounterRate;

    public int ChanceLower { get; set; }
    public int ChanceUpper { get; set; }
}
