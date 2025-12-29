using System;

public class BattleContext
{
    public BattleType Type;
    public BattleTrigger Trigger;

    public int UnitCount = 1;

    public bool CanUseItems;
    public bool CanSwitchPokemon;
    public bool CanRun;
    public bool CanCatchPokemon;

    public WeatherConditionID StartingWeather;

    public TrainerController Trainer;
    public PokemonParty PlayerParty;
    public PokemonParty EnemyParty;
    public Pokemon WildPokemon;

    public TrainerBattleProfile Profile;

    public Action<BattleSystem> OnBattleStart;
    public Action<BattleSystem, bool> OnBattleEnd;
}

public enum BattleType { Wild, Trainer, GymLeader, Boss }
