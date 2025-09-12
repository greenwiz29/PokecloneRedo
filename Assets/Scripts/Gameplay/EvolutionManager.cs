using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionManager : MonoSingleton<EvolutionManager>
{
    [SerializeField] GameObject evolutionUI;
    [SerializeField] Image pokemonImage;

    public event Action OnStartEvolution;
    public event Action OnEndEvolution;

    public IEnumerator Evolve(Pokemon pokemon, Evolution evolution, StatChangesWrapper statChanges)
    {
        OnStartEvolution?.Invoke();
        evolutionUI.SetActive(true);

        pokemonImage.sprite = pokemon.Base.FrontSprite;
        var oldName = pokemon.Name;

        yield return DialogManager.I.ShowDialogText($"{oldName} is evolving!");

        if (statChanges != null)
            statChanges.Changes = pokemon.Evolve(evolution);
        else
            pokemon.Evolve(evolution);

        pokemonImage.sprite = pokemon.Base.FrontSprite;

        yield return DialogManager.I.ShowDialogText($"{oldName} evolved into {pokemon.Name}!");

        evolutionUI.SetActive(false);
        OnEndEvolution?.Invoke();
    }
}
