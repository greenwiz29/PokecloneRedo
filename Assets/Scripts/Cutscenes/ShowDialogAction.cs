using System.Collections;
using UnityEngine;

[System.Serializable]
public class ShowDialogAction : CutsceneAction
{
	[SerializeField] Dialog dialog;

	public override IEnumerator Play()
	{
		yield return DialogManager.I.ShowDialog(dialog);
	}
}
