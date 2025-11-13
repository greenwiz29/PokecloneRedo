using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;

    [SerializeField] int level;

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;

        Init();
    }

    public PokemonBase Base => _base;
    public int Level => level;
    int exp;
    public static int maxMoves = 4;

    public string Name => _base.name;

    public int Attack => GetStat(Stat.Attack);

    public int Defense => GetStat(Stat.Defense);

    public int SpAttack => GetStat(Stat.SpAttack);

    public int SpDefense => GetStat(Stat.SpDefense);

    public int Speed => GetStat(Stat.Speed);

    public int MaxHP { get; private set; }

    public int HP { get; set; }
    public int Exp
    {
        get => exp;
        set
        {
            exp = value;
        }
    }

    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public StatusCondition Status { get; private set; }
    public StatusCondition VolatileStatus { get; private set; }
    public int StatusTime { get; set; }
    public int VolatileStatusTime { get; set; }
    public Queue<StatusEvent> StatusChanges { get; private set; }
    public event Action OnStatusChanged;
    public event Action OnHPChanged;

    public Pokemon(PokemonSaveData saveData)
    {
        _base = PokemonDB.GetObjectByName(saveData.name);
        HP = saveData.hp;
        level = saveData.level;
        exp = saveData.exp;
        _base.GrowthRate = saveData.growthRate;

        if (saveData.status != null)
            Status = StatusConditionsDB.Conditions[saveData.status.Value];
        else
            Status = null;

        Moves = saveData.moves.Select(m => new Move(m)).ToList();

        InitCondition();
    }

    public void Init()
    {
        // Set Base Growth Rate randomly
        var growthRates = Enum.GetValues(typeof(GrowthRate));
        int randomIndex = UnityEngine.Random.Range(0, growthRates.Length);
        Base.GrowthRate = (GrowthRate)growthRates.GetValue(randomIndex);

        Exp = Base.CalculateBaseExpForLevel(Level);

        // Generate Moves
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.LevelLearned <= Level)
            {
                Moves.Add(new Move(move.Base));
            }

            if (Moves.Count > maxMoves)
            {
                // unlearn earliest move
                Moves.RemoveAt(0);
            }
        }

        InitCondition();
    }

    private void InitCondition()
    {
        CalculateStats();

        HP = MaxHP;
        StatusChanges = new Queue<StatusEvent>();
        ResetStatBoosts();
        CureStatus();
        CureVolatileStatus();

        foreach (var move in Moves)
        {
            move.PP = move.Base.PP;
        }
    }

    public void Heal()
    {
        InitCondition();
        OnHPChanged?.Invoke();
    }

    public PokemonSaveData GetSaveData()
    {
        var saveData = new PokemonSaveData()
        {
            name = Name,
            hp = HP,
            level = Level,
            exp = Exp,
            status = Status?.Id,
            moves = Moves.Select(m => m.GetSaveData()).ToList(),
            growthRate = Base.GrowthRate
        };
        return saveData;
    }

    public float GetNormalizedExp()
    {
        int currLevelExp = Base.CalculateBaseExpForLevel(Level);
        int nextLevelExp = Base.CalculateBaseExpForLevel(Level + 1);

        float normalizedExp = ((float)Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

    public StatChanges CheckForLevelUp(out LearnableMove newMove)
    {
        int nextLevelExp = Base.CalculateBaseExpForLevel(Level + 1);
        if (Exp >= nextLevelExp)
        {
            ++level;

            // Update stats
            var statChanges = CalculateStats();

            HP += statChanges.hpDiff;

            newMove = GetLearnableMoveForLevel();

            return statChanges;
        }
        newMove = null;
        return null;
    }

    public bool TryLearnMove(MoveBase newMove)
    {
        bool learned = false;

        if (Moves.Count < maxMoves)
        {
            Moves.Add(new Move(newMove));
            learned = true;
        }
        return learned;
    }

    public Evolution CheckForEvolution()
    {
        return Base.Evolutions.FirstOrDefault(e => e.RequiredLevel != -1 && e.RequiredLevel <= level);
    }

    public Evolution CheckForEvolution(ItemBase item)
    {
        return Base.Evolutions.FirstOrDefault(e => e.RequiredItem == item);
    }

    public StatChanges Evolve(Evolution evolution)
    {
        _base = evolution.EvolvesInto;
        var statChanges = CalculateStats();

        HP += statChanges.hpDiff;

        return statChanges;
    }

    public bool HasMove(MoveBase move)
    {
        return Moves.Count(m => m.Base == move) > 0;
    }

    private LearnableMove GetLearnableMoveForLevel()
    {
        return Base.LearnableMoves.Where(m => m.LevelLearned == level).FirstOrDefault();
    }

    StatChanges CalculateStats()
    {
        bool oldStatsExist = false;
        int oldAtk = 0, oldDef = 0, oldSpAtk = 0, oldSpDef = 0, oldSpeed = 0, oldMaxHP = 0;
        if (Stats != null)
        {
            oldAtk = Stats[Stat.Attack];
            oldDef = Stats[Stat.Defense];
            oldSpAtk = Stats[Stat.SpAttack];
            oldSpDef = Stats[Stat.SpDefense];
            oldSpeed = Stats[Stat.Speed];
            oldMaxHP = MaxHP;
            oldStatsExist = true;
        }
        Stats = new Dictionary<Stat, int>
        {
            { Stat.Attack, Mathf.FloorToInt(Base.Attack * Level / 100f) + 5 },
            { Stat.Defense, Mathf.FloorToInt(Base.Defense * Level / 100f) + 5 },
            { Stat.SpAttack, Mathf.FloorToInt(Base.SpAttack * Level / 100f) + 5 },
            { Stat.SpDefense, Mathf.FloorToInt(Base.SpDef * Level / 100f) + 5 },
            { Stat.Speed, Mathf.FloorToInt(Base.Speed * Level / 100f) + 5 }
        };

        MaxHP = Mathf.FloorToInt(Base.MaxHP * Level / 100f) + 10 + Level;

        if (oldStatsExist)
        {
            StatChanges changes = new()
            {
                atkDiff = Stats[Stat.Attack] - oldAtk,
                defDiff = Stats[Stat.Defense] - oldDef,
                spAtkDiff = Stats[Stat.SpAttack] - oldSpAtk,
                spDefDiff = Stats[Stat.SpDefense] - oldSpDef,
                speedDiff = Stats[Stat.Speed] - oldSpeed,
                hpDiff = MaxHP - oldMaxHP
            };
            return changes;
        }
        else return null;
    }

    public void ResetStatBoosts()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            { Stat.Attack, 0 },
            { Stat.Defense, 0 },
            { Stat.SpAttack, 0 },
            { Stat.SpDefense, 0 },
            { Stat.Speed, 0 },
            { Stat.Accuracy, 0 },
            { Stat.Evasion, 0 },
        };
    }

    int GetStat(Stat stat)
    {
        int value = Stats[stat];

        // TODO: apply stat boost
        int boost = StatBoosts[stat];
        var boostValue = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost < 0)
        {
            value = Mathf.FloorToInt(value / boostValue[-boost]);
        }
        else
        {
            value = Mathf.FloorToInt(value * boostValue[boost]);
        }

        return value;
    }

    public void ApplyBoosts(List<StatBoost> boosts)
    {
        foreach (var statBoost in boosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (boost > 0 && boost < 6)
            {
                AddStatusEvent(StatusEventType.StatBoost, $"{Base.Name}'s {stat} rose!");
            }
            else if (boost == 6)
            {
                AddStatusEvent(StatusEventType.StatBoost, $"{Base.Name}'s {stat} can't go any higher.");
            }
            else if (boost < 0 && boost > -6)
            {
                AddStatusEvent(StatusEventType.StatBoost, $"{Base.Name}'s {stat} fell!");
            }
            else if (boost == -6)
            {
                AddStatusEvent(StatusEventType.StatBoost, $"{Base.Name}'s {stat} can't go any lower.");
            }
        }
    }

    public void SetStatus(StatusConditionID conditionID)
    {
        if (Status != null)
            return;

        Status = StatusConditionsDB.Conditions[conditionID];
        Status?.OnStart?.Invoke(this);
        AddStatusEvent($"{Base.Name} {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }

    public void SetVolatileStatus(StatusConditionID conditionID)
    {
        if (Status != null)
            return;

        VolatileStatus = StatusConditionsDB.Conditions[conditionID];
        VolatileStatus?.OnStart?.Invoke(this);
        AddStatusEvent($"{Base.Name} {VolatileStatus.StartMessage}");
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }

    public void AddStatusEvent(StatusEventType type, string message)
    {
        StatusChanges.Enqueue(new StatusEvent(type, message));
    }

    public void AddStatusEvent(string message)
    {
        AddStatusEvent(StatusEventType.Text, message);
    }

    public bool OnBeforeMove()
    {
        bool canPerformMove = true;

        if (Status?.OnBeforeMove != null)
            canPerformMove = Status.OnBeforeMove.Invoke(this);

        if (VolatileStatus?.OnBeforeMove != null)
            canPerformMove = VolatileStatus.OnBeforeMove.Invoke(this);

        return canPerformMove;
    }

    public DamageDetails ApplyDamage(Move move, Pokemon attacker, float weatherModifier = 1f, int hitCount = 1)
    {
        float crit = 1f;
        // TODO: make critRate constant
        if (UnityEngine.Random.value * 100f <= move.Base.CritChance)
            crit = 2f;
        float type = GetTypeEffectiveness(move.Base.Type);

        var damageDetails = new DamageDetails() { TypeEffectiveness = type, Crit = crit, WeatherModifier = weatherModifier, MoveHitsCount = hitCount };

        float modifiers = UnityEngine.Random.Range(0.85f, 1.1f) * type * crit * weatherModifier;
        float a = (2 * attacker.Level + 10) / 250f;

        float attack = move.Base.MoveType == MoveType.Physical ? attacker.Attack : attacker.SpAttack;
        float defense = move.Base.MoveType == MoveType.Physical ? Defense : SpDefense;
        float d = a * move.Base.Power * ((float)attack / defense);

        int damage = Mathf.FloorToInt(d * modifiers);

        ReduceHP(damage);

        return damageDetails;
    }

    public float GetTypeEffectiveness(PokemonType type)
    {
        return TypeChart.GetEffectiveness(type, Base.Type1)
            * TypeChart.GetEffectiveness(type, Base.Type2);
    }

    public void ReduceHP(int damage, bool callUpdateEvent = false)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHP);
        if (callUpdateEvent)
            OnHPChanged?.Invoke();
    }

    public void IncreaseHP(int damage, bool callUpdateEvent = false)
    {
        HP = Mathf.Clamp(HP + damage, 0, MaxHP);
        if (callUpdateEvent)
            OnHPChanged?.Invoke();
    }

    public void OnBattleOver()
    {
        ResetStatBoosts();
        CureVolatileStatus();
    }

    public Move GetRandomMove()
    {
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();

        if (movesWithPP == null || movesWithPP.Count == 0)
        {
            // no more usable moves left, time to Struggle
            return null;
        }

        int r = UnityEngine.Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }

    public bool IsOfType(PokemonType type)
    {
        return type == Base.Type1 || type == Base.Type2;
    }
}

public class DamageDetails
{
    public float Crit { get; set; }
    public float TypeEffectiveness { get; set; }
    public float WeatherModifier { get; set; }
    public int MoveHitsCount { get; set; }
}

public enum StatusEventType { Text, Damage, StatBoost }

public class StatusEvent
{
    public StatusEventType Type { get; private set; }
    public string Message { get; private set; }
    public StatusEvent(StatusEventType type, string message)
    {
        Type = type;
        Message = message;
    }
}

public class StatChanges
{
    public int atkDiff, defDiff, spAtkDiff, spDefDiff, speedDiff, hpDiff;
}

public class StatChangesWrapper
{
    public StatChanges Changes;
}

[Serializable]
public class PokemonSaveData
{
    public string name; // might replace with dexID later
    public int level;
    public int hp;
    public int exp;
    public StatusConditionID? status;
    public List<MoveSaveData> moves;
    public GrowthRate growthRate;
}
