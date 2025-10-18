using GDEUtils.StateMachine;
using UnityEngine;

public class CutsceneState : State<GameController>
{
    
    public static CutsceneState I { get; private set; }
    
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
        gc.Player.Character.HandleUpdate();
    }
}
