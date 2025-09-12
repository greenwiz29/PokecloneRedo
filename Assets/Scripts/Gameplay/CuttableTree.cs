using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttableTree : MonoBehaviour, IInteractable
{
    [SerializeField] KeyItem requiredItem;

    public IEnumerator Interact(Transform initiator)
    {
        yield return DialogManager.I.ShowDialogText("The tree looks like it can be cut down.");

        int selection = 0;
        if (Inventory.GetPlayerInventory().HasItem(requiredItem))
        {
            yield return DialogManager.I.ShowDialogText($"Use your {requiredItem.Name}?", waitForInput: false, autoClose: false, choices: new List<string>() { "Yes", "No" },
                onChoiceSelected: (choiceIndex) => selection = choiceIndex);

            if (selection == 0)
            {
                yield return DialogManager.I.ShowDialogText($"You cut down the tree.");
                gameObject.SetActive(false);
            }
        }
        else
        {
            yield return DialogManager.I.ShowDialogText($"You need a {requiredItem.Name} for that.");
        }

    }
}
