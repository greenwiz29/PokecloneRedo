using System;
using System.Collections;
using GDEUtils.StateMachine;
using GDEUtils.UI;
using UnityEngine;

public class GamePartyState : State<GameController>
{
    [SerializeField] PartyScreen partyScreen;

    public static GamePartyState I { get; private set; }
    void Awake()
    {
        I = this;
    }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;
        partyScreen.gameObject.SetActive(true);
        partyScreen.SetPartyData();
        partyScreen.OnSelected += OnPokemonSelected;
        partyScreen.OnBack += OnBack;
    }

    public override void Execute()
    {
        partyScreen.HandleUpdate(SelectionMode.GRID);
    }

    public override void Exit()
    {
        partyScreen.gameObject.SetActive(false);
        partyScreen.OnSelected -= OnPokemonSelected;
        partyScreen.OnBack -= OnBack;
    }

    private void OnBack()
    {
        gc.stateMachine.Pop();
    }

    private void OnPokemonSelected(int obj)
    {
        var prevState = gc.stateMachine.GetPrevState();

        if (prevState == InventoryState.I)
        {
            // Use item
            StartCoroutine(GoToUseItemState());
        }
        else
        {
            // TODO: Open summary screen
            Debug.Log($"Selected {partyScreen.SelectedPokemon.Base.Name}");
        }
    }

    IEnumerator GoToUseItemState()
    {
        yield return gc.stateMachine.PushAndWait(UseItemState.I);
        gc.stateMachine.Pop();
    }
}
