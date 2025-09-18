using System;
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
        // TODO: Open summary screen
        Debug.Log($"Selected {partyScreen.SelectedPokemon.Base.Name}");
	}
}
