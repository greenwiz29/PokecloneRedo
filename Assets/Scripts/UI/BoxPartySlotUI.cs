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
        image.sprite = pokemon.Base.FrontSprite;
    }

    public void Clear()
    {
        nameText.text = "";
        lvlText.text = "";
        image.sprite = null;
        image.color = Color.clear;
    }
}
