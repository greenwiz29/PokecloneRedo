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
        var context = new BattleContext
        {
            Trigger = Trigger,
            PlayerParty = playerParty,
            CanCatchPokemon = true,
            CanRun = true,
            CanSwitchPokemon = true,
            CanUseItems = true,
        };
        if (Trainer == null && WildPokemon == null)
        {
            // normal random encounter
            var wildPokemon = gc.CurrentScene.GetComponent<MapArea>().GetRandomWildPokemon(Trigger);
            if (wildPokemon == null)
            {
                Debug.LogError("No valid wild Pokémon generated");
                gc.stateMachine.Pop();
                return;
            }
            var wildCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);
            context.Type = BattleType.Wild;
            context.WildPokemon = wildCopy;
            context.StartingWeather = mapArea == null ? WeatherConditionID.none : mapArea.Weather;

            battleSystem.StartBattle(context);
        }
        else if (WildPokemon == null)
        {
            // trainer battle
            var profile = Trainer.BattleProfile;
            var trainerParty = Trainer.GetComponent<PokemonParty>();

            battleSystem.StartTrainerBattle(Trainer, Trigger);
        }
        else
        {
            // battle with overworld pokemon
            context.Type = BattleType.Wild;
            context.WildPokemon = WildPokemon.WildPokemon;
            context.StartingWeather = mapArea == null ? WeatherConditionID.none : mapArea.Weather;
            battleSystem.StartBattle(context);
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
        if (WildPokemon != null)
        {
            WildPokemon.OnBattleOver();
        }

        gc.stateMachine.Pop();
    }
}
