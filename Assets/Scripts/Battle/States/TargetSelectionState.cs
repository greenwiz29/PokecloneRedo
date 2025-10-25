using GDEUtils.StateMachine;
using UnityEngine;

public class TargetSelectionState : State<BattleSystem>
{
    int selectedTarget = 0;
    public static TargetSelectionState I { get; private set; }

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
        bs.EnemyUnits[selectedTarget].SetSelected(false);
    }

    public override void Execute()
    {
        int prevSelection = selectedTarget;
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedTarget++;
            if (selectedTarget == bs.ActiveEnemyUnitsCount)
                selectedTarget = 0;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedTarget--;
            if (selectedTarget < 0)
                selectedTarget = bs.ActiveEnemyUnitsCount - 1;
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
        for (int i = 0; i < bs.EnemyUnits.Count; i++)
        {
            bs.EnemyUnits[i].SetSelected(i == selectedTarget);
        }
    }
}
