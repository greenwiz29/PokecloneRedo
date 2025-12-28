using System.Collections.Generic;
using GDEUtils.StateMachine;
using UnityEngine;

/// <summary>
/// Make sure to set <see cref="ValidTargets"/> before pushing this state!
/// </summary>
public class TargetSelectionState : State<BattleSystem>
{
    int selectedTarget = 0;

    /// <summary>
    /// Make sure to set <see cref="ValidTargets"/> before pushing this state!
    /// </summary>
    public static TargetSelectionState I { get; private set; }

    // Input
    public List<BattleUnit> ValidTargets { get; set; }
    //Output
    public int SelectedTarget => selectedTarget;
    public bool SelectionMade { get; private set; }
    void Awake()
    {
        I = this;
    }

    BattleSystem bs;
    public override void Enter(BattleSystem owner)
    {
        bs = owner;

        selectedTarget = 0;
        SelectionMade = false;
        UpdateSelectionInUI();
    }

    public override void Exit()
    {
        ValidTargets[selectedTarget].SetSelected(false);
    }

    public override void Execute()
    {
        int prevSelection = selectedTarget;
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedTarget++;
            if (selectedTarget == ValidTargets.Count)
                selectedTarget = 0;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedTarget--;
            if (selectedTarget < 0)
                selectedTarget = ValidTargets.Count - 1;
        }

        if (selectedTarget != prevSelection)
        {
            UpdateSelectionInUI();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SelectionMade = true;
            bs.StateMachine.Pop();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            SelectionMade = false;
            bs.StateMachine.Pop();
        }
    }

    void UpdateSelectionInUI()
    {
        for (int i = 0; i < ValidTargets.Count; i++)
        {
            ValidTargets[i].SetSelected(i == selectedTarget);
        }
    }

    public List<BattleUnit> GetValidTargets(
        Move move,
        BattleUnit user,
        BattleSystem bs)
    {
        switch (move.Base.Target)
        {
            case MoveTarget.Foe:
                return bs.EnemyUnits;

            case MoveTarget.Ally:
                // Other friendly units only
                return bs.PlayerUnits.FindAll(u => u != user);

            case MoveTarget.Self:
                return new List<BattleUnit> { user };

            case MoveTarget.Area:
                // All units except self
                var targets = new List<BattleUnit>();
                targets.AddRange(bs.PlayerUnits);
                targets.AddRange(bs.EnemyUnits);
                targets.Remove(user);
                return targets;

            default:
                return new List<BattleUnit>();
        }
    }
}
