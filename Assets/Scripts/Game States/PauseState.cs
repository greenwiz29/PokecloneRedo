using GDEUtils.StateMachine;
using UnityEngine;

public class PauseState : State<GameController>
{
    public static PauseState I { get; private set; }

    void Awake()
    {
        I = this;
    }
}
