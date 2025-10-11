using TMPro;
using UnityEngine;

public class TextSlot : MonoBehaviour, ISelectableItem
{
    [SerializeField] TMP_Text text;

    public void Init()
    {
        text.color = GlobalSettings.I.DefaultFontColor;
    }
    
    public void OnSelectionChanged(bool selected)
    {
        text.color = selected ? GlobalSettings.I.HighlightedColor : GlobalSettings.I.DefaultFontColor;
    }

	public void SetText(string text)
    {
        this.text.text = text;
    }
}
