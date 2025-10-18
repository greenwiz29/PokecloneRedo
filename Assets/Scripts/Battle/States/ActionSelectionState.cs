using System;
using System.Collections;
using GDEUtils.StateMachine;
using UnityEngine;

public class ActionSelectionState : State<BattleSystem>
{
    [SerializeField] ActionSelectionUI selectionUI;

    public static ActionSelectionState I { get; private set; }
    void Awake()
    {
        I = this;
    }

    BattleSystem bs;
    public override void Enter(BattleSystem owner)
    {
        bs = owner;
        selectionUI.gameObject.SetActive(true);
        selectionUI.OnSelected += OnActionSelected;

        bs.DialogBox.EnableDialogText(true);
        bs.DialogBox.SetDialog("Choose an action.");
    }

    public override void Execute()
    {
        selectionUI.HandleUpdate();
    }

    public override void Exit()
    {
        selectionUI.gameObject.SetActive(false);
        selectionUI.OnSelected -= OnActionSelected;

        bs.DialogBox.EnableDialogText(false);
    }

    private void OnActionSelected(int selection)
    {
        switch (selection)
        {
            case 0: // Fight
                bs.SelectedAction = BattleAction.Move;
                MoveSelectionState.I.Moves = bs.PlayerUnit.Pokemon.Moves;
                bs.StateMachine.ChangeState(MoveSelectionState.I);
                break;
            case 1: // Bag
                StartCoroutine(GoToInventoryState());
                break;
            case 2: // Switch Pokemon
                StartCoroutine(GoToPartyState());
                break;
            case 3: // Run
                bs.SelectedAction = BattleAction.Run;
                bs.StateMachine.ChangeState(RunTurnState.I);
                break;
        }
    }

    IEnumerator GoToPartyState()
    {
        yield return GameController.I.stateMachine.PushAndWait(PartyState.I);
        var selectedPokemon = PartyState.I.SelectedPokemon;
        if (selectedPokemon != null)
        {
            bs.SelectedAction = BattleAction.Switch;
            bs.SelectedPokemon = selectedPokemon;
            bs.StateMachine.ChangeState(RunTurnState.I);
        }
    }
    
    IEnumerator GoToInventoryState()
    {
        yield return GameController.I.stateMachine.PushAndWait(InventoryState.I);
        var item = InventoryState.I.SelectedItem;
        if(item != null)
        {
            bs.SelectedAction = BattleAction.Item;
            bs.SelectedItem = item;
            bs.StateMachine.ChangeState(RunTurnState.I);
        }
    }
}
