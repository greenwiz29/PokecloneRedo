using System;
using System.Collections;
using GDEUtils.StateMachine;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Pause, Dialog, Cutscene, Menu, PartyScreen, Bag, Evolution, Shop }
public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;

    public static GameController I { get; private set; }
    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PreviousScene { get; private set; }
    public PlayerController Player => playerController;

    public GameState State => state;

    GameState state, prevState, preEvoState;

    public StateMachine<GameController> stateMachine { get; private set; }

    void Awake()
    {
        I = this;

        // Hide and disable mouse
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;

        ConditionsDB.Init();
        PokemonDB.Init();
        MoveDB.Init();
        ItemsDB.Init();
        QuestDB.Init();
    }

    void Start()
    {
        stateMachine = new StateMachine<GameController>(this);
        stateMachine.Push(FreeRoamState.I);

        battleSystem.OnBattleOver += EndBattle;

        partyScreen.Init();

        DialogManager.I.OnShowDialog += () =>
        {
            prevState = state;
            state = GameState.Dialog;
        };
        DialogManager.I.OnDialogFinished += () =>
        {
            if (state == GameState.Dialog)
                state = prevState;
        };

        EvolutionManager.I.OnStartEvolution += () =>
        {
            preEvoState = state;
            state = GameState.Evolution;
        };

        EvolutionManager.I.OnEndEvolution += () =>
        {
            state = preEvoState;
            partyScreen.SetPartyData();
        };

        ShopController.I.OnStart += () =>
        {
            // prevState = state;
            state = GameState.Shop;
        };

        ShopController.I.OnEnd += () =>
        {
            state = GameState.FreeRoam;
        };
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            prevState = state;
            state = GameState.Pause;
        }
        else
            state = prevState;
    }

    public void StartCutsceneState()
    {
        playerController.Character.Animator.IsMoving = false;
        state = GameState.Cutscene;
    }
    public void StartFreeRoamState()
    {
        state = GameState.FreeRoam;
    }

    public void OnEnterTrainerView(TrainerController trainer)
    {
        if (trainer != null)
        {
            state = GameState.Cutscene;
            StartCoroutine(trainer.TriggerTrainerBattle(playerController));
        }
    }

    private void EndBattle(bool playerWon)
    {
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);

        if (trainer != null && playerWon)
        {
            trainer.BattleLost();
            trainer = null;
        }
    }

    public void StartBattle(BattleTrigger trigger)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = CurrentScene.GetComponent<MapArea>().GetRandomWildPokemon(trigger);
        var wildCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);

        battleSystem.StartBattle(playerParty, wildCopy, trigger);
    }

    TrainerController trainer;
    public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        this.trainer = trainer;
        var playerParty = playerController.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Execute();

        switch (state)
        {
            case GameState.Cutscene:
                playerController.HandleUpdate();
                break;
            case GameState.Battle:
                battleSystem.HandleUpdate();
                break;
            case GameState.Dialog:
                DialogManager.I.HandleUpdate();
                break; 
            case GameState.Shop:
                ShopController.I.HandleUpdate();
                break;
            default:
                break;
        }

    }

    public void SetCurrentScene(SceneDetails currScene)
    {
        PreviousScene = CurrentScene;
        CurrentScene = currScene;
    }

    public IEnumerator MoveCamera(Vector2 offset, bool waitForFadeOut = false)
    {
        yield return Fader.I.FadeIn(0.5f);
        worldCamera.transform.position += new Vector3(offset.x, offset.y);

        if (waitForFadeOut)
            yield return new WaitForSeconds(0.5f);
        else
            StartCoroutine(Fader.I.FadeOut(0.5f));
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
		var style = new GUIStyle
		{
			fontSize = 25
		};
		style.normal.textColor = Color.black;

        GUILayout.Label("State Stack", style);
        foreach (var s in stateMachine.StateStack)
        {
            GUILayout.Label(s.GetType().Name, style);
        }
    }
#endif
}
