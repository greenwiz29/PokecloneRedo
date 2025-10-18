using System;
using System.Collections;
using GDEUtils.StateMachine;
using GDEUtils.UI;
using UnityEngine;

public class PartyState : State<GameController>
{
    [SerializeField] PartyScreen partyScreen;

    public static PartyState I { get; private set; }

    public Pokemon SelectedPokemon { get; private set; }

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
        partyScreen.SetSelectionSettings(GDEUtils.UI.SelectionMode.GRID, 2);
        partyScreen.OnSelected += OnPokemonSelected;
        partyScreen.OnBack += OnBack;
    }

    public override void Execute()
    {
        partyScreen.HandleUpdate();
    }

    public override void Exit()
    {
        partyScreen.gameObject.SetActive(false);
        partyScreen.OnSelected -= OnPokemonSelected;
        partyScreen.OnBack -= OnBack;
    }

    private void OnBack()
    {
        SelectedPokemon = null;

        var prevState = gc.stateMachine.GetPrevState();
        if (prevState == BattleState.I)
        {
            var battleState = BattleState.I;
            if (battleState.BattleSystem.PlayerUnit.Pokemon.HP <= 0)
            {
                partyScreen.SetMessageText("You must choose a pokemon to continue");
                return;
            }
        }
        gc.stateMachine.Pop();
    }

    private void OnPokemonSelected(int obj)
    {
        SelectedPokemon = partyScreen.SelectedPokemon;
        var prevState = gc.stateMachine.GetPrevState();

        if (prevState == InventoryState.I)
        {
            // Use item
            StartCoroutine(GoToUseItemState());
        }
        else if (prevState == BattleState.I)
        {
            var battleState = BattleState.I;
            if (SelectedPokemon.HP <= 0)
            {
                partyScreen.SetMessageText($"You can't send out a fainted pokemon");
                return;
            }
            if (SelectedPokemon == battleState.BattleSystem.PlayerUnit.Pokemon)
            {
                partyScreen.SetMessageText($"{SelectedPokemon.Base.Name} is already out.");
                return;
            }

            gc.stateMachine.Pop();
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
