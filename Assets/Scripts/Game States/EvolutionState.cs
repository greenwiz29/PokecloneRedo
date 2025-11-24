using System;
using System.Collections;
using GDEUtils.StateMachine;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionState : State<GameController>
{
    [SerializeField] GameObject evolutionUI;
    [SerializeField] Image pokemonImage;

    public static EvolutionState I { get; private set; }

    void Awake()
    {
        I = this;
    }

    GameController gc;
    public IEnumerator Evolve(Pokemon pokemon, Evolution evolution, StatChangesWrapper statChanges)
    {
        gc = GameController.I;

        gc.stateMachine.Push(this);

        evolutionUI.SetActive(true);

        pokemonImage.sprite = pokemon.FrontSprite;
        var oldName = pokemon.Name;

        yield return DialogManager.I.ShowDialogText($"{oldName} is evolving!");

        if (statChanges != null)
            statChanges.Changes = pokemon.Evolve(evolution);
        else
            pokemon.Evolve(evolution);

        pokemonImage.sprite = pokemon.FrontSprite;

        yield return DialogManager.I.ShowDialogText($"{oldName} evolved into {pokemon.Name}!");

        evolutionUI.SetActive(false);
        gc.PartyScreen.SetPartyData();

        gc.stateMachine.Pop();
    }
}
