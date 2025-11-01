using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonGiver : MonoBehaviour, ISavable
{
    [SerializeField] List<Pokemon> pokemonToGive;
    [SerializeField] Dialog dialog;

    bool Used = false;

    public IEnumerator GivePokemon(PlayerController player)
    {
        if (Used) yield break;

        int selection = 0;

        List<string> pokemonNames = pokemonToGive.Select(p => p.Name).ToList();
        yield return DialogManager.I.ShowDialog(dialog, pokemonNames, (choiceIndex) => selection = choiceIndex);

        var pokemon = pokemonToGive[selection];
        pokemon.Init();

        var dest = player.GetComponent<PokemonParty>().AddPokemon(pokemon);

        Used = true;

        yield return DialogManager.I.ShowDialogText($"You received {pokemon.Name}");
        
        string destMessage = "";
        switch (dest)
        {
            case AddedToDestination.PARTY:
                destMessage = "added to your party!";
                break;
            case AddedToDestination.STORAGE:
                destMessage = "sent to storage!";
                break;
        }
        yield return DialogManager.I.ShowDialogText($"{pokemon.Name} was {destMessage}");

    }

    public bool CanBeGiven()
    {
        return pokemonToGive.Count > 0 && !Used;
    }

    public object CaptureState()
    {
        return Used;
    }

    public void RestoreState(object state)
    {
        Used = (bool)state;
    }
}
