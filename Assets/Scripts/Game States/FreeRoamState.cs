using GDEUtils.StateMachine;
using UnityEngine;

public class FreeRoamState : State<GameController>
{
    public static FreeRoamState I { get; private set; }

	void Awake()
	{
		I = this;
	}

    GameController gc;

	public override void Enter(GameController owner)
	{
		gc = owner;
	}

    public override void Execute()
    {
        GameController.I.Player.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Return))
        {
            gc.stateMachine.Push(GameMenuState.I);
        }
    }
}
