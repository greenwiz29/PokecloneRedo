using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// A single condition: one flag or quest check.
[System.Serializable]
public class DialogCondition
{
    public enum ConditionType { FlagSet, FlagNotSet, QuestStarted, QuestCompleted, CounterMin }

    [SerializeField] public ConditionType type;

    // Used for FlagSet / FlagNotSet
    [SerializeField] public WorldStateFlag flag;

    // Used for QuestStarted / QuestCompleted
    [SerializeField] public QuestBase quest;

    // Used for CounterMin — passes if GetCounter(counterKey) >= minValue
    [SerializeField] public string counterKey;
    [SerializeField] public int minValue;

    public bool Evaluate()
    {
        var wsm = WorldStateManager.I;
        var questList = QuestList.GetQuestList();

        return type switch
        {
            ConditionType.FlagSet => wsm.IsFlagSet(flag),
            ConditionType.FlagNotSet => !wsm.IsFlagSet(flag),
            ConditionType.QuestStarted => questList.IsStarted(quest.Name),
            ConditionType.QuestCompleted => questList.IsCompleted(quest.Name),
            ConditionType.CounterMin => wsm.GetCounter(counterKey) >= minValue,
            _ => false
        };
    }
}

// One AND-group: all conditions must pass.
[System.Serializable]
public class ConditionGroup
{
    [SerializeField] public List<DialogCondition> conditions = new();

    public bool Evaluate() => conditions.All(c => c.Evaluate());
}

// A full condition set: any one group must pass (AND-groups OR'd together).
[System.Serializable]
public class ConditionSet
{
    [SerializeField] public List<ConditionGroup> groups = new();

    // An empty condition set passes unconditionally (useful as a fallback).
    public bool Evaluate() => groups.Count == 0 || groups.Any(g => g.Evaluate());
}

// Maps a condition set to either a simple Dialog or a Yarn node name.
[System.Serializable]
public class ConditionalDialog
{
    [SerializeField] public ConditionSet conditions;

    [Tooltip("Used for simple, non-branching conversations.")]
    [SerializeField] public Dialog simpleDialog;

}