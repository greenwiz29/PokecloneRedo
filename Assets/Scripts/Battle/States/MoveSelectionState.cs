using System;
using System.Collections;
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
        StartCoroutine(OnMoveSelectedAsync(selection));
    }
    
    IEnumerator OnMoveSelectedAsync(int selection)
    {
        // TODO: adjust to use the MoveTarget enum to determine what set of units to choose from
        int moveTarget = 0;
        if (bs.ActiveEnemyUnitsCount > 1)
        {
            yield return bs.StateMachine.PushAndWait(TargetSelectionState.I);
            if (!TargetSelectionState.I.SelectionMade)
            {
                yield break;
            }
            moveTarget = TargetSelectionState.I.SelectedTarget;
        }

        bs.AddBattleAction(new BattleAction()
        {
            Type = BattleActionType.Move,
            SelectedMove = Moves[selection],
            Target = bs.EnemyUnits[moveTarget]
        });
    }
}
