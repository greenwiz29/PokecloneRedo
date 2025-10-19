using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GDEUtils.StateMachine;
using UnityEngine;
using UnityEngine.UI;

public enum BattleTrigger { LongGrass, Water, Cave }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] List<BattleUnit> playerUnits, enemyUnits;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage, trainerImage;
    [SerializeField] GameObject pokeballSprite;
    [SerializeField] StatChangesUI statChangesUI;

    [Header("Background Images")]
    [SerializeField] Image backgroundImage;
    [SerializeField] Sprite longGrassBG;
    [SerializeField] Sprite waterBG;
    [SerializeField] Sprite caveBG;

    PokemonParty playerParty, trainerParty;
    Pokemon wildPokemon;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;
    public int EscapeAttempts { get; set; }
    int unitCount = 1;
    int selectedUnit = 0;
    List<BattleAction> battleActions;

    /// <summary>
    /// Event to indicate the end of a battle.
    /// The bool parameter indicates if the player won.
    /// </summary>
    public event Action<bool> OnBattleOver;

    BattleTrigger trigger;
    public StateMachine<BattleSystem> StateMachine { get; private set; }
    public BattleDialogBox DialogBox => dialogBox;
    public List<BattleUnit> PlayerUnits => playerUnits;
    public List<BattleUnit> EnemyUnits => enemyUnits;
    public bool IsBattleOver { get; private set; }
    public bool IsTrainerBattle => isTrainerBattle;
    public PokemonParty PlayerParty => playerParty;
    public PokemonParty TrainerParty => trainerParty;
    public TrainerController Trainer => trainer;
    public BattleUnit SelectedUnit => playerUnits[selectedUnit];

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon, BattleTrigger trigger = BattleTrigger.LongGrass)
    {
        this.trigger = trigger;
        this.unitCount = 1;

        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        isTrainerBattle = false;
        player = playerParty.GetComponent<PlayerController>();

        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty, BattleTrigger trigger = BattleTrigger.LongGrass, int unitCount = 1)
    {
        this.trigger = trigger;
        this.unitCount = unitCount;

        this.playerParty = playerParty;
        this.trainerParty = trainerParty;
        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        StateMachine = new StateMachine<BattleSystem>(this);
        battleActions = new List<BattleAction>();
        EscapeAttempts = 0;
        selectedUnit = 0;
        IsBattleOver = false;

        for (int i = 0; i < unitCount; i++)
        {
            playerUnits[i].Clear();
            enemyUnits[i].Clear();
        }

        SetBackground(trigger);

        if (!isTrainerBattle)
        {
            enemyUnits[0].gameObject.SetActive(true);
            enemyUnits[0].Setup(wildPokemon);
            yield return dialogBox.TypeDialog($"A wild {enemyUnits[0].Pokemon.Name} appeared!");
        }
        else
        {
            // Trainer battle
            for (int i = 0; i < unitCount; i++)
            {
                playerUnits[0].gameObject.SetActive(false);
                enemyUnits[0].gameObject.SetActive(false);
            }

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);

            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle!");

            trainerImage.gameObject.SetActive(false);

            var enemyPokemon = trainerParty.GetHealthyPokemon(unitCount);
            for (int i = 0; i < unitCount; i++)
            {
                enemyUnits[i].gameObject.SetActive(true);
                enemyUnits[i].Setup(enemyPokemon[i]);
            }
            var enemyPokemonNames = String.Join(" and ", enemyPokemon.Select(p => p.Name));
            yield return dialogBox.TypeDialog($"{trainer.Name} sent out {enemyPokemonNames}!");

            playerImage.gameObject.SetActive(false);
        }

        var playerPokemon = playerParty.GetHealthyPokemon(unitCount);
        for (int i = 0; i < unitCount; i++)
        {
            playerUnits[i].gameObject.SetActive(true);
            playerUnits[i].Setup(playerPokemon[i]);
        }
        var pokemonNames = String.Join(" and ", playerPokemon.Select(p => p.Name));

        yield return dialogBox.TypeDialog($"Go, {pokemonNames}!");

        partyScreen.Init();

        StateMachine.ChangeState(ActionSelectionState.I);
    }

    private void SetBackground(BattleTrigger trigger)
    {
        switch (trigger)
        {
            case BattleTrigger.LongGrass:
                backgroundImage.sprite = longGrassBG;
                break;
            case BattleTrigger.Water:
                backgroundImage.sprite = waterBG;
                break;
            case BattleTrigger.Cave:
                backgroundImage.sprite = caveBG;
                break;
        }
    }

    public void AddBattleAction(BattleAction action)
    {
        action.User = SelectedUnit;
        battleActions.Add(action);

        if (battleActions.Count == unitCount)
        {
            // Add enemy actions
            foreach (var enemy in enemyUnits)
            {
                battleActions.Add(new BattleAction()
                {
                    Type = BattleActionType.Move,
                    SelectedMove = enemy.Pokemon.GetRandomMove(),
                    User = enemy,
                    Target = playerUnits[UnityEngine.Random.Range(0, playerUnits.Count)]
                });
            }

            // Sort actions by priority and speed
            battleActions = battleActions.OrderByDescending(a => a.Priority).ThenByDescending(a => a.User.Pokemon.Speed).ToList();

            // Run turns
            RunTurnState.I.BattleActions = battleActions;
            StateMachine.ChangeState(RunTurnState.I);
        }
        else
        {
            ++selectedUnit;
            // Select another action
            StateMachine.ChangeState(ActionSelectionState.I);
        }
    }

    /// <summary>
    /// Should be called at the end of each turn
    /// </summary>
    public void ClearBattleActions()
    {
        battleActions.Clear();
        selectedUnit = 0;
    }

    public void HandleUpdate()
    {
        StateMachine.Execute();
    }

    public IEnumerator SwitchPokemon(Pokemon newPokemon, BattleUnit unitToSwitch)
    {
        dialogBox.EnableDialogText(true);
        if (unitToSwitch.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back, {unitToSwitch.Pokemon.Name}");
            unitToSwitch.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        // yield return SendOutPlayerPokemon(newPokemon);
        unitToSwitch.gameObject.SetActive(true);
        unitToSwitch.Setup(newPokemon);
        yield return dialogBox.TypeDialog($"Go, {newPokemon.Name}!");
    }

    public IEnumerator SendNextTrainerPokemon(BattleUnit unitToSwitch)
    {
        var next = trainerParty.GetHealthyPokemon();

        unitToSwitch.Setup(next);
        yield return dialogBox.TypeDialog($"{trainer.Name} sent out {next.Name}");
    }

    public IEnumerator ApplyExpGain(BattleUnit sourceUnit, BattleUnit targetUnit, bool unitFainted, DamageDetails damageDetails = null, bool shareExp = true)
    {
        // Exp gain
        int expYield = targetUnit.Pokemon.Base.ExpYield;
        int enemyLevel = targetUnit.Pokemon.Level;
        float trainerBonus = IsTrainerBattle ? 1.5f : 1f;

        // Base exp gain
        int expGain = Mathf.FloorToInt(expYield * enemyLevel * trainerBonus / 7);
        if (shareExp)
            expGain /= unitCount;

        // Adjust expGain depending on per-move or fainted
        if (!unitFainted)
            expGain = (int)Mathf.Clamp(expGain / 50f, 1, float.MaxValue);
        else
            expGain = (int)(expGain * 0.8);

        // apply modifiers
        if (damageDetails != null)
        {
            expGain = (int)(expGain * damageDetails.Crit * damageDetails.TypeEffectiveness);
        }

        var pokemon = sourceUnit.Pokemon;
        var playerHud = sourceUnit.HUD;
        pokemon.Exp += expGain;
        yield return dialogBox.TypeDialog($"{pokemon.Name} gained {expGain} exp.");
        yield return playerHud.UpdateEXP(false);
        yield return HandleLevelUp(sourceUnit, playerHud);
    }

    private IEnumerator HandleLevelUp(BattleUnit sourceUnit, BattleHUD playerHud)
    {
        var pokemon = sourceUnit.Pokemon;
        // Check level up
        var leveledUp = pokemon.CheckForLevelUp(out LearnableMove newMove);
        while (leveledUp != null)
        {
            playerHud.SetLevel();
            if (sourceUnit.IsPlayerUnit)
                yield return dialogBox.TypeDialog($"{pokemon.Name} grew to level {pokemon.Level}!");

            // Show stat changes dialog
            var statChanges = leveledUp;

            var evolution = pokemon.CheckForEvolution();
            if (evolution != null)
            {
                var changes = new StatChangesWrapper();
                yield return EvolutionState.I.Evolve(pokemon, evolution, changes);
                sourceUnit.Setup(pokemon);
                partyScreen.SetPartyData();
                statChanges = changes.Changes;
            }

            if (sourceUnit.IsPlayerUnit)
            {
                statChangesUI.gameObject.SetActive(true);
                statChangesUI.SetStatChanges(statChanges);

                yield return new WaitForEndOfFrame();
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

                statChangesUI.SetStats(pokemon);

                yield return new WaitForEndOfFrame();
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

                statChangesUI.gameObject.SetActive(false);
            }

            // Try to learn a new Move
            if (newMove != null)
            {
                if (pokemon.TryLearnMove(newMove.Base))
                {
                    if (sourceUnit.IsPlayerUnit)
                    {
                        yield return dialogBox.TypeDialog($"{pokemon.Name} learned {newMove.Base.Name}");
                        dialogBox.SetMoveNames(pokemon.Moves);
                    }
                }
                else
                {
                    var moveToLearn = newMove.Base;
                    if (sourceUnit.IsPlayerUnit)
                    {
                        yield return dialogBox.TypeDialog($"{pokemon.Name} is trying to learn {newMove.Base.Name}");
                        yield return dialogBox.TypeDialog($"But it cannot know more than {Pokemon.maxMoves} moves at once.");
                        yield return dialogBox.TypeDialog($"Choose a move to forget.");
                        MoveToForgetState.I.NewMove = newMove.Base;
                        MoveToForgetState.I.CurrentMoves = pokemon.Moves.Select(m => m.Base).ToList();
                        yield return GameController.I.stateMachine.PushAndWait(MoveToForgetState.I);

                        int moveIndex = MoveToForgetState.I.Selection;
                        if (moveIndex == -1 || moveIndex == Pokemon.maxMoves)
                        {
                            // new move was selected, or player canceled out.
                            // TODO: prompt if new move should be abandoned
                            yield return dialogBox.TypeDialog($"{pokemon.Name} did not learn {moveToLearn.Name}");
                        }
                        else
                        {
                            // Forget selected move and learn new move
                            yield return dialogBox.TypeDialog($"{pokemon.Name} forgot {pokemon.Moves[moveIndex].Base.Name} and learned {moveToLearn.Name}");

                            pokemon.Moves[moveIndex] = new Move(moveToLearn);
                        }
                    }
                    else
                    {
                        // Enemy pokemon just forget their first move
                        pokemon.Moves[0] = new Move(moveToLearn);
                    }
                    newMove = null;
                }
            }
            yield return playerHud.WaitForHPUpdate();
            yield return playerHud.UpdateEXP(true);

            leveledUp = pokemon.CheckForLevelUp(out newMove);
        }
    }

    public void BattleOver(bool playerWon)
    {
        IsBattleOver = true;
        playerParty.Party.ForEach(p => p.OnBattleOver());
        partyScreen.Cleanup();

        for (int i = 0; i < unitCount; i++)
        {
            playerUnits[i].Clear();
            enemyUnits[i].Clear();
        }

        OnBattleOver(playerWon);
    }

    public IEnumerator ThrowPokeball(PokeballItem pokeballItem)
    {
        dialogBox.EnableDialogText(true);
        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't steal another trainer's pokemon!");
            yield break;
        }
        var enemyUnit = enemyUnits[0];
        var playerUnit = playerUnits[0];

        yield return dialogBox.TypeDialog($"{player.Name} threw a {pokeballItem.Name.ToLower()}!");

        var pokeballObject = Instantiate(
            pokeballSprite,
            playerUnit.transform.position - new Vector3(2, 0),
            Quaternion.identity
        );

        var pokeball = pokeballObject.GetComponent<SpriteRenderer>();
        pokeball.sprite = pokeballItem.Icon;

        yield return PlayThrowAnimation(pokeball);

        if (pokeballItem.IsMaster)
        {
            yield return CatchPokemon(pokeball);
            yield break;
        }
        int numShakes = TryToCatchPokemon(enemyUnit.Pokemon, pokeballItem);
        for (int i = 0; i < Mathf.Min(numShakes, 3); i++)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball
                .transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f)
                .WaitForCompletion();
        }

        if (numShakes == 4)
        {
            yield return CatchPokemon(pokeball);
        }
        else
        {
            yield return new WaitForSeconds(1f);
            pokeball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();

            switch (numShakes)
            {
                case 3:
                    yield return dialogBox.TypeDialog($"Arrgh! So close!");
                    break;
                case 2:
                    yield return dialogBox.TypeDialog($"Almost!");
                    break;
                case 1:
                    yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Name} broke free!");
                    break;
            }

            Destroy(pokeball);
        }
    }

    private IEnumerator PlayThrowAnimation(SpriteRenderer pokeball)
    {
        var enemyUnit = enemyUnits[0];

        // Animations
        yield return pokeball
            .transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f)
            .WaitForCompletion();

        yield return enemyUnit.PlayCaptureAnimation();

        yield return pokeball
            .transform.DOMoveY(enemyUnit.transform.position.y - 1.75f, 0.5f)
            .WaitForCompletion();
        // Aiming for bouncing, but it's not great.
        // yield return pokeball.transform.DOJump(enemyUnit.transform.position - new Vector3(0, 2), 1f, 3, 0.5f).WaitForCompletion();
    }

    private IEnumerator CatchPokemon(SpriteRenderer pokeball)
    {
        var enemyUnit = enemyUnits[0];
        // Pokemon caught!
        yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Name} was caught!");
        yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

        playerParty.AddPokemon(enemyUnit.Pokemon);

        foreach (var unit in playerUnits)
        {
            yield return ApplyExpGain(unit, enemyUnit, true);
        }

        Destroy(pokeball);
        BattleOver(true);
    }

    int TryToCatchPokemon(Pokemon pokemon, PokeballItem pokeball)
    {
        float a =
            (3 * pokemon.MaxHP - 2 * pokemon.HP)
            * pokemon.Base.CatchRate
            * pokeball.CatchRateModifier
            * ConditionsDB.GetStatusBonus(pokemon.Status)
            / (3 * pokemon.MaxHP);

        if (a >= 255)
            return 4; // catch

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
                break;
            ++shakeCount;
        }

        return shakeCount;
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        var style = new GUIStyle()
        {
            fontSize = 24,
            alignment = TextAnchor.UpperRight
        };
        GUILayout.BeginArea(new Rect(0, 0, Screen.width - 5, Screen.height));
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("BATTLE STATE STACK", style);
        GUILayout.EndHorizontal();
        foreach (var state in StateMachine.StateStack)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(state.GetType().ToString(), style);
            GUILayout.EndHorizontal();
        }
        GUILayout.EndArea();
    }
#endif
}
