using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class Quest
{
    public QuestBase Base { get; private set; }
    public QuestStatus Status { get; private set; }

    public Quest(QuestBase _base)
    {
        Base = _base;
    }

    public Quest(QuestSaveData saveData)
    {
        Base = QuestDB.GetObjectByName(saveData.name);
        Status = saveData.status;
    }

    public QuestSaveData GetSaveData()
    {
        var saveData = new QuestSaveData()
        {
            name = Base.name,
            status = Status
        };
        return saveData;
    }

    public IEnumerator StartQuest()
    {
        Status = QuestStatus.Started;
        yield return DialogManager.I.ShowDialog(Base.StartDialog);

        var questList = QuestList.GetQuestList();
        questList.AddQuest(this);
    }

    public IEnumerator CompleteQuest()
    {
        Status = QuestStatus.Completed;
        yield return DialogManager.I.ShowDialog(Base.CompletedDialog);

        var inventory = Inventory.GetPlayerInventory();
        if (Base.RequiredItem != null)
        {
            inventory.RemoveItem(Base.RequiredItem);
        }

        if (Base.RewardItem != null)
        {
            inventory.AddItem(Base.RewardItem, Base.RewardCount);

            yield return DialogManager.I.ShowDialogText($"You received {Base.RewardItem.Name} X{Base.RewardCount}");
        }

        var questList = QuestList.GetQuestList();
        questList.AddQuest(this);
    }

    public bool CanBeCompleted()
    {
        if (Base.RequiredItem != null)
        {
            var inventory = Inventory.GetPlayerInventory();

            return inventory.HasItem(Base.RequiredItem);
        }

        return true;
    }
}

public enum QuestStatus { None, Started, Completed }

[Serializable]
public class QuestSaveData
{
    public string name;
    public QuestStatus status;
}