using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GDEUtils.StateMachine;
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

    PokemonParty playerParty;
    bool isSwitchingPosition;
    int selectedIndexForSwitching = 0;
    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;
        partyScreen.gameObject.SetActive(true);
        partyScreen.SetPartyData();
        partyScreen.SetSelectionSettings(GDEUtils.UI.SelectionMode.GRID, 2);
        partyScreen.OnSelected += OnPokemonSelected;
        partyScreen.OnBack += OnBack;
        playerParty = PokemonParty.GetPlayerParty();
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
            if (battleState.BattleSystem.PlayerUnits.Any(u => u.Pokemon.HP <= 0))
            {
                partyScreen.SetMessageText("You must choose a pokemon to continue");
                return;
            }
        }
        gc.stateMachine.Pop();
    }

    private void OnPokemonSelected(int selection)
    {
        StartCoroutine(PokemonSelectedAction(selection));
    }

    IEnumerator GoToUseItemState()
    {
        yield return gc.stateMachine.PushAndWait(UseItemState.I);
        gc.stateMachine.Pop();
    }

    IEnumerator PokemonSelectedAction(int selection)
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
            DynamicMenuState.I.MenuItems = new List<string>() { "Switch", "Summary", "Cancel" };
            yield return gc.stateMachine.PushAndWait(DynamicMenuState.I);
            int result;
            if (DynamicMenuState.I.SelectedItem != null)
            {
                result = (int)DynamicMenuState.I.SelectedItem;
                switch (result)
                {
                    case 0:
                        // Switch
                        if (SelectedPokemon.HP <= 0)
                        {
                            partyScreen.SetMessageText($"You can't send out a fainted pokemon");
                            yield break;
                        }
                        if (battleState.BattleSystem.PlayerUnits.Any(u => u.Pokemon == SelectedPokemon))
                        {
                            partyScreen.SetMessageText($"{SelectedPokemon.Base.Name} is already out.");
                            yield break;
                        }
                        if (battleState.BattleSystem.UnitCount > 1 && battleState.BattleSystem.IsPokemonSelectedToShift(SelectedPokemon))
                        {
                            partyScreen.SetMessageText($"You can't send {SelectedPokemon.Base.Name} out twice!");
                            yield break;
                        }

                        gc.stateMachine.Pop();
                        break;
                    case 1:
                        // Summary
                        SummaryState.I.SelectedPokemonIndex = selection;
                        yield return gc.stateMachine.PushAndWait(SummaryState.I);
                        break;
                    case 2:
                    // Cancel
                    default:
                        yield break;
                }
            }

        }
        else
        {
            if (isSwitchingPosition)
            {
                if (selectedIndexForSwitching == selection)
                {
                    partyScreen.SetMessageText($"You can't switch {SelectedPokemon.Name} with itself!");
                    yield break;
                }

                isSwitchingPosition = false;
                // Swap pokemon
                (playerParty.Pokemon[selection], playerParty.Pokemon[selectedIndexForSwitching]) = (playerParty.Pokemon[selectedIndexForSwitching], playerParty.Pokemon[selection]);
                playerParty.PartyUpdated();

                yield break;
            }

            DynamicMenuState.I.MenuItems = new List<string>() { "Summary", "Switch", "Cancel" };
            yield return gc.stateMachine.PushAndWait(DynamicMenuState.I);
            int result;
            if (DynamicMenuState.I.SelectedItem != null)
            {
                result = (int)DynamicMenuState.I.SelectedItem;
                switch (result)
                {
                    case 0:
                        // Summary
                        SummaryState.I.SelectedPokemonIndex = selection;
                        yield return gc.stateMachine.PushAndWait(SummaryState.I);
                        break;
                    case 1:
                        // Switch
                        isSwitchingPosition = true;
                        selectedIndexForSwitching = selection;
                        partyScreen.SetMessageText("Choose a Pokemon to swap spots with.");
                        break;
                    case 2:
                    // Cancel
                    default:
                        yield break;
                }
            }
        }

    }
}
