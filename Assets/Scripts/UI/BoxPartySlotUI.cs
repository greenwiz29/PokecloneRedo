using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoxPartySlotUI : MonoBehaviour
{
    [SerializeField] TMP_Text nameText, lvlText;
    [SerializeField] Image image;

    public void SetData(Pokemon pokemon)
    {
        nameText.text = pokemon.Name;
        lvlText.text = "Lvl " + pokemon.Level;
        image.sprite = pokemon.FrontSprite;
        image.color = Color.white;
    }

    public void ClearData()
    {
        nameText.text = "";
        lvlText.text = "";
        image.sprite = null;
        image.color = GlobalSettings.I.Transparent;
    }
}
