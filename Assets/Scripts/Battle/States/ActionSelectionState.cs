using System;
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
                MoveSelectionState.I.Moves = bs.PlayerUnit.Pokemon.Moves;
                bs.StateMachine.ChangeState(MoveSelectionState.I);
                break;
            case 1: // Bag
                // OpenBag();
                break;
            case 2: // Switch Pokemon
                // OpenPartyScreen();
                break;
            case 3: // Run
                // StartCoroutine(RunTurns(BattleAction.Run));
                break;
        }
    }
}
