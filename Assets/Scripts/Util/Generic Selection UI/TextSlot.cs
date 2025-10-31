using TMPro;
using UnityEngine;

public class TextSlot : MonoBehaviour, ISelectableItem
{
    [SerializeField] TMP_Text text;

    public void Clear()
    {
        text.color = GlobalSettings.I.DefaultFontColor;
    }

    public void Init()
    {
        Clear();
    }
    
    public void OnSelectionChanged(bool selected)
    {
        text.color = selected ? GlobalSettings.I.HighlightedTextColor : GlobalSettings.I.DefaultFontColor;
    }

	public void SetText(string text)
    {
        this.text.text = text;
    }
}
