using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Image pokemonIcon;
    [SerializeField] TMP_Text nameText, levelText, genderText;
    [SerializeField] Text statusLabel;
    [SerializeField] StatBar hpBar, expBar;
    [SerializeField] TMP_Text messageText;

    Pokemon _pokemon;

    public void Init(Pokemon pokemon)
    {
        _pokemon = pokemon;
        UpdateData();
        SetMessage("");

        _pokemon.OnHPChanged += UpdateData;
    }

    public void UpdateData()
    {
        pokemonIcon.sprite = _pokemon.Base.WalkDownAnim[0];
        nameText.text = _pokemon.Base.Name;
        levelText.text = "Lvl " + _pokemon.Level;
        SetGenderText();
        SetStatusText();
        hpBar.SetStat((float)_pokemon.HP / _pokemon.MaxHP);
        expBar.SetStat(_pokemon.GetNormalizedExp());
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

    public void SetStatusText()
    {
        if (_pokemon.Status == null)
        {
            statusLabel.text = "";
        }
        else
        {
            var conditionID = _pokemon.Status.Id;
            statusLabel.text = conditionID.ToString().ToUpper();
            statusLabel.color = GlobalSettings.I.GetStatusColor(conditionID);
        }
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            nameText.color = GlobalSettings.I.HighlightedTextColor;
        }
        else
        {
            nameText.color = GlobalSettings.I.DefaultFontColor;
        }
    }

    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}
