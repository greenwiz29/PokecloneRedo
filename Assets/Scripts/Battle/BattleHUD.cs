using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] TMP_Text nameText, levelText;
    [SerializeField] Text statusLabel;
    [SerializeField] StatBar hpBar, expBar;

    Pokemon _pokemon;

    public void SetData(Pokemon pokemon)
    {
        ClearData();

        _pokemon = pokemon;
        nameText.text = pokemon.Base.Name;
        SetLevel();
        hpBar.SetStat((float)pokemon.HP / pokemon.MaxHP);

        SetStatusText();
        SetExp();
        _pokemon.OnStatusChanged += SetStatusText;
        _pokemon.OnHPChanged += () =>
        {
            StartCoroutine(UpdateHP());
        };
    }

    public void ClearData()
    {
        if (_pokemon != null)
        {
            _pokemon.OnStatusChanged -= SetStatusText;
            _pokemon.OnHPChanged -= () =>
            {
                StartCoroutine(UpdateHP());
            };
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

    public IEnumerator UpdateHP()
    {
        yield return hpBar.SetStatSmooth((float)_pokemon.HP / _pokemon.MaxHP);
    }

    public IEnumerator WaitForHPUpdate()
    {
        yield return new WaitUntil(() => hpBar.IsUpdating == false);
    }

    public IEnumerator UpdateEXP(bool reset)
    {
        if (expBar == null) yield break;

        if (reset) expBar.SetStat(0f);

        yield return expBar.SetStatSmooth(_pokemon.GetNormalizedExp());
    }

    public void SetExp()
    {
        if (expBar == null) return;

        float normalizedExp = _pokemon.GetNormalizedExp();
        expBar.SetStat(normalizedExp);
    }

    public void SetLevel()
    {
        levelText.text = "Lvl " + _pokemon.Level;
    }
}
