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
    [SerializeField] Image image;
    [SerializeField] Image shinyIcon;

    [Header("Pages")]
    [SerializeField] GameObject skillsPage, movesPage, effectsPane;
    [SerializeField] TMP_Text pageName;

    [Header("Pokemon Skills")]
    [SerializeField] TMP_Text hpText;
    [SerializeField] TMP_Text attackText, defenseText, spAttackText, spDefenseText, speedText, abilityNameText, abilityDescText, expPointsText, nextLevelExpText;
    [SerializeField] StatBar expBar;

    [Header("Pokemon Moves")]
    [SerializeField] List<TMP_Text> moveTypes, moveNames, movePPs;
    [SerializeField] TMP_Text descriptionText, powerText, accuracyText;

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
        levelText.text = "Lvl " + pokemon.Level;
        image.sprite = pokemon.Base.FrontSprite;
        shinyIcon.gameObject.SetActive(pokemon.IsShiny);
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

        abilityNameText.text = pokemon.Ability.Name;
        abilityDescText.text = pokemon.Ability.Description;

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

    private void SetMoveDetails(int selection)
    {
        var move = _pokemon.Moves[selection];
        descriptionText.text = move.Base.Desc;
        powerText.text = move.Base.Power + "";
        accuracyText.text = move.Base.Accuracy + "";
    }

}
