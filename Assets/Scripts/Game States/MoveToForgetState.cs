using System.Collections.Generic;
using GDEUtils.StateMachine;
using UnityEngine;

/// <summary>
/// Make sure to set <see cref="CurrentMoves"/> and <see cref="NewMove"/> before pushing this state!
/// </summary>
public class MoveToForgetState : State<GameController>
{
    [SerializeField] MoveToForgetUI moveSelectionUI;

    // Inputs that must be set before entering the state
    public List<MoveBase> CurrentMoves { get; set; }
    public MoveBase NewMove { get; set; }

    // Output
    public int Selection { get; private set; }

    /// <summary>
    /// Make sure to set <see cref="CurrentMoves"/> and <see cref="NewMove"/> before pushing this state!
    /// </summary>
    public static MoveToForgetState I { get; private set; }
    void Awake()
    {
        I = this;
    }

    GameController gc;

    public override void Enter(GameController owner)
    {
        gc = owner;
        Selection = 0;

        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(CurrentMoves, NewMove);
        moveSelectionUI.SetSelectionSettings(GDEUtils.UI.SelectionMode.LIST);
        moveSelectionUI.OnSelected += OnMoveSelected;
        moveSelectionUI.OnBack += OnBack;
    }

    public override void Execute()
    {
        moveSelectionUI.HandleUpdate();
    }

    public override void Exit()
    {
        moveSelectionUI.gameObject.SetActive(false);
        moveSelectionUI.OnSelected -= OnMoveSelected;
        moveSelectionUI.OnBack -= OnBack;
    }

    private void OnBack()
    {
        Selection = -1;
        gc.stateMachine.Pop();
    }

    private void OnMoveSelected(int obj)
    {
        Selection = obj;
        gc.stateMachine.Pop();
    }
}
