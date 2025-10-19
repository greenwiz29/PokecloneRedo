using System;
using System.Collections.Generic;
using GDEUtils.StateMachine;
using UnityEngine;

/// <summary>
/// Make sure to set <see cref="Moves"/> before pushing this state!
/// </summary>
public class MoveSelectionState : State<BattleSystem>
{
    [SerializeField] MoveSelectionUI selectionUI;
    [SerializeField] GameObject moveDetailsUI;

    //Input
    public List<Move> Moves { get; set; }

    /// <summary>
    /// Make sure to set <see cref="Moves"/> before pushing this state!
    /// </summary>
    public static MoveSelectionState I { get; private set; }
    void Awake()
    {
        I = this;
    }

    BattleSystem bs;
    public override void Enter(BattleSystem owner)
    {
        bs = owner;

        selectionUI.SetMoves(Moves);
        selectionUI.gameObject.SetActive(true);
        moveDetailsUI.SetActive(true);
        selectionUI.OnSelected += OnMoveSelected;
        selectionUI.OnBack += OnBack;

        bs.DialogBox.EnableDialogText(false);
    }

    public override void Execute()
    {
        selectionUI.HandleUpdate();
    }

    public override void Exit()
    {
        selectionUI.gameObject.SetActive(false);
        moveDetailsUI.SetActive(false);
        selectionUI.OnSelected -= OnMoveSelected;
        selectionUI.OnBack -= OnBack;

        bs.DialogBox.EnableDialogText(true);
    }

    private void OnBack()
    {
        bs.StateMachine.ChangeState(ActionSelectionState.I);
    }

    private void OnMoveSelected(int selection)
    {
        // TODO: create Targeting state to select target instead of defaulting to the first unit
        bs.AddBattleAction(new BattleAction()
        {
            Type = BattleActionType.Move,
            SelectedMove = Moves[selection],
            Target = bs.EnemyUnits[0]
        });
    }
}
