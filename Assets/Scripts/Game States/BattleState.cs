using System;
using GDEUtils.StateMachine;
using UnityEngine;

/// <summary>
/// Make sure to set <see cref="Trigger"/> and <see cref="Trainer"/> before pushing this state!
/// </summary>
public class BattleState : State<GameController>
{
    [SerializeField] BattleSystem battleSystem;

    /// <summary>
    /// Make sure to set <see cref="Trigger"/> and <see cref="Trainer"/> before pushing this state!
    /// </summary>
    public static BattleState I { get; private set; }

    // Input. Must be set before Enter runs
    public BattleTrigger Trigger { get; set; }
    public TrainerController Trainer { get; set; }

    void Awake()
    {
        I = this;
    }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;
        battleSystem.gameObject.SetActive(true);
        gc.WorldCamera.gameObject.SetActive(false);

        var playerParty = gc.Player.GetComponent<PokemonParty>();
        if (Trainer == null)
        {
            var wildPokemon = gc.CurrentScene.GetComponent<MapArea>().GetRandomWildPokemon(Trigger);
            var wildCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);

            battleSystem.StartBattle(playerParty, wildCopy, Trigger);
        }
        else
        {
            var trainerParty = Trainer.GetComponent<PokemonParty>();

            battleSystem.StartTrainerBattle(playerParty, trainerParty, Trigger);
        }

        battleSystem.OnBattleOver += EndBattle;
    }

    public override void Execute()
    {
        battleSystem.HandleUpdate();
    }

    public override void Exit()
    {
        battleSystem.gameObject.SetActive(false);
        gc.WorldCamera.gameObject.SetActive(true);

        battleSystem.OnBattleOver -= EndBattle;      
    }

    private void EndBattle(bool playerWon)
    {
        if (Trainer != null && playerWon)
        {
            Trainer.BattleLost();
            Trainer = null;
        }

        gc.stateMachine.Pop();
    }
}
