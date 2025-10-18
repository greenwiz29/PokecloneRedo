using GDEUtils.StateMachine;
using UnityEditorInternal;
using UnityEngine;

public class DialogState : State<GameController>
{
    
    public static DialogState I { get; private set; }
    
    void Awake()
    {
        I = this;
    }

    GameController gc;
    public override void Enter(GameController owner)
    {
        gc = owner;
    }
}
