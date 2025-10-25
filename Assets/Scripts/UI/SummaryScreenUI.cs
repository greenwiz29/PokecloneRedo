using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummaryScreenUI : MonoBehaviour
{
    [Header("Basic Details")]
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] Image image;

    [Header("Pokemon Skills")]
    [SerializeField] TMP_Text hpText; 
    [SerializeField] TMP_Text attackText, defenseText, spAttackText, spDefenseText, speedText, expPointsText, nextLevelExpText;
    [SerializeField] StatBar expBar;

    public void SetBasicDetails(Pokemon pokemon)
    {
        nameText.text = pokemon.Name;
        levelText.text = "Lvl " + pokemon.Level;
        image.sprite = pokemon.Base.FrontSprite;
    }

    public void SetSkills(Pokemon pokemon)
    {
        hpText.text = $"{pokemon.HP}/{pokemon.MaxHP}";
        attackText.text = "" + pokemon.Attack;
        defenseText.text = "" + pokemon.Defense;
        spAttackText.text = "" + pokemon.SpAttack;
        spDefenseText.text = "" + pokemon.SpDefense;
        speedText.text = "" + pokemon.Speed;

        expPointsText.text = "" + pokemon.Exp;
        nextLevelExpText.text = "" + (pokemon.Base.CalculateBaseExpForLevel(pokemon.Level + 1) - pokemon.Exp);

        float normalizedExp = pokemon.GetNormalizedExp();
        expBar.SetStat(normalizedExp);
    }
}
