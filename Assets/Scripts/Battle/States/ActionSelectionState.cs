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
                MoveSelectionState.I.Moves = bs.SelectedUnit.Pokemon.Moves;
                bs.StateMachine.ChangeState(MoveSelectionState.I);
                break;
            case 1: // Bag
                StartCoroutine(GoToInventoryState());
                break;
            case 2: // Switch Pokemon
                StartCoroutine(GoToPartyState());
                break;
            case 3: // Run
                bs.AddBattleAction(new BattleAction()
                {
                    Type = BattleActionType.Run
                });
                break;
        }
    }

    IEnumerator GoToPartyState()
    {
        yield return GameController.I.stateMachine.PushAndWait(PartyState.I);
        var selectedPokemon = PartyState.I.SelectedPokemon;
        if (selectedPokemon != null)
        {
            bs.AddBattleAction(new BattleAction()
            {
                Type = BattleActionType.Switch,
                SelectedPokemon = selectedPokemon
            });
        }
    }

    IEnumerator GoToInventoryState()
    {
        yield return GameController.I.stateMachine.PushAndWait(InventoryState.I);
        var item = InventoryState.I.SelectedItem;
        if (item != null)
        {
            bs.AddBattleAction(new BattleAction()
            {
                Type = BattleActionType.Item,
                SelectedItem = item
            });
        }
    }
}
