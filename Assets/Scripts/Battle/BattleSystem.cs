using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using GDEUtils.StateMachine;
using UnityEngine;
using UnityEngine.UI;

public enum BattleAction { Move, Switch, Item, Run, }

public enum BattleStateEnum { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, BattleOver, AboutToUse, MoveToForget, Bag }

public enum BattleTrigger { LongGrass, Water, Cave }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit, enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] MoveBase struggle;
    [SerializeField] Image playerImage, trainerImage;
    [SerializeField] GameObject pokeballSprite;
    [SerializeField] MoveToForgetUI moveToForgetUI;
    [SerializeField] StatChangesUI statChangesUI;
    [SerializeField] InventoryUI inventoryUI;

    [Header("Background Images")]
    [SerializeField] Image backgroundImage;
    [SerializeField] Sprite longGrassBG;
    [SerializeField] Sprite waterBG;
    [SerializeField] Sprite caveBG;

    BattleStateEnum state;
    int currentAction, currentMove;
    bool aboutToUseChoice;

    PokemonParty playerParty, trainerParty;
    Pokemon wildPokemon;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;
    private int escapeAttempts;
    MoveBase moveToLearn;
    Action onBagBack;
    Action<ItemBase> onItemUsed;

    /// <summary>
    /// Event to indicate the end of a battle.
    /// The bool parameter indicates if the player won.
    /// </summary>
    public event Action<bool> OnBattleOver;

    BattleTrigger trigger;
    public StateMachine<BattleSystem> StateMachine { get; private set; }
    public BattleDialogBox DialogBox => dialogBox;
    public BattleUnit PlayerUnit => playerUnit;
    public BattleUnit EnemyUnit => enemyUnit;

    void Awake()
    {
        onBagBack = () =>
        {
            inventoryUI.gameObject.SetActive(false);
            state = BattleStateEnum.ActionSelection;
        };

        onItemUsed = (usedItem) =>
        {
            StartCoroutine(OnItemUsed(usedItem));
        };
    }

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon, BattleTrigger trigger = BattleTrigger.LongGrass)
    {
        this.trigger = trigger;

        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        isTrainerBattle = false;
        player = playerParty.GetComponent<PlayerController>();

        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty, BattleTrigger trigger = BattleTrigger.LongGrass)
    {
        this.trigger = trigger;

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
        escapeAttempts = 0;

        playerUnit.Clear();
        enemyUnit.Clear();

        SetBackground(trigger);

        if (!isTrainerBattle)
        {
            enemyUnit.gameObject.SetActive(true);
            enemyUnit.Setup(wildPokemon);
            yield return dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Name} appeared!");
        }
        else
        {
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);

            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle!");

            trainerImage.gameObject.SetActive(false);

            var enemyPokemon = trainerParty.GetHealthyPokemon();
            enemyUnit.gameObject.SetActive(true);
            enemyUnit.Setup(enemyPokemon);
            yield return dialogBox.TypeDialog($"{trainer.Name} sent out {enemyPokemon.Name}!");

            playerImage.gameObject.SetActive(false);
        }

        yield return SendOutPlayerPokemon(playerParty.GetHealthyPokemon());

        partyScreen.Init();

        StateMachine.ChangeState(ActionSelectionState.I);
    }

    private IEnumerator SendOutPlayerPokemon(Pokemon pokemon)
    {
        playerUnit.gameObject.SetActive(true);
        playerUnit.Setup(pokemon);

        dialogBox.SetMoveNames(pokemon.Moves);

        yield return dialogBox.TypeDialog($"Go, {pokemon.Name}!");
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

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleStateEnum.RunningTurn;
        switch (playerAction)
        {
            case BattleAction.Move:
                playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
                enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();
                bool playerGoesFirst = CheckTurnOrder();

                var firstUnit = playerGoesFirst ? playerUnit : enemyUnit;
                var secondUnit = playerGoesFirst ? enemyUnit : playerUnit;
                var secondPokemon = secondUnit.Pokemon;

                yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(firstUnit);
                if (state == BattleStateEnum.BattleOver)
                    yield break;

                if (secondPokemon.HP > 0)
                {
                    yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                    yield return RunAfterTurn(secondUnit);
                    if (state == BattleStateEnum.BattleOver)
                        yield break;
                }
                break;

            case BattleAction.Switch:
                state = BattleStateEnum.Busy;
                yield return SwitchPokemon(partyScreen.SelectedPokemon);

                yield return RunEnemyTurn();
                break;

            case BattleAction.Item:
                dialogBox.EnableActionSelector(false);
                yield return RunEnemyTurn();
                break;

            case BattleAction.Run:
                yield return TryToRun();

                yield return RunEnemyTurn();
                break;
        }

        if (state != BattleStateEnum.BattleOver)
        {
            ActionSelection();
        }
    }

    private IEnumerator RunEnemyTurn()
    {
        var enemyMove = enemyUnit.Pokemon.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, enemyMove);
        yield return RunAfterTurn(enemyUnit);
        if (state == BattleStateEnum.BattleOver)
            yield break;
    }

    private bool CheckTurnOrder()
    {
        int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
        int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;

        bool playerGoesFirst;

        if (enemyMovePriority > playerMovePriority)
            playerGoesFirst = false;
        else if (playerMovePriority > enemyMovePriority)
            playerGoesFirst = true;
        else
            playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;

        return playerGoesFirst;
    }

    private IEnumerator RunMove(BattleUnit source, BattleUnit target, Move move)
    {
        if (!source.Pokemon.OnBeforeMove())
        {
            yield return ShowStatusChanges(source.Pokemon);
            yield return source.HUD.WaitForHPUpdate();
            yield break;
        }
        yield return ShowStatusChanges(source.Pokemon);

        var enemy = source.IsPlayerUnit ? "" : "Enemy ";

        if (move != null)
        {
            move.PP--;
        }
        else
        {
            move = new Move(struggle);
        }

        yield return dialogBox.TypeDialog($"{enemy}{source.Pokemon.Name} used {move.Base.Name}");

        if (!CheckIfMoveHits(move, source.Pokemon, target.Pokemon))
        {
            yield return dialogBox.TypeDialog("... but it missed.");
        }
        else
        {
            DamageDetails damageDetails = null;

            source.PlayAttackAnimation();
            yield return new WaitForSeconds(0.75f);

            target.PlayHitAnimation();

            if (move.Base.MoveType == MoveType.Status)
            {
                yield return RunMoveEffects(source, target, move.Base.Effects, move.Base.Target);
            }
            else
            {
                damageDetails = target.Pokemon.ApplyDamage(move, source.Pokemon);

                yield return target.HUD.WaitForHPUpdate();

                yield return ShowDamageDetails(damageDetails);
            }

            var secondaries = move.Base.SecondaryEffects;
            if (secondaries != null && secondaries.Count > 0 && target.Pokemon.HP <= 0)
            {
                foreach (var secEffect in secondaries)
                {
                    var rand = UnityEngine.Random.Range(1, 101);
                    if (rand <= secEffect.Chance)
                    {
                        yield return RunMoveEffects(source, target, secEffect, secEffect.Target);
                    }
                }
            }

            if (target.Pokemon.HP <= 0)
            {
                yield return HandlePokemonFainted(target, damageDetails);
            }
            else
            {
                yield return ApplyExpGain(enemyUnit, false, damageDetails);
            }
        }
    }

    private IEnumerator RunMoveEffects(
        BattleUnit source,
        BattleUnit target,
        MoveEffects effects,
        MoveTarget moveTarget
    )
    {
        // Stat boosts
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
            {
                source.Pokemon.ApplyBoosts(effects.Boosts);
            }
            else
            {
                target.Pokemon.ApplyBoosts(effects.Boosts);
            }
        }
        // Conditions
        if (effects.Status != ConditionID.none)
        {
            target.Pokemon.SetStatus(effects.Status);
        }
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.Pokemon.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source.Pokemon);
        yield return ShowStatusChanges(target.Pokemon);
    }

    IEnumerator RunAfterTurn(BattleUnit source)
    {
        if (state == BattleStateEnum.BattleOver)
            yield break;
        yield return new WaitUntil(() => state == BattleStateEnum.RunningTurn);

        source.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(source.Pokemon);
        yield return source.HUD.WaitForHPUpdate();

        // Check if source pokemon fainted after status effect
        if (source.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(source);
            yield return new WaitUntil(() => state == BattleStateEnum.RunningTurn);
        }
    }

    bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
    {
        if (move.Base.AlwaysHits)
            return true;

        float moveAccuracy = move.Base.Accuracy;
        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };
        if (accuracy > 0)
        {
            moveAccuracy *= boostValues[accuracy];
        }
        else if (accuracy < 0)
        {
            moveAccuracy /= boostValues[-accuracy];
        }
        if (evasion > 0)
        {
            moveAccuracy /= boostValues[evasion];
        }
        else if (evasion < 0)
        {
            moveAccuracy *= boostValues[-evasion];
        }

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var next = playerParty.GetHealthyPokemon();
            if (next == null)
            {
                BattleOver(false);
            }
            else
            {
                OpenPartyScreen();
            }
        }
        else
        {
            if (!isTrainerBattle)
                BattleOver(true);
            else
            {
                var next = trainerParty.GetHealthyPokemon();
                if (next == null)
                {
                    BattleOver(true);
                }
                else
                {
                    StartCoroutine(AboutToUse(next));
                }
            }
        }
    }

    private IEnumerator HandlePokemonFainted(BattleUnit faintedUnit, DamageDetails damageDetails = null)
    {
        string enemy;
        // target unit fainted
        faintedUnit.PlayFaintAnimation();
        enemy = !faintedUnit.IsPlayerUnit ? "Enemy " : "";
        yield return dialogBox.TypeDialog($"{enemy}{faintedUnit.Pokemon.Name} fainted.");
        yield return new WaitForSeconds(1f);

        if (!faintedUnit.IsPlayerUnit)
        {
            yield return ApplyExpGain(faintedUnit, true, damageDetails);
        }

        CheckForBattleOver(faintedUnit);
    }

    private IEnumerator ApplyExpGain(BattleUnit targetUnit, bool unitFainted, DamageDetails damageDetails = null)
    {
        // Exp gain
        int expYield = targetUnit.Pokemon.Base.ExpYield;
        int enemyLevel = targetUnit.Pokemon.Level;
        float trainerBonus = isTrainerBattle ? 1.5f : 1f;

        // Base exp gain
        int expGain = Mathf.FloorToInt(expYield * enemyLevel * trainerBonus / 7);

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

        var pokemon = playerUnit.Pokemon;
        var playerHud = playerUnit.HUD;
        pokemon.Exp += expGain;
        yield return dialogBox.TypeDialog($"{pokemon.Name} gained {expGain} exp.");
        yield return playerHud.UpdateEXP(false);
        yield return HandleLevelUp(pokemon, playerHud);
    }

    private IEnumerator HandleLevelUp(Pokemon pokemon, BattleHUD playerHud)
    {
        // Check level up
        var leveledUp = pokemon.CheckForLevelUp(out LearnableMove newMove);
        while (leveledUp != null)
        {
            playerHud.SetLevel();
            yield return dialogBox.TypeDialog($"{pokemon.Name} grew to level {pokemon.Level}!");

            // Show stat changes dialog
            var statChanges = leveledUp;

            var evolution = pokemon.CheckForEvolution();
            if (evolution != null)
            {
                var changes = new StatChangesWrapper();
                yield return EvolutionManager.I.Evolve(pokemon, evolution, changes);
                playerUnit.Setup(pokemon);
                partyScreen.SetPartyData();
                statChanges = changes.Changes;
            }

            statChangesUI.gameObject.SetActive(true);
            statChangesUI.SetStatChanges(statChanges);

            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

            statChangesUI.SetStats(pokemon);

            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

            statChangesUI.gameObject.SetActive(false);

            // Try to learn a new Move
            if (newMove != null)
            {
                if (pokemon.TryLearnMove(newMove.Base))
                {
                    yield return dialogBox.TypeDialog($"{pokemon.Name} learned {newMove.Base.Name}");
                    dialogBox.SetMoveNames(pokemon.Moves);
                }
                else
                {
                    yield return dialogBox.TypeDialog($"{pokemon.Name} is trying to learn {newMove.Base.Name}");
                    yield return dialogBox.TypeDialog($"But it cannot know more than {Pokemon.maxMoves} moves at once.");
                    yield return ChooseMoveToForget(pokemon, newMove.Base);
                    yield return new WaitUntil(() => state != BattleStateEnum.MoveToForget);
                }
                newMove = null;
            }
            yield return playerHud.WaitForHPUpdate();
            yield return playerHud.UpdateEXP(true);

            leveledUp = pokemon.CheckForLevelUp(out newMove);
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails details)
    {
        if (details.Crit > 1f)
        {
            yield return dialogBox.TypeDialog($"A critical hit!");
        }

        if (details.TypeEffectiveness > 2f)
        {
            yield return dialogBox.TypeDialog($"It's amazingly effective!");
        }
        else if (details.TypeEffectiveness > 1f)
        {
            yield return dialogBox.TypeDialog($"It's really effective!");
        }
        else if (details.TypeEffectiveness == 0.0f)
        {
            yield return dialogBox.TypeDialog("It has no effect.");
        }
        else if (details.TypeEffectiveness < 1f)
        {
            yield return dialogBox.TypeDialog("It's not very effective...");
        }
    }

    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();

            yield return dialogBox.TypeDialog(message);
        }
    }

    public void HandleUpdate()
    {
        StateMachine.Execute();

        switch (state)
        {
            case BattleStateEnum.PartyScreen:
                HandlePartyScreenSelection();
                break;
            case BattleStateEnum.AboutToUse:
                HandleAboutToUse();
                break;
            case BattleStateEnum.MoveToForget:
                // moveToForgetUI.HandleUpdate((moveIndex) =>
                // {
                //     var pokemon = playerUnit.Pokemon;
                //     moveToForgetUI.gameObject.SetActive(false);
                //     dialogBox.EnableMoveDetails(false);
                //     if (moveIndex == Pokemon.maxMoves)
                //     {
                //         // new move was selected
                //         // TODO: prompt if new move should be abandoned
                //         StartCoroutine(dialogBox.TypeDialog($"{pokemon.Name} did not learn {moveToLearn.Name}"));
                //     }
                //     else
                //     {
                //         // Forget selected move and learn new move
                //         StartCoroutine(dialogBox.TypeDialog($"{pokemon.Name} forgot {pokemon.Moves[moveIndex].Base.Name} and learned {moveToLearn.Name}"));

                //         pokemon.Moves[moveIndex] = new Move(moveToLearn);
                //         dialogBox.SetMoveNames(pokemon.Moves);
                //     }

                //     moveToLearn = null;
                //     state = BattleState.RunningTurn;
                // });
                break;
            case BattleStateEnum.Bag:
                // inventoryUI.HandleUpdate(onBagBack, onItemUsed);
                break;
        }
    }

    private void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var move = playerUnit.Pokemon.Moves[currentMove];
            if (move.PP <= 0)
                return;

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            ActionSelection();
        }
    }

    private void HandlePartyScreenSelection()
    {
        Action onSelected = () =>
        {
            var selectedMember = partyScreen.SelectedPokemon;
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText($"You can't send out a fainted pokemon");
                return;
            }
            if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText($"{selectedMember.Base.Name} is already out.");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            // player's previous 'mon fainted
            // if (partyScreen.CalledFrom == BattleState.ActionSelection)
            {
                StartCoroutine(RunTurns(BattleAction.Switch));
            }
            // else
            {
                // switched from menu
                state = BattleStateEnum.Busy;
                // bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;
                // StartCoroutine(SwitchPokemon(selectedMember, isTrainerAboutToUse));
            }
            // partyScreen.CalledFrom = null;
        };
        Action onBack = () =>
        {
            if (playerUnit.Pokemon.HP <= 0)
            {
                partyScreen.SetMessageText("You must choose a pokemon to continue");
                return;
            }

            // if (partyScreen.CalledFrom == BattleState.AboutToUse)
            {
                StartCoroutine(SendNextTrainerPokemon());
            }
            // else
            ActionSelection();

            // partyScreen.CalledFrom = null;
        };

        // partyScreen.HandleUpdate(onSelected, onBack);
    }

    private void HandleAboutToUse()
    {
        bool prevChoice = aboutToUseChoice;
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            aboutToUseChoice = !aboutToUseChoice;
        }

        if (aboutToUseChoice != prevChoice)
            dialogBox.UpdateChoiceBoxSelection(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // change 'mon
            dialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice)
            {
                OpenPartyScreen();
            }
            else
                StartCoroutine(SendNextTrainerPokemon());
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            dialogBox.EnableChoiceBox(false);
            // cancel choice, keep current 'mon out
            // Send out next Pokemon
            StartCoroutine(SendNextTrainerPokemon());
        }
    }

    private void OpenBag()
    {
        state = BattleStateEnum.Bag;
        inventoryUI.gameObject.SetActive(true);
    }

    private void OpenPartyScreen()
    {
        // partyScreen.CalledFrom = state;
        state = BattleStateEnum.PartyScreen;
        partyScreen.SetPartyData();
        partyScreen.gameObject.SetActive(true);
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon, bool isTrainerAboutToUse = false)
    {
        if (playerUnit.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back, {playerUnit.Pokemon.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        yield return SendOutPlayerPokemon(newPokemon);

        if (isTrainerAboutToUse)
        {
            yield return SendNextTrainerPokemon();
        }
        else
        {
            state = BattleStateEnum.RunningTurn;
        }
    }

    IEnumerator SendNextTrainerPokemon()
    {
        state = BattleStateEnum.Busy;
        var next = trainerParty.GetHealthyPokemon();

        enemyUnit.Setup(next);
        yield return dialogBox.TypeDialog($"{trainer.Name} sent out {next.Name}");

        state = BattleStateEnum.RunningTurn;
    }

    private void MoveSelection()
    {
        state = BattleStateEnum.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);
    }

    private void ActionSelection()
    {
        state = BattleStateEnum.ActionSelection;
        dialogBox.EnableDialogText(true);
        dialogBox.SetDialog("Choose an action.");
        dialogBox.EnableActionSelector(true);
        dialogBox.EnableMoveSelector(false);
        partyScreen.gameObject.SetActive(false);
        dialogBox.UpdateActionSelection(currentAction);
    }

    IEnumerator AboutToUse(Pokemon newPokemon)
    {
        state = BattleStateEnum.Busy;
        yield return dialogBox.TypeDialog(
            $"{trainer.Name} is about to use {newPokemon.Name}. Do you want to switch pokemon?"
        );

        state = BattleStateEnum.AboutToUse;
        dialogBox.EnableChoiceBox(true);
        dialogBox.UpdateChoiceBoxSelection(aboutToUseChoice);
    }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = BattleStateEnum.Busy;

        moveToLearn = newMove;
        yield return dialogBox.TypeDialog($"Choose a move to forget.");
        moveToForgetUI.gameObject.SetActive(true);
        dialogBox.EnableMoveDetails(true);
        moveToForgetUI.SetMoveData(pokemon.Moves.Select(m => m.Base).ToList(), newMove);

        state = BattleStateEnum.MoveToForget;
    }

    private void UpdateMoveDetails(int currentMove)
    {
        if (currentMove < playerUnit.Pokemon.Moves.Count)
            dialogBox.UpdateMoveDetails(playerUnit.Pokemon.Moves[currentMove]);
        else
            dialogBox.UpdateMoveDetails(new Move(moveToLearn));

    }

    IEnumerator TryToRun()
    {
        state = BattleStateEnum.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't run from trainer battles!");
            state = BattleStateEnum.RunningTurn;
            yield break;
        }

        escapeAttempts++;

        var playerSpeed = playerUnit.Pokemon.Speed;
        var enemySpeed = enemyUnit.Pokemon.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Ran away safely!");
            BattleOver(true);
        }
        else
        {
            float f = playerSpeed * 128 / enemySpeed + 30 * escapeAttempts;
            f %= 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Ran away safely!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"Can't escape!");
                state = BattleStateEnum.RunningTurn;
            }
        }
    }

    void BattleOver(bool playerWon)
    {
        state = BattleStateEnum.BattleOver;
        playerParty.Party.ForEach(p => p.OnBattleOver());
        partyScreen.Cleanup();
        playerUnit.Clear();
        enemyUnit.Clear();
        OnBattleOver(playerWon);
    }

    private IEnumerator OnItemUsed(ItemBase usedItem)
    {
        state = BattleStateEnum.Busy;
        inventoryUI.gameObject.SetActive(false);

        // Check if usedItem is a pokeball
        if (usedItem is PokeballItem)
        {
            yield return ThrowPokeball(usedItem as PokeballItem);
        }

        yield return RunTurns(BattleAction.Item);
    }

    IEnumerator ThrowPokeball(PokeballItem pokeballItem)
    {
        state = BattleStateEnum.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't steal another trainer's pokemon!");
            state = BattleStateEnum.RunningTurn;
            yield break;
        }

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
            state = BattleStateEnum.RunningTurn;
        }
    }

    private IEnumerator PlayThrowAnimation(SpriteRenderer pokeball)
    {

        // Animations
        yield return pokeball
            .transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f)
            .WaitForCompletion();

        yield return enemyUnit.PlayCaptureAnimation();

        yield return pokeball
            .transform.DOMoveY(enemyUnit.transform.position.y - 2f, 0.5f)
            .WaitForCompletion();
        // Aiming for bouncing, but it's not great.
        // yield return pokeball.transform.DOJump(enemyUnit.transform.position - new Vector3(0, 2), 1f, 3, 0.5f).WaitForCompletion();
    }

    private IEnumerator CatchPokemon(SpriteRenderer pokeball)
    {
        // Pokemon caught!
        yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Name} was caught!");
        yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

        playerParty.AddPokemon(enemyUnit.Pokemon);

        yield return ApplyExpGain(enemyUnit, true);

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
