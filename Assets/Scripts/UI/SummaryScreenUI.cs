using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummaryScreenUI : MonoBehaviour
{
    [Header("Basic Details")]
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] Image image;

    [Header("Pages")]
    [SerializeField] GameObject skillsPage, movesPage;
    [SerializeField] TMP_Text pageName;

    [Header("Pokemon Skills")]
    [SerializeField] TMP_Text hpText;
    [SerializeField] TMP_Text attackText, defenseText, spAttackText, spDefenseText, speedText, expPointsText, nextLevelExpText;
    [SerializeField] StatBar expBar;

    [Header("Pokemon Moves")]
    [SerializeField] List<TMP_Text> moveTypes, moveNames, movePPs;
    [SerializeField] TMP_Text descriptionText;

    Pokemon _pokemon;
    public void SetBasicDetails(Pokemon pokemon)
    {
        _pokemon = pokemon;
        nameText.text = pokemon.Name;
        levelText.text = "Lvl " + pokemon.Level;
        image.sprite = pokemon.Base.FrontSprite;
    }

    public void ShowPage(int page)
    {
        switch (page)
        {
            case 0:
                // Skills page
                pageName.text = "Pokemon Stats";
                skillsPage.SetActive(true);
                movesPage.SetActive(false);
                SetSkills(_pokemon);
                break;
            case 1:
                // Moves page
                pageName.text = "Pokemon Moves";
                skillsPage.SetActive(false);
                movesPage.SetActive(true);
                SetMoves(_pokemon);
                break;
            default:
                break;
        }
    }
    
    private void SetSkills(Pokemon pokemon)
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

    private void SetMoves(Pokemon pokemon)
    {
        var moves = pokemon.Moves;
        for (int i = 0; i < moveNames.Count; i++)
        {
            if (i < moves.Count)
            {
                var move = moves[i];
                moveTypes[i].text = move.Base.Type.ToString().ToUpper();
                moveNames[i].text = move.Base.Name.ToUpper();
                movePPs[i].text = $"{move.PP}/{move.MaxPP}";
            }
            else
            {
                moveTypes[i].text = "-";
                moveNames[i].text = "-";
                movePPs[i].text = "-";
            }
        }
    }
}
