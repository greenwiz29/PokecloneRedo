using System.Collections;
using GDEUtils.StateMachine;
using UnityEngine;

/// <summary>
/// Make sure to set <see cref="NewPokemon"/> before pushing this state!
/// </summary>
public class AboutToUseState : State<BattleSystem>
{

    /// <summary>
    /// Make sure to set <see cref="NewPokemon"/> before pushing this state!
    /// </summary>
    public static AboutToUseState I { get; private set; }

    // Input
    public Pokemon NewPokemon { get; set; }

    void Awake()
    {
        I = this;
    }

    BattleSystem bs;
    bool aboutToUseChoice;
    public override void Enter(BattleSystem owner)
    {
        bs = owner;
        StartCoroutine(StartState());
    }

    public override void Execute()
    {
        if (!bs.DialogBox.IsChoiceBoxEnabled)
            return;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            aboutToUseChoice = !aboutToUseChoice;
        }

        bs.DialogBox.UpdateChoiceBoxSelection(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // change 'mon
            bs.DialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice)
            {
                StartCoroutine(SwitchAndContinueBattle());
            }
            else
                StartCoroutine(ContinueBattle());
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            bs.DialogBox.EnableChoiceBox(false);
            // cancel choice, keep current 'mon out
            // Send out next Pokemon
            StartCoroutine(ContinueBattle());
        }
    }

    IEnumerator StartState()
    {
        yield return bs.DialogBox.TypeDialog(
            $"{bs.Trainer.Name} is about to use {NewPokemon.Name}. Do you want to switch pokemon?"
        );

        bs.DialogBox.EnableChoiceBox(true);
    }

    IEnumerator SwitchAndContinueBattle()
    {
        yield return GameController.I.stateMachine.PushAndWait(PartyState.I);
        var selectedPokemon = PartyState.I.SelectedPokemon;
        if (selectedPokemon != null)
        {
            yield return bs.SwitchPokemon(selectedPokemon);
        }
        yield return ContinueBattle();
    }

    IEnumerator ContinueBattle()
    {
        yield return bs.SendNextTrainerPokemon();
        bs.StateMachine.Pop();
    }
}
