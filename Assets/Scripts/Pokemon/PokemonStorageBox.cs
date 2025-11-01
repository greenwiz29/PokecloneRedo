using UnityEngine;

public class PokemonStorageBox : MonoBehaviour
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
}
