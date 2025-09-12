using UnityEngine;

public class TrainerFov : MonoBehaviour, IPlayerTriggerable
{
	public bool TriggerRepeatedly => false;

	public void OnPlayerTriggered(PlayerController player)
	{
        GameController.I.OnEnterTrainerView(GetComponentInParent<TrainerController>());
	}
}
