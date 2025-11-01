using System;
using System.Collections.Generic;
using UnityEngine;

public class PokemonStorageBox : MonoBehaviour, ISavable
{
    const int maxBoxes = 16;
    const int slotsPerBox = 42;
    Pokemon[,] boxes = new Pokemon[maxBoxes, slotsPerBox];

    public int MaxBoxes => maxBoxes;
    public int SlotsPerBox => slotsPerBox;

    public void AddPokemon(Pokemon pokemon)
    {
        // Find first empty slot
        for (int i = 0; i < maxBoxes; i++)
        {
            for (int j = 0; j < slotsPerBox; j++)
            {
                if (boxes[i, j] == null)
                {
                    boxes[i, j] = pokemon;
                    return;
                }
            }
        }
    }

    public void AddPokemon(Pokemon pokemon, int boxIndex, int slotIndex)
    {
        boxes[boxIndex, slotIndex] = pokemon;
    }

    public void RemovePokemon(int boxIndex, int slotIndex)
    {
        boxes[boxIndex, slotIndex] = null;
    }

    public Pokemon GetPokemon(int boxIndex, int slotIndex)
    {
        return boxes[boxIndex, slotIndex];
    }

    public static PokemonStorageBox GetPlayerStorageBoxes()
    {
        return GameController.I.Player.GetComponent<PokemonStorageBox>();
    }

    public object CaptureState()
    {
        var saveData = new BoxSaveData()
        {
            boxSlots = new List<BoxSlotSaveData>()
        };

        for (int i = 0; i < maxBoxes; i++)
        {
            for (int j = 0; j < slotsPerBox; j++)
            {
                if (boxes[i, j] != null)
                {
                    var boxSlot = new BoxSlotSaveData()
                    {
                        pokemonData = boxes[i, j].GetSaveData(),
                        boxIndex = i,
                        slotIndex = j
                    };
                    saveData.boxSlots.Add(boxSlot);
                }
            }
        }
        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as BoxSaveData;
        // Clear all existing data
        for (int i = 0; i < maxBoxes; i++)
        {
            for (int j = 0; j < slotsPerBox; j++)
            {
                boxes[i, j] = null;
            }
        }

        // Load data
        foreach (var slot in saveData.boxSlots)
        {
            boxes[slot.boxIndex, slot.slotIndex] = new Pokemon(slot.pokemonData);
        }
    }
}

[Serializable]
public class BoxSaveData
{
    public List<BoxSlotSaveData> boxSlots;
}

[Serializable]
public class BoxSlotSaveData
{
    public PokemonSaveData pokemonData;
    public int boxIndex;
    public int slotIndex;
}
