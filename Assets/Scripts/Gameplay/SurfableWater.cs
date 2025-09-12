using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SurfableWater : MonoBehaviour, IInteractable, IPlayerTriggerable
{
    [SerializeField] KeyItem requiredItem;
    bool isJumpingToWater = false;

	public bool TriggerRepeatedly => true;

	public IEnumerator Interact(Transform initiator)
    {
        var animator = initiator.GetComponent<CharacterAnimator>();
        if (animator.IsSurfing || isJumpingToWater) yield break;

        yield return DialogManager.I.ShowDialogText("The water is clear and calm, perfect for a swim.");

        int selection = 0;
        if (Inventory.GetPlayerInventory().HasItem(requiredItem))
        {
            yield return DialogManager.I.ShowDialogText($"Use your {requiredItem.Name}?", waitForInput: false, autoClose: false, choices: new List<string>() { "Yes", "No" },
                onChoiceSelected: (choiceIndex) => selection = choiceIndex);

            if (selection == 0)
            {
                yield return DialogManager.I.ShowDialogText($"You jump right in!");

                var dir = new Vector3(animator.MoveX, animator.MoveY);
                var targetPos = initiator.position + dir;

                // Move the player onto the water tile
                isJumpingToWater = true;
                yield return initiator.DOJump(targetPos, 0.3f, 1, 0.5f).WaitForCompletion();
                isJumpingToWater = false;
                animator.IsSurfing = true;
            }
        }
        else
        {
            yield return DialogManager.I.ShowDialogText($"You need a {requiredItem.Name} for that.");
        }
    }

	public void OnPlayerTriggered(PlayerController player)
	{
		float encounterRate = player.EncounterRate;
        // Player is walking on grass. Check random encounter rate
        if (Random.Range(1f, 101f) <= encounterRate)
        {
            GameController.I.StartBattle(BattleTrigger.Water);
        }
	}
}
