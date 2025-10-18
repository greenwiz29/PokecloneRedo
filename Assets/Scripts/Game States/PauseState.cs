using GDEUtils.StateMachine;
using UnityEngine;

public class PauseState : State<GameController>
{
    
    /// <summary>
    /// Make sure to set <see cref="AvailableItems"/> before pushing this state!
    /// </summary>
    public static PauseState I { get; private set; }

    void Awake()
    {
        I = this;
    }
}
