using System;
using System.Collections;
using GDEUtils.StateMachine;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] Camera worldCamera;
    [SerializeField] PartyScreen partyScreen;

    public static GameController I { get; private set; }
    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PreviousScene { get; private set; }
    public PlayerController Player => playerController;
    public PartyScreen PartyScreen => partyScreen;

    public StateMachine<GameController> stateMachine { get; private set; }
    public Camera WorldCamera => worldCamera;

    void Awake()
    {
        I = this;

        // Hide and disable mouse
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;

        StatusConditionsDB.Init();
        WeatherConditionsDB.Init();
        PokemonDB.Init();
        MoveDB.Init();
        ItemsDB.Init();
        QuestDB.Init();
    }

    void Start()
    {
        stateMachine = new StateMachine<GameController>(this);
        stateMachine.Push(FreeRoamState.I);

        partyScreen.Init();

        DialogManager.I.OnShowDialog += () =>
        {
            stateMachine.Push(DialogState.I);
        };
        DialogManager.I.OnDialogFinished += () =>
        {
            stateMachine.Pop();
        };
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            stateMachine.Push(PauseState.I);
        }
        else
            stateMachine.Pop();
    }

    public void OnEnterTrainerView(TrainerController trainer)
    {
        if (trainer != null)
        {
            StartCoroutine(trainer.TriggerTrainerBattle(playerController));
        }
    }

    public void StartBattle(BattleTrigger trigger)
    {
        BattleState.I.Trigger = trigger;
        BattleState.I.Trainer = null;
        BattleState.I.WildPokemon = null;
        stateMachine.Push(BattleState.I);
    }

    public void StartOverworldPokemonBattle(WildPokemonController wildPokemon, BattleTrigger trigger = BattleTrigger.LongGrass)
    {
        BattleState.I.WildPokemon = wildPokemon;
        BattleState.I.Trainer = null;
        BattleState.I.Trigger = trigger;
        stateMachine.Push(BattleState.I);
    }

    public void StartTrainerBattle(TrainerController trainer, BattleTrigger trigger = BattleTrigger.LongGrass)
    {
        BattleState.I.Trigger = trigger;
        BattleState.I.Trainer = trainer;
        BattleState.I.WildPokemon = null;
        stateMachine.Push(BattleState.I);
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Execute();
    }

    bool isTransitioning;
    public void TransitionToScene(SceneDetails newScene)
    {
        if (isTransitioning)
            return;

        if (CurrentScene == newScene)
            return;

        isTransitioning = true;

        newScene.LoadScene();

        var prevScene = CurrentScene;
        PreviousScene = prevScene;
        CurrentScene = newScene;

        foreach (var scene in newScene.ConnectedScenes)
            scene.LoadScene();

        if (prevScene != null)
        {
            foreach (var scene in prevScene.ConnectedScenes)
            {
                if (!newScene.ConnectedScenes.Contains(scene) && scene != newScene)
                    scene.UnloadScene();
            }

            if (!newScene.ConnectedScenes.Contains(prevScene))
                prevScene.UnloadScene();
        }

        isTransitioning = false;
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

        GUILayout.Label(" State Stack", style);
        foreach (var s in stateMachine.StateStack)
        {
            GUILayout.Label(" " + s.GetType().Name, style);
        }
    }
#endif
}
