using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : MonoBehaviour, IInteractable, ISavable
{
    [SerializeField] Dialog dialog;

    [Header("Quests")]
    [SerializeField] QuestBase questToStart, questToComplete;

    [Header("Movement")]
    [SerializeField] List<Vector2> movePattern;
    [SerializeField] float moveDelay;

    float idleTimer = 0f;
    NPCState state;

    private Character character;
    ItemGiver itemGiver;
    private int currentPattern;
    Quest activeQuest;
    PokemonGiver pokemonGiver;
    Healer healer;
    Merchant merchant;

    void Awake()
    {
        character = GetComponent<Character>();
        itemGiver = GetComponent<ItemGiver>();
        pokemonGiver = GetComponent<PokemonGiver>();
        healer = GetComponent<Healer>();
        merchant = GetComponent<Merchant>();
    }

    public IEnumerator Interact(Transform initiator)
    {
        if (state == NPCState.Idle)
        {
            state = NPCState.Dialog;
            character.LookTowards(initiator.position);

            if (questToComplete != null)
            {
                var quest = new Quest(questToComplete);
                yield return quest.CompleteQuest();
                questToComplete = null;
            }

            if (pokemonGiver != null && pokemonGiver.CanBeGiven())
            {
                yield return pokemonGiver.GivePokemon(initiator.GetComponent<PlayerController>());
            }
            if (itemGiver != null && itemGiver.CanBeGiven())
            {
                yield return itemGiver.GiveItem(initiator.GetComponent<PlayerController>());
            }
            else if (healer != null)
            {
                yield return healer.Heal(initiator);
            }
            else if (questToStart != null)
            {
                activeQuest = new Quest(questToStart);
                yield return activeQuest.StartQuest();
                questToStart = null;

                if (activeQuest.CanBeCompleted())
                {
                    yield return activeQuest.CompleteQuest();
                    activeQuest = null;
                }
            }
            else if (activeQuest != null)
            {
                if (activeQuest.CanBeCompleted())
                {
                    yield return activeQuest.CompleteQuest();
                    activeQuest = null;
                }
                else
                {
                    yield return DialogManager.I.ShowDialog(activeQuest.Base.InProgressDialog);
                }
            }
            else if (merchant != null)
            {
                yield return merchant.Trade();
            }
            else
            {
                yield return DialogManager.I.ShowDialog(dialog);
            }

            state = NPCState.Idle;
            idleTimer = 0f;
        }
    }

    void Update()
    {
        if (state == NPCState.Idle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > moveDelay)
            {
                idleTimer = 0f;
                if (movePattern.Count > 0)
                {
                    StartCoroutine(Walk());
                }
            }
        }
        character.HandleUpdate();
    }

    private IEnumerator Walk()
    {
        state = NPCState.Walking;

        yield return character.Move(movePattern[currentPattern], () =>
        {
            currentPattern = (currentPattern + 1) % movePattern.Count;
        });

        state = NPCState.Idle;
    }

    [ContextMenu("Show Path")]
    void ShowPath()
    {
        var pos = new Vector2(transform.position.x, transform.position.y);
        var index = 0;
        var colours = new List<Color>()
        {
            Color.red,
            Color.green,
            Color.blue
        };

        foreach (Vector2 path in movePattern)
        {
            Vector2 newPosRef = movePattern[index];

            if (newPosRef.x == 0)
                newPosRef.y *= 1f;
            else if (newPosRef.y == 0)
                newPosRef.x *= 1f;

            Debug.DrawLine(pos, pos + newPosRef, colours[index % 3], 1f);

            index += 1;
            pos += newPosRef;
        }

    }

    public object CaptureState()
    {
        var saveData = new NPCQuestSaveData();

        saveData.activeQuest = activeQuest?.GetSaveData();

        if (questToStart != null)
            saveData.questToStart = new Quest(questToStart).GetSaveData();
        if (questToComplete != null)
            saveData.questToComplete = new Quest(questToComplete).GetSaveData();

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as NPCQuestSaveData;

        if (saveData != null)
        {
            activeQuest = (saveData.activeQuest != null) ? new Quest(saveData.activeQuest) : null;

            questToStart = (saveData.questToStart != null) ? new Quest(saveData.questToStart).Base : null;

            questToComplete = (saveData.questToComplete != null) ? new Quest(saveData.questToComplete).Base : null;
        }
    }
}

public enum NPCState { Idle, Walking, Dialog }

[Serializable]
public class NPCQuestSaveData
{
    public QuestSaveData activeQuest;
    public QuestSaveData questToStart;
    public QuestSaveData questToComplete;
}