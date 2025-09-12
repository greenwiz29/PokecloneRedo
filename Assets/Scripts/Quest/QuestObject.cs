using UnityEngine;

public class QuestObject : MonoBehaviour
{
    [SerializeField] QuestBase questToCheck;
    [SerializeField] ObjectActions onStart;
    [SerializeField] ObjectActions onComplete;

    QuestList questList;

    void Start()
    {
        questList = QuestList.GetQuestList();
        questList.OnUpdated += UpdateObjectStatus;

        UpdateObjectStatus();
    }

	void OnDestroy()
	{
        questList.OnUpdated -= UpdateObjectStatus;
	}

	public void UpdateObjectStatus()
    {
        if (onStart != ObjectActions.DoNothing && questList.IsStarted(questToCheck.Name))
        {
            foreach (Transform child in transform)
            {
                if (onStart == ObjectActions.Enable)
                {
                    child.gameObject.SetActive(true);

                    var entity = child.GetComponent<SavableEntity>();
                    if (entity != null)
                    {
                        SavingSystem.i.RestoreEntity(entity);
                    }
                }
                else if (onStart == ObjectActions.Disable)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        if (onComplete != ObjectActions.DoNothing && questList.IsCompleted(questToCheck.Name))
        {
            foreach (Transform child in transform)
            {
                if (onComplete == ObjectActions.Enable)
                {
                    child.gameObject.SetActive(true);

                    var entity = child.GetComponent<SavableEntity>();
                    if (entity != null)
                    {
                        SavingSystem.i.RestoreEntity(entity);
                    }
                }
                else if (onComplete == ObjectActions.Disable)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }
}

public enum ObjectActions { DoNothing, Enable, Disable }
