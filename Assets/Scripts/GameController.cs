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
        stateMachine.Push(BattleState.I);        
    }

    public void StartTrainerBattle(TrainerController trainer, BattleTrigger trigger = BattleTrigger.LongGrass)
    {
        BattleState.I.Trigger = trigger;
        BattleState.I.Trainer = trainer;
        stateMachine.Push(BattleState.I); 
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Execute();
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
		// var style = new GUIStyle
		// {
		// 	fontSize = 25
		// };
		// style.normal.textColor = Color.black;

        // GUILayout.Label(" State Stack", style);
        // foreach (var s in stateMachine.StateStack)
        // {
        //     GUILayout.Label(" " + s.GetType().Name, style);
        // }
    }
#endif
}
