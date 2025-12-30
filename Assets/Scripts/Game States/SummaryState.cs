using System.Collections.Generic;
using GDEUtils.StateMachine;
using UnityEngine;

/// <summary>
/// Make sure to set <see cref="SelectedPokemonIndex"/> before pushing this state!
/// </summary>
public class SummaryState : State<GameController>
{
    [SerializeField] SummaryScreenUI summaryScreen;
    [SerializeField] int pageCount = 3;

    //Input
    public int SelectedPokemonIndex { get; set; }

    /// <summary>
    /// Make sure to set <see cref="SelectedPokemonIndex"/> before pushing this state!
    /// </summary>
    public static SummaryState I { get; private set; }
    void Awake()
    {
        I = this;
    }

    List<Pokemon> playerParty;

    int selectedPage = 0;
    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        playerParty = gc.Player.Party.Pokemon;
        summaryScreen.gameObject.SetActive(true);
        summaryScreen.SetBasicDetails(playerParty[SelectedPokemonIndex]);
        summaryScreen.ShowPage(selectedPage);
    }

    public override void Execute()
    {
        if (!summaryScreen.InMoveSelection)
        {
            // Pokemon selection
            int prevIndex = SelectedPokemonIndex;
            int index = SelectedPokemonIndex;
            MenuSelectionMethods.HandleListSelection(ref index, playerParty.Count);
            // if (Input.GetKeyDown(KeyCode.DownArrow))
            // {
            //     SelectedPokemonIndex++;
            //     if (SelectedPokemonIndex >= playerParty.Count)
            //     {
            //         SelectedPokemonIndex = 0;
            //     }
            // }
            // else if (Input.GetKeyDown(KeyCode.UpArrow))
            // {
            //     SelectedPokemonIndex--;
            //     if (SelectedPokemonIndex < 0)
            //     {
            //         SelectedPokemonIndex = playerParty.Count - 1;
            //     }
            // }
            SelectedPokemonIndex = index;

            if (SelectedPokemonIndex != prevIndex)
            {
                summaryScreen.SetBasicDetails(playerParty[SelectedPokemonIndex]);
                summaryScreen.ShowPage(selectedPage);
            }

            // Page selection
            int prevPage = selectedPage;
            MenuSelectionMethods.HandleCategorySelection(ref selectedPage, pageCount);

            // if (Input.GetKeyDown(KeyCode.LeftArrow))
            // {
            //     selectedPage = (selectedPage - 1) % pageCount;
            // }
            // else if (Input.GetKeyDown(KeyCode.RightArrow))
            // {
            //     selectedPage = (selectedPage + 1) % pageCount;
            // }

            if (selectedPage != prevPage)
            {
                summaryScreen.ShowPage(selectedPage);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (selectedPage == 1 && !summaryScreen.InMoveSelection)
                summaryScreen.InMoveSelection = true;
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (summaryScreen.InMoveSelection)
                summaryScreen.InMoveSelection = false;
            else
            {
                gc.stateMachine.Pop();
                return;
            }
        }

        summaryScreen.HandleUpdate();
    }

    public override void Exit()
    {
        summaryScreen.gameObject.SetActive(false);
    }
}
