using System.Collections.Generic;
using GDEUtils.StateMachine;
using UnityEngine;

public class GameStatesContainer : MonoSingleton<GameStatesContainer>
{
    [SerializeField] List<State<GameController>> gameStates;
    [SerializeField] List<State<BattleSystem>> battleStates;

    public List<State<GameController>> GameStates => gameStates;
    public List<State<BattleSystem>> BattleStates => battleStates;
}
