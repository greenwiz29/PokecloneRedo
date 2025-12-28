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
        var move = Moves[selection];
        var user = bs.SelectedUnit; // or however you track the acting unit

        var validTargets = TargetSelectionState.I.GetValidTargets(move, user, bs);

        // No valid targets → move fails
        if (validTargets.Count == 0)
        {
            bs.DialogBox.SetDialog(
                $"{user.Pokemon.Name} tried to use {move.Base.Name}, but it failed!"
            );
            yield return new WaitForSeconds(1f);
            yield break;
        }

        // SELF or forced single-target auto-pick
        if (move.Base.Target == MoveTarget.Self ||
            validTargets.Count == 1)
        {
            CommitAction(move, validTargets[0], validTargets);
            yield break;
        }

        // AREA moves: no selector, targets resolved now
        if (move.Base.Target == MoveTarget.Area)
        {
            bs.AddBattleAction(new BattleAction
            {
                Type = BattleActionType.Move,
                SelectedMove = move,
                Targets = validTargets,
                MoveTargetType = MoveTarget.Area
            });
            yield break;
        }

        // Otherwise: player must choose
        TargetSelectionState.I.ValidTargets = validTargets;
        yield return bs.StateMachine.PushAndWait(TargetSelectionState.I);

        var chosenTarget = validTargets[TargetSelectionState.I.SelectedTarget];
        CommitAction(move, chosenTarget, new List<BattleUnit> { chosenTarget });
    }

    void CommitAction(Move move, BattleUnit target, List<BattleUnit> areaTargets)
    {
        bs.AddBattleAction(new BattleAction
        {
            Type = BattleActionType.Move,
            SelectedMove = move,
            Target = target,
            Targets = areaTargets,
            MoveTargetType = move.Base.Target
        });
    }
}
