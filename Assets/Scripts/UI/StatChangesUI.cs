using TMPro;
using UnityEngine;

public class StatChangesUI : MonoBehaviour
{
    [SerializeField] TMP_Text hpText, atkText, defText, spAtkText, spDefText, speedText;

    public void SetStatChanges(StatChanges changes)
    {
        hpText.text = $"{"+" + changes.hpDiff,3}";
        atkText.text = $"{"+" + changes.atkDiff,3}";
        defText.text = $"{"+" + changes.defDiff,3}";
        spAtkText.text = $"{"+" + changes.spAtkDiff,3}";
        spDefText.text = $"{"+" + changes.spDefDiff,3}";
        speedText.text = $"{"+" + changes.speedDiff,3}";
    }

    public void SetStats(Pokemon pokemon)
    {
        hpText.text = $"{pokemon.MaxHP,3}";
        atkText.text = $"{pokemon.Stats[Stat.Attack],3}";
        defText.text = $"{pokemon.Stats[Stat.Defense],3}";
        spAtkText.text = $"{pokemon.Stats[Stat.SpAttack],3}";
        spDefText.text = $"{pokemon.Stats[Stat.SpDefense],3}";
        speedText.text = $"{pokemon.Stats[Stat.Speed],3}";
    }
}
