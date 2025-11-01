using UnityEngine;

public class PokemonStorageBox : MonoBehaviour
{
    Pokemon[,] boxes = new Pokemon[16, 42];

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
