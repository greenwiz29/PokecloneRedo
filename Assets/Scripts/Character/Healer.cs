using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog onCancelDialog;
    [SerializeField] Dialog onHealDialog;

    public IEnumerator Heal(Transform player)
    {
        int selection = 0;

        yield return DialogManager.I.ShowDialog(dialog, new List<string>() { "Yes", "No" }, (choiceIndex) => selection = choiceIndex);

        if (selection == 1)
        {
            yield return DialogManager.I.ShowDialog(onCancelDialog);
            yield break;
        }

        yield return Fader.I.FadeIn(0.5f);

        var party = player.GetComponent<PokemonParty>();

        party.HealParty();

        yield return Fader.I.FadeOut(0.5f);

        yield return DialogManager.I.ShowDialog(onHealDialog);
    }
}
