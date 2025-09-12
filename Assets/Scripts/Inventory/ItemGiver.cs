using System.Collections;
using UnityEngine;

public class ItemGiver : MonoBehaviour, ISavable
{
    [SerializeField] ItemBase item;
    [SerializeField] int count = 1;
    [SerializeField] Dialog dialog;

    bool Used = false;

    public IEnumerator GiveItem(PlayerController player)
    {
        if (Used) yield break;

        yield return DialogManager.I.ShowDialog(dialog);

        player.GetComponent<Inventory>().AddItem(item, count);

        Used = true;

        yield return DialogManager.I.ShowDialogText($"You received {item.Name} X{count}");
    }

    public bool CanBeGiven()
    {
        return item != null && !Used && count > 0;
    }

	public object CaptureState()
	{
        return Used;
	}

	public void RestoreState(object state)
	{
        Used = (bool)state;
	}
}
