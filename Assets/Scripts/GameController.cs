using System;
using System.Collections;
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
    MenuController menuController;

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
        battleSystem.OnBattleOver += EndBattle;

        partyScreen.Init();

        menuController = GetComponent<MenuController>();
        menuController.OnBack += () =>
        {
            state = GameState.FreeRoam;
        };
        menuController.OnMenuSelected += OnMenuSelected;

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

    private void OnMenuSelected(int selectedItem)
    {
        switch (selectedItem)
        {
            case 0: // Pokemon
                partyScreen.gameObject.SetActive(true);
                partyScreen.SetPartyData();
                state = GameState.PartyScreen;
                break;
            case 1: // Bag
                inventoryUI.gameObject.SetActive(true);
                state = GameState.Bag;
                break;
            case 2: // Save
                SavingSystem.i.Save("saveSlot1");
                state = GameState.FreeRoam;
                break;
            case 3: // Load
                SavingSystem.i.Load("saveSlot1");
                state = GameState.FreeRoam;
                break;
        }
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
        switch (state)
        {
            case GameState.FreeRoam:
                playerController.HandleUpdate();

                if (Input.GetKeyDown(KeyCode.Return))
                {
                    menuController.OpenMenu();
                    state = GameState.Menu;
                }
                break;
            case GameState.Cutscene:
                playerController.HandleUpdate();            
                break;
            case GameState.Battle:
                battleSystem.HandleUpdate();
                break;
            case GameState.Dialog:
                DialogManager.I.HandleUpdate();
                break;
            case GameState.Menu:
                menuController.HandleUpdate();
                break;
            case GameState.PartyScreen:
                Action onSelected = () =>
                {
                    // int selectedChoice = 0;

                    // StartCoroutine(DialogManager.I.ShowDialogText("Choose an action", true, true,
                    //     new List<string>() { "Swap", "Summary", "Cancel" },
                    //     (choiceIndex) =>
                    //     {
                    //         selectedChoice = choiceIndex; // This doesn't seem to be working. selectedChoice always 0
                    //     }));

                    // if (selectedChoice == 0)
                    // {
                    //     // Swap pokemon
                    // }
                    // else if (selectedChoice == 1)
                    // {
                    //     // Summary
                    // }
                    // else if (selectedChoice == 2)
                    // {
                    //     // Cancel - return to selection
                    //     return;
                    // }
                };
                Action onBack = () =>
                {
                    partyScreen.gameObject.SetActive(false);
                    menuController.OpenMenu();
                    state = GameState.Menu;
                };
                partyScreen.HandleUpdate(onSelected, onBack);
                break;
            case GameState.Bag:
                Action<ItemBase> onItemUsed = (itemUsed) =>
                {
                    // TODO: Options for switching, summary, etc.
                    inventoryUI.gameObject.SetActive(false);
                    menuController.OpenMenu();
                    state = GameState.Menu;
                };
                onBack = () =>
                {
                    inventoryUI.gameObject.SetActive(false);
                    menuController.OpenMenu();
                    state = GameState.Menu;
                };
                inventoryUI.HandleUpdate(onBack);
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
}
