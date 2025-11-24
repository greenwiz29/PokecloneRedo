using System.Collections.Generic;
using System.Linq;
using GDEUtils.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummaryScreenUI : SelectionUI<TextSlot>
{
    [Header("Basic Details")]
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text genderText;
    [SerializeField] Image image;
    [SerializeField] Image shinyIcon;

    [Header("Pages")]
    [SerializeField] GameObject skillsPage, movesPage, effectsPane, detailedStatsPage;
    [SerializeField] TMP_Text pageName;

    [Header("Pokemon Skills")]
    [SerializeField] TMP_Text hpText;
    [SerializeField] TMP_Text attackText, defenseText, spAttackText, spDefenseText, speedText, abilityNameText, abilityDescText, expPointsText, nextLevelExpText;
    [SerializeField] StatBar expBar;

    [Header("Pokemon Moves")]
    [SerializeField] List<TMP_Text> moveTypes, moveNames, movePPs;
    [SerializeField] TMP_Text descriptionText, powerText, accuracyText;

    [Header("Detailed Skills")]
    [SerializeField] TMP_Text hpIVText;
    [SerializeField] TMP_Text attackIVText, defenseIVText, spAttackIVText, spDefenseIVText, speedIVText;

    List<TextSlot> moveSlots;
    void Start()
    {
        moveSlots = moveNames.Select(m => m.GetComponent<TextSlot>()).ToList();
        effectsPane.SetActive(false);
        descriptionText.text = "";
    }

    public bool InMoveSelection
    {
        get => inMoveSelection;
        set
        {
            inMoveSelection = value;
            if (inMoveSelection)
            {
                selection = 0;
                SetItems(moveSlots.Take(_pokemon.Moves.Count).ToList());
                effectsPane.SetActive(true);
                SetMoveDetails(selection);
            }
            else
            {
                ClearItems();
                effectsPane.SetActive(false);
            }
        }
    }

    Pokemon _pokemon;
    private bool inMoveSelection;

    public override void HandleUpdate()
    {
        if (InMoveSelection)
            base.HandleUpdate();
    }
    public override void UpdateSelectionUI()
    {
        base.UpdateSelectionUI();
        SetMoveDetails(selection);
    }

    public void SetBasicDetails(Pokemon pokemon)
    {
        _pokemon = pokemon;
        nameText.text = pokemon.Name;
        SetGenderText();
        levelText.text = "Lvl " + pokemon.Level;
        image.sprite = pokemon.FrontSprite;
        shinyIcon.gameObject.SetActive(pokemon.IsShiny);
    }

    public void SetGenderText()
    {
        switch (_pokemon.Gender)
        {
            case PokemonGender.Female:
                char f = '\u2640';
                genderText.text = f.ToString();
                genderText.color = new Color(255, 0, 210);
                break;
            case PokemonGender.Male:
                char m = '\u2642';
                genderText.text = m.ToString();
                genderText.color = new Color(0, 100, 255);
                break;
            default:
                genderText.text = "";
                break;
        }
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
                detailedStatsPage.SetActive(false);
                SetSkills(_pokemon);
                break;
            case 1:
                // Moves page
                pageName.text = "Pokemon Moves";
                skillsPage.SetActive(false);
                movesPage.SetActive(true);
                detailedStatsPage.SetActive(false);
                SetMoves(_pokemon);
                break;
            case 2:
                // Detailed Stats page
                pageName.text = "Detailed Stats";
                skillsPage.SetActive(false);
                movesPage.SetActive(false);
                detailedStatsPage.SetActive(true);
                SetStatDetails(_pokemon);
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

        abilityNameText.text = pokemon.Ability.Name;
        abilityDescText.text = pokemon.Ability.Description;

        expPointsText.text = "" + pokemon.Exp;
        nextLevelExpText.text = "" + (pokemon.CalculateBaseExpForLevel(pokemon.Level + 1) - pokemon.Exp);

        float normalizedExp = pokemon.GetNormalizedExp();
        expBar.SetStat(normalizedExp);
    }

    private void SetStatDetails(Pokemon pokemon)
    {
        hpIVText.text = "" + pokemon.StatIVs[Stat.HP];
        attackIVText.text = "" + pokemon.StatIVs[Stat.Attack];
        defenseIVText.text = "" + pokemon.StatIVs[Stat.Defense];
        spAttackIVText.text = "" + pokemon.StatIVs[Stat.SpAttack];
        spDefenseIVText.text = "" + pokemon.StatIVs[Stat.SpDefense];
        speedIVText.text = "" + pokemon.StatIVs[Stat.Speed];
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

    private void SetMoveDetails(int selection)
    {
        var move = _pokemon.Moves[selection];
        descriptionText.text = move.Base.Desc;
        powerText.text = move.Base.Power + "";
        accuracyText.text = move.Base.Accuracy + "";
    }

}
