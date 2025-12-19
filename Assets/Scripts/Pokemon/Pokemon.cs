using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;

    [SerializeField] int level;
    [SerializeField] PokemonGender gender;

    private AbilityID abilityID;
    bool useHiddenAbility = false;

    public PokemonBase Base => _base;
    public int Level => level;
    int exp;
    public static int maxMoves = 4;

    public string Name => _base.name;
    public PokemonGender Gender { get { return gender; } }

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
    public bool IsShiny { get; private set; }

    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }
    public Dictionary<Stat, int> Stats { get; private set; }
    public Dictionary<Stat, int> StatIVs { get; private set; }
    public Dictionary<Stat, int> StatBoosts { get; private set; }
    public StatusCondition Status { get; private set; }
    public StatusCondition VolatileStatus { get; private set; }
    public int StatusTime { get; set; }
    public int VolatileStatusTime { get; set; }
    public Queue<StatusEvent> StatusChanges { get; private set; }
    public Ability Ability { get; set; }
    public GrowthRate GrowthRate { get; set; }
    public Sprite FrontSprite { get; private set; }
    public Sprite BackSprite { get; private set; }
    public List<Sprite> WalkDownAnim { get; private set; }
    public List<Sprite> WalkUpAnim { get; private set; }
    public List<Sprite> WalkLeftAnim { get; private set; }
    public List<Sprite> WalkRightAnim { get; private set; }
    public event Action OnStatusChanged;
    public event Action OnHPChanged;

    /// <summary>
    /// Generate a new Pokemon of the given species and level.
    /// </summary>
    /// <param name="pBase">The species of the Pokemon to generate.</param>
    /// <param name="pLevel">The level of the Pokemon to generate.</param>
    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;

        Init();
    }

    /// <summary>
    /// Generate a Pokemon from the given save data.
    /// </summary>
    /// <param name="saveData">The saved data of the Pokemon to generate.</param>
    public Pokemon(PokemonSaveData saveData)
    {
        _base = PokemonDB.GetObjectByName(saveData.name);
        HP = saveData.hp;
        level = saveData.level;
        exp = saveData.exp;
        useHiddenAbility = saveData.useHiddenAbility;
        abilityID = saveData.ability;
        GrowthRate = saveData.growthRate;
        IsShiny = saveData.shiny;
        gender = saveData.gender;
        StatIVs = saveData.statIVs;

        if (saveData.status != null)
            Status = StatusConditionsDB.Conditions[saveData.status.Value];
        else
            Status = null;

        Moves = saveData.moves.Select(m => new Move(m)).ToList();

        InitCondition();
    }

    /// <summary>
    /// Initialize a new Pokemon.
    /// <para>Randomly sets growth rate, ability, gender, shininess, and stat IVs.</para>
    /// <para>Also sets basic starting moves based on level.</para>
    /// </summary>
    public void Init()
    {
        // Set Base Growth Rate randomly
        var growthRates = Enum.GetValues(typeof(GrowthRate));
        int randomIndex = UnityEngine.Random.Range(0, growthRates.Length);
        GrowthRate = (GrowthRate)growthRates.GetValue(randomIndex);

        Exp = CalculateBaseExpForLevel(Level);

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

        // Set ability at random from Base's normal or hidden
        int random = UnityEngine.Random.Range(0, 101);
        if (random < Base.NormalAbilityPercent)
        {
            abilityID = Base.AbilityID;
        }
        else
        {
            abilityID = Base.HiddenAbilityID;
            useHiddenAbility = true;
        }

        // Shiny?
        random = UnityEngine.Random.Range(0, 5);
        IsShiny = random == 1;

        DecideGender();

        // Generate IVs
        StatIVs = new Dictionary<Stat, int>()
        {
            {Stat.HP, UnityEngine.Random.Range(1,32)},
            {Stat.Attack, UnityEngine.Random.Range(1,32)},
            {Stat.Defense, UnityEngine.Random.Range(1,32)},
            {Stat.SpAttack, UnityEngine.Random.Range(1,32)},
            {Stat.SpDefense, UnityEngine.Random.Range(1,32)},
            {Stat.Speed, UnityEngine.Random.Range(1,32)},
        };

        InitCondition();
    }

    /// <summary>
    /// Initializes/resets the Pokemon's condition.
    /// <para>Recalulates stats, resets sprites, clears all status conditions and stat boosts, 
    /// and restores HP and PP of all moves.</para> 
    /// </summary>
    private void InitCondition()
    {
        CalculateStats();
        SetSprites(IsShiny, gender);

        HP = MaxHP;

        if (abilityID != AbilityID.none)
        {
            Ability = AbilityDB.Abilities[abilityID];
        }

        StatusChanges = new Queue<StatusEvent>();
        ResetStatBoosts();
        CureStatus();
        CureVolatileStatus();

        foreach (var move in Moves)
        {
            move.PP = move.Base.PP;
        }
    }

    /// <summary>
    /// Randomly set gender based on the species' defined GenderRatio.
    /// </summary>
    public void DecideGender()
    {
        if (gender != PokemonGender.NotSet) return;

        int ran = UnityEngine.Random.Range(1, 8);
        switch (Base.GenderRatio)
        {
            case PokemonGenderRatio.OneInTwoFemale:
                if (ran <= 4)
                    gender = PokemonGender.Female;
                else
                    gender = PokemonGender.Male;
                break;
            case PokemonGenderRatio.OneInFourFemale:
                if (ran <= 2)
                    gender = PokemonGender.Female;
                else
                    gender = PokemonGender.Male;
                break;
            case PokemonGenderRatio.OneInEightFemale:
                if (ran <= 1)
                    gender = PokemonGender.Female;
                else
                    gender = PokemonGender.Male;
                break;
            case PokemonGenderRatio.ThreeInFourFemale:
                if (ran <= 6)
                    gender = PokemonGender.Female;
                else
                    gender = PokemonGender.Male;
                break;
            case PokemonGenderRatio.SevenInEightFemale:
                if (ran <= 7)
                    gender = PokemonGender.Female;
                else
                    gender = PokemonGender.Male;
                break;
            case PokemonGenderRatio.FemaleOnly:
                gender = PokemonGender.Female;
                break;
            case PokemonGenderRatio.MaleOnly:
                gender = PokemonGender.Male;
                break;
            case PokemonGenderRatio.Ditto:
                gender = PokemonGender.Ditto;
                break;
            default:
                gender = PokemonGender.Genderless;
                break;
        }
    }

    public void SetSprites(bool isShiny, PokemonGender gender)
    {
        switch (gender)
        {
            case PokemonGender.Female:
                if (isShiny)
                {
                    FrontSprite = Base.FrontSpriteFemaleShiny;
                    BackSprite = Base.BackSpriteFemaleShiny;
                    WalkDownAnim = Base.WalkDownAnimShiny;
                    WalkUpAnim = Base.WalkUpAnimShiny;
                    WalkLeftAnim = Base.WalkLeftAnimShiny;
                    WalkRightAnim = Base.WalkRightAnimShiny;
                }
                else
                {
                    FrontSprite = Base.FrontSpriteFemale;
                    BackSprite = Base.BackSpriteFemale;
                    WalkDownAnim = Base.WalkDownAnim;
                    WalkUpAnim = Base.WalkUpAnim;
                    WalkLeftAnim = Base.WalkLeftAnim;
                    WalkRightAnim = Base.WalkRightAnim;
                }
                break;
            default:
                if (isShiny)
                {
                    FrontSprite = Base.FrontSpriteShiny;
                    BackSprite = Base.BackSpriteShiny;
                    WalkDownAnim = Base.WalkDownAnimShiny;
                    WalkUpAnim = Base.WalkUpAnimShiny;
                    WalkLeftAnim = Base.WalkLeftAnimShiny;
                    WalkRightAnim = Base.WalkRightAnimShiny;
                }
                else
                {
                    FrontSprite = Base.FrontSprite;
                    BackSprite = Base.BackSprite;
                    WalkDownAnim = Base.WalkDownAnim;
                    WalkUpAnim = Base.WalkUpAnim;
                    WalkLeftAnim = Base.WalkLeftAnim;
                    WalkRightAnim = Base.WalkRightAnim;
                }
                break;
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
            ability = abilityID,
            useHiddenAbility = this.useHiddenAbility,
            status = Status?.Id,
            moves = Moves.Select(m => m.GetSaveData()).ToList(),
            growthRate = GrowthRate,
            shiny = IsShiny,
            gender = this.gender,
            statIVs = StatIVs
        };
        return saveData;
    }

    /// <summary>
    /// Calculates the lower exp threshold for the given level, based on the growth rate.
    /// </summary>
    public int CalculateBaseExpForLevel(int level)
    {
        int exp;
        int n = level; // for ease of typing
        switch (GrowthRate)
        {
            case GrowthRate.Erratic:
                if (n < 50)
                {
                    exp = n * n * n * (100 - n) / 50;
                }
                else if (n < 68)
                {
                    exp = n * n * n * (150 - n) / 100;
                }
                else if (n < 98)
                {
                    exp = n * n * n * Mathf.FloorToInt((1911 - 10 * n) / 3) / 500;
                }
                else
                {
                    exp = n * n * n * (160 - n) / 100;
                }
                break;
            case GrowthRate.Fast:
                exp = 4 * n * n * n / 5;
                break;
            case GrowthRate.MediumFast:
                exp = n * n * n;
                break;
            case GrowthRate.MediumSlow:
                exp = (int)(1.2f * n * n * n - 15 * n * n + 100 * n - 140);
                break;
            case GrowthRate.Slow:
                exp = 5 * n * n * n / 4;
                break;
            case GrowthRate.Fluctuating:
                if (n < 15)
                {
                    exp = (n * n * n * Mathf.FloorToInt((n + 1) / 3) + 24) / 50;
                }
                else if (n < 36)
                {
                    exp = n * n * n * (n + 14) / 50;
                }
                else
                {
                    exp = n * n * n * (Mathf.FloorToInt(n / 2) + 32) / 50;
                }
                break;
            default:
                exp = 100 * n;
                break;
        }
        return exp;
    }

    /// <summary>
    /// Converts current exp to a value between 0 and 1. 
    /// <para>Represents the scale of current exp in relation to the threshold for the next level.
    /// Used for scaling of exp bars.</para>
    /// </summary>
    public float GetNormalizedExp()
    {
        int currLevelExp = CalculateBaseExpForLevel(Level);
        int nextLevelExp = CalculateBaseExpForLevel(Level + 1);

        float normalizedExp = (float)(Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

    /// <summary>
    /// Also returns a <see cref="StatChanges"/> object for displaying stat changes,
    /// and any new moves that can be learned.
    /// </summary>
    /// <param name="newMove">A reference object to contain any new moves that can be learned at the next level.</param>
    public StatChanges CheckForLevelUp(out LearnableMove newMove)
    {
        int nextLevelExp = CalculateBaseExpForLevel(Level + 1);
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

    public Evolution CheckForEvolution()
    {
        return Base.Evolutions.FirstOrDefault(e => e.RequiredLevel != -1 && e.RequiredLevel <= level);
    }

    /// <summary>
    /// Get any evolutions for the given item.
    /// </summary>
    public Evolution CheckForEvolution(ItemBase item)
    {
        return Base.Evolutions.FirstOrDefault(e => e.RequiredItem == item);
    }

    /// <summary>
    /// Perform the given evolution.
    /// <para>Restores some health and returns any <see cref="StatChanges"/>.
    /// </summary>
    public StatChanges Evolve(Evolution evolution)
    {
        _base = evolution.EvolvesInto;
        var statChanges = CalculateStats();
        SetSprites(IsShiny, gender);
        // An evolution might not have the same ability options, 
        // but normal/hidden should be preserved.
        abilityID = useHiddenAbility ? Base.HiddenAbilityID : Base.AbilityID;
        Ability = AbilityDB.Abilities[abilityID];

        // TODO: Handle any other updates 

        Mathf.Clamp(HP += statChanges.hpDiff, 0, MaxHP);

        // Forcefully override status condition
        VolatileStatus = StatusConditionsDB.Conditions[StatusConditionID.evoboost];
        VolatileStatus?.OnStart?.Invoke(this);
        AddStatusEvent($"{Base.Name} {VolatileStatus.StartMessage}");
        OnStatusChanged?.Invoke();

        return statChanges;
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

    public bool HasMove(MoveBase move)
    {
        return Moves.Count(m => m.Base == move) > 0;
    }

    private LearnableMove GetLearnableMoveForLevel()
    {
        return Base.LearnableMoves.Where(m => m.LevelLearned == level).FirstOrDefault();
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
            { Stat.Attack, Mathf.FloorToInt((2 * Base.Attack + StatIVs[Stat.Attack]) * Level / 150f) + 5 },
            { Stat.Defense, Mathf.FloorToInt((2 * Base.Defense + StatIVs[Stat.Defense]) * Level / 150f) + 5 },
            { Stat.SpAttack, Mathf.FloorToInt((2 * Base.SpAttack + StatIVs[Stat.SpAttack]) * Level / 150f) + 5 },
            { Stat.SpDefense, Mathf.FloorToInt((2 * Base.SpDef + StatIVs[Stat.SpDefense]) * Level / 150f) + 5 },
            { Stat.Speed, Mathf.FloorToInt((2 * Base.Speed + StatIVs[Stat.Speed]) * Level / 150f) + 5 }
        };

        MaxHP = Mathf.FloorToInt((2 * Base.MaxHP + StatIVs[Stat.HP]) * Level / 150f) + 10 + Level;

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

    /// <summary>
    /// Retreives the requested <see cref="Stat"/>, with any boosts applied.
    /// </summary>
    /// <returns>The current value of the <see cref="Stat"/> with boosts.</returns>
    int GetStat(Stat stat)
    {
        int value = Stats[stat];

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

    /// <summary>
    /// Try to apply all boosts.
    /// <para>Checks if a boost is at the limit, and generates a messages based on the change.</para>
    /// </summary>
    /// <param name="boosts">Boosts to apply</param>
    /// <param name="source">The <see cref="Pokemon"/> that originated the boost (usually by <see cref="Move"/>).</param>
    public void ApplyBoosts(List<StatBoost> boosts, Pokemon source)
    {
        var statsDict = boosts.ToDictionary(x => x.stat, x => x.boost);
        Ability?.OnBoost?.Invoke(statsDict, source, this);

        foreach (var statBoost in statsDict)
        {
            var stat = statBoost.Key;
            var boost = statBoost.Value;
            bool changeIsPositive = boost > 0;

            if ((changeIsPositive && boost == 6) || (!changeIsPositive && boost == -6))
            {
                string riseOrFall = changeIsPositive ? "higher" : "lower";
                AddStatusEvent(StatusEventType.Text, $"{Base.Name}'s {stat} won't go any {riseOrFall}!");
            }
            else
            {
                StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

                string riseOrFall = changeIsPositive ? "rose" : boost < 0 ? "fell" : "did not change";
                string bigChange = (Mathf.Abs(boost) >= 3) ? " severly " : (Mathf.Abs(boost) == 2) ? " harsly " : " ";
                AddStatusEvent(StatusEventType.Text, $"{Base.Name}'s {stat}{bigChange}{riseOrFall}!");
            }
        }
    }

    /// <summary>
    /// Set the given status condition that will persit between battles.
    /// </summary>
    public void SetStatus(StatusConditionID conditionID, EffectSource effectSource = EffectSource.Move)
    {
        if (Status != null)
            return;

        var canSetStatus = Ability?.OnTrySetStatus?.Invoke(conditionID, this, effectSource);
        if (!canSetStatus.Value)
            return;

        Status = StatusConditionsDB.Conditions[conditionID];
        Status?.OnStart?.Invoke(this);
        AddStatusEvent($"{Base.Name} {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }

    /// <summary>
    /// Set the given status condition that will be removed on battle's end.
    /// </summary>
    public void SetVolatileStatus(StatusConditionID conditionID, EffectSource effectSource = EffectSource.Move)
    {
        if (Status != null)
            return;

        var canSetStatus = Ability?.OnTrySetVolatileStatus?.Invoke(conditionID, this, effectSource);
        if (!canSetStatus.Value)
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

    /// <summary>
    /// Run any after-turn effects from status conditions.
    /// </summary>
    /// <param name="target"></param>
    public void OnAfterTurn(Pokemon target)
    {
        Status?.OnAfterTurn?.Invoke(this, target);
        VolatileStatus?.OnAfterTurn?.Invoke(this, target);
    }

    /// <summary>
    /// Add a status event of the given type with the given message to the queue.
    /// </summary>
    public void AddStatusEvent(StatusEventType type, string message)
    {
        StatusChanges.Enqueue(new StatusEvent(type, message));
    }

    /// <summary>
    /// Add a status event with only a message to the queue.
    /// </summary>
    /// <param name="message"></param>
    public void AddStatusEvent(string message)
    {
        AddStatusEvent(StatusEventType.Text, message);
    }

    /// <summary>
    /// Run on before move effects from status conditions
    /// </summary>
    /// <returns>Whether the <see cref="Pokemon"/> can perform its <see cref="Move"/> this turn.</returns>
    public bool OnBeforeMove()
    {
        bool canPerformMove = true;

        if (Status?.OnBeforeMove != null)
            canPerformMove = Status.OnBeforeMove.Invoke(this);

        if (VolatileStatus?.OnBeforeMove != null)
            canPerformMove = VolatileStatus.OnBeforeMove.Invoke(this);

        return canPerformMove;
    }

    /// <summary>
    /// Apply damage from the given move.
    /// <para>Takes various things into account, such as weather, crit chance, multi-hit and one-hit-KO moves, 
    /// attack and defense stats (normal vs special base on the Move), and ability effects.
    /// </summary>
    /// <returns>An object containing details of the damage dealt (was it a crit, etc.)</returns>
    public DamageDetails ApplyDamage(Move move, Pokemon attacker, float weatherModifier = 1f, int hitCount = 1)
    {
        float crit = 1f;
        // Erina's tutorial
        if (!(move.Base.CriticalBehavior == CritBehavior.NeverCrits))
        {
            if (move.Base.CriticalBehavior == CritBehavior.AlwaysCrits)
            {
                crit = 2f;
            }
            else
            {
                int critChance = 0 + ((move.Base.CriticalBehavior == CritBehavior.HighCritRatio) ? 1 : 0); //Todo: Ability, HeldItem
                if (UnityEngine.Random.value * 100f <= GlobalSettings.I.CritChances[Mathf.Clamp(critChance, 0, GlobalSettings.I.CritChances.Length - 1)])
                    crit = 2f;
            }
        }

        if (move.Base.OneHitKo.isOneHitKnockOut)
        {
            int oneHitDamage = HP;
            ReduceHP(oneHitDamage);
            return new DamageDetails()
            {
                TypeEffectiveness = 1f,
                Crit = 1f,
                // Fainted = false
            };
        }

        float type = GetTypeEffectiveness(move.Base.Type);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Crit = crit,
            WeatherModifier = weatherModifier,
            MoveHitsCount = hitCount,
            //Erina's tutorial 
            DamageDealt = 0
        };

        float modifiers = UnityEngine.Random.Range(0.85f, 1.1f) * type * crit * weatherModifier;
        float a = (2 * attacker.Level + 10) / 250f;

        float attack, defense;
        if (move.Base.MoveType == MoveType.Special)
        {
            attack = attacker.ModifySpAtk(attacker.SpAttack, this, move);
            defense = ModifySpDef(SpDefense, attacker, move);
        }
        else
        {
            attack = attacker.ModifyAtk(attacker.Attack, this, move);
            defense = ModifyDef(Defense, attacker, move);
        }

        float d = a * move.Base.Power * ((float)attack / defense);

        int damage = Mathf.FloorToInt(d * modifiers);

        ReduceHP(damage);
        //Erina's tutorial
        damageDetails.DamageDealt = damage;

        return damageDetails;
    }

    //Erina's tutorial
    public void TakeRecoilDamage(int damage)
    {
        if (damage < 1)
            damage = 1;
        ReduceHP(damage, true);
        AddStatusEvent(StatusEventType.Text, $"{Base.Name} was damaged by the recoil!");
    }

    public float GetTypeEffectiveness(PokemonType type)
    {
        return TypeChart.GetEffectiveness(type, Base.Type1)
            * TypeChart.GetEffectiveness(type, Base.Type2);
    }

    public bool IsOfType(PokemonType type)
    {
        return type == Base.Type1 || type == Base.Type2;
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

    public float ModifyAtk(float atk, Pokemon defender, Move move)
    {
        if (Ability?.OnModifyAtk != null)
        {
            return Ability.OnModifyAtk(atk, this, defender, move);
        }
        return atk;
    }

    public float ModifySpAtk(float spAtk, Pokemon defender, Move move)
    {
        if (Ability?.OnModifySpAtk != null)
        {
            return Ability.OnModifySpAtk(spAtk, this, defender, move);
        }
        return spAtk;
    }

    public float ModifyDef(float def, Pokemon attacker, Move move)
    {
        if (Ability?.OnModifySpDef != null)
        {
            return Ability.OnModifyDef(def, attacker, this, move);
        }
        return def;
    }

    public float ModifySpDef(float spDef, Pokemon attacker, Move move)
    {
        if (Ability?.OnModifySpDef != null)
        {
            return Ability.OnModifySpDef(spDef, attacker, this, move);
        }
        return spDef;
    }

    public float ModifySpd(float spd, Pokemon defender, Move move)
    {
        if (Ability?.OnModifySpd != null)
        {
            return Ability.OnModifySpd(spd, this, defender, move);
        }
        return spd;
    }

    public float ModifyAcc(float acc, Pokemon defender, Move move)
    {
        if (Ability?.OnModifyAcc != null)
        {
            return Ability.OnModifyAcc(acc, this, defender, move);
        }
        return acc;
    }

}

public class DamageDetails
{
    public float Crit { get; set; }
    public float TypeEffectiveness { get; set; }
    public float WeatherModifier { get; set; }
    public int MoveHitsCount { get; set; }
    //Erina's tutorial
    public int DamageDealt { get; set; }
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
    public AbilityID ability;
    public bool useHiddenAbility;
    public StatusConditionID? status;
    public List<MoveSaveData> moves;
    public GrowthRate growthRate;
    public bool shiny;
    public PokemonGender gender;
    public Dictionary<Stat, int> statIVs;
}
