using System.Collections.Generic;
using GDEUtils.StateMachine;
using UnityEngine;

    /// <summary>
    /// Make sure to set <see cref="SelectedPokemonIndex"/> before pushing this state!
    /// </summary>
public class SummaryState : State<GameController>
{
    [SerializeField] SummaryScreenUI summaryScreen;

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

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;

        playerParty = gc.Player.Party.Pokemon;
        summaryScreen.gameObject.SetActive(true);
        summaryScreen.SetBasicDetails(playerParty[SelectedPokemonIndex]);
        summaryScreen.SetSkills(playerParty[SelectedPokemonIndex]);
    }

    public override void Execute()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gc.stateMachine.Pop();
            return;
        }
    }

    public override void Exit()
    {
        summaryScreen.gameObject.SetActive(false);
    }
}
