using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TMP_Text nameText, levelText;
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

    public void UpdateData()
    {
        nameText.text = _pokemon.Base.Name;
        levelText.text = "Lvl " + _pokemon.Level;
        SetStatusText();
        hpBar.SetStat((float)_pokemon.HP / _pokemon.MaxHP);
        expBar.SetStat(_pokemon.GetNormalizedExp());
    }

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            nameText.color = GlobalSettings.I.HighlightedColor;
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
