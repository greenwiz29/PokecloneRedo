using GDEUtils.StateMachine;
using UnityEngine;

    /// <summary>
    /// Make sure to set <see cref="Trigger"/> and <see cref="Trainer"/> before pushing this state!
    /// <para><see cref="WildPokemon"/> must be set for overworld wild pokemon, but can be left null.</para>
    /// </summary>
public class BattleState : State<GameController>
{
    [SerializeField] BattleSystem battleSystem;

    /// <summary>
    /// Make sure to set <see cref="Trigger"/> and <see cref="Trainer"/> before pushing this state!
    /// <para><see cref="WildPokemon"/> must be set for overworld wild pokemon, but can be left null.</para>
    /// </summary>
    public static BattleState I { get; private set; }
    public BattleSystem BattleSystem => battleSystem;

    // Input. Must be set before Enter runs
    public BattleTrigger Trigger { get; set; }
    public TrainerController Trainer { get; set; }
    public WildPokemonController WildPokemon { get; set; }

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
        var mapArea = gc.CurrentScene.GetComponent<MapArea>();
        if (Trainer == null && WildPokemon == null)
        {
            // normal random encounter
            var wildPokemon = gc.CurrentScene.GetComponent<MapArea>().GetRandomWildPokemon(Trigger);
            var wildCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);

            battleSystem.StartBattle(playerParty, wildCopy, Trigger, mapArea.Weather);
        }
        else if (WildPokemon == null)
        {
            // trainer battle
            var trainerParty = Trainer.GetComponent<PokemonParty>();

            battleSystem.StartTrainerBattle(playerParty, trainerParty, Trigger, Trainer.BattleUnitCount, mapArea.Weather);
        }
        else
        {
            // battle with overworld pokemon
            battleSystem.StartBattle(playerParty, WildPokemon.WildPokemon, Trigger, mapArea.Weather);
        }

        battleSystem.OnBattleOver += EndBattle;
    }

    public override void Execute()
    {
        battleSystem.HandleUpdate();
    }

    public override void Exit()
    {
        WildPokemon = null;

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
        if(WildPokemon != null)
        {
            WildPokemon.OnBattleOver();
        }

        gc.stateMachine.Pop();
    }
}
