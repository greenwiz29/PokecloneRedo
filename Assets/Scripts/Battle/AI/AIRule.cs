using UnityEngine;

public abstract class AIRule : ScriptableObject
{
    /// <summary>
    /// Modify desirability of an action.
    /// </summary>
    public virtual int ScoreAction(
        BattleSystem bs,
        BattleUnit unit,
        BattleAction action
    ) => 0;
}
