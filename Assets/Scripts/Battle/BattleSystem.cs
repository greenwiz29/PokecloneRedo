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
    [SerializeField] Image playerImage, trainerImage;
    [SerializeField] GameObject pokeballSprite;
    [SerializeField] MoveToForgetUI moveToForgetUI;
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] StatChangesUI statChangesUI;

    [Header("Background Images")]
    [SerializeField] Image backgroundImage;
    [SerializeField] Sprite longGrassBG;
    [SerializeField] Sprite waterBG;
    [SerializeField] Sprite caveBG;

    BattleStateEnum state;
    bool aboutToUseChoice;

    PokemonParty playerParty, trainerParty;
    Pokemon wildPokemon;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;
    public int EscapeAttempts { get; set; }
    MoveBase moveToLearn;

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
    public PartyScreen PartyScreen => partyScreen;
    public int SelectedMove { get; set; }
    public BattleAction SelectedAction { get; set; }
    public Pokemon SelectedPokemon { get; set; }
    public bool IsBattleOver { get; private set; }
    public bool IsTrainerBattle => isTrainerBattle;
    public PokemonParty PlayerParty => playerParty;
    public PokemonParty TrainerParty => trainerParty;


    void Awake()
    {
        // onBagBack = () =>
        // {
        //     inventoryUI.gameObject.SetActive(false);
        //     state = BattleStateEnum.ActionSelection;
        // };

        // onItemUsed = (usedItem) =>
        // {
        //     StartCoroutine(OnItemUsed(usedItem));
        // };
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
        EscapeAttempts = 0;
        IsBattleOver = false;

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

    public void HandleUpdate()
    {
        StateMachine.Execute();

        switch (state)
        {
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

    public IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        dialogBox.EnableDialogText(true);
        if (playerUnit.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back, {playerUnit.Pokemon.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        yield return SendOutPlayerPokemon(newPokemon);
    }

    IEnumerator SendNextTrainerPokemon()
    {
        state = BattleStateEnum.Busy;
        var next = trainerParty.GetHealthyPokemon();

        enemyUnit.Setup(next);
        yield return dialogBox.TypeDialog($"{trainer.Name} sent out {next.Name}");

        state = BattleStateEnum.RunningTurn;
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

    public IEnumerator ApplyExpGain(BattleUnit targetUnit, bool unitFainted, DamageDetails damageDetails = null)
    {
        // Exp gain
        int expYield = targetUnit.Pokemon.Base.ExpYield;
        int enemyLevel = targetUnit.Pokemon.Level;
        float trainerBonus = IsTrainerBattle ? 1.5f : 1f;

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
                    MoveToForgetState.I.NewMove = newMove.Base;
                    MoveToForgetState.I.CurrentMoves = pokemon.Moves.Select(m => m.Base).ToList();
                    // yield return bs.StateMachine.PushAndWait(MoveToForgetState.I);
                }
                newMove = null;
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

        // yield return RunTurns(BattleAction.Item);
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
