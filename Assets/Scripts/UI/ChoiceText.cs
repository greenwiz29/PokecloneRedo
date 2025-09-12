using TMPro;
using UnityEngine;

public class ChoiceText : MonoBehaviour
{
    TMP_Text text;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
        text.color = GlobalSettings.I.DefaultFontColor;
    }

    public TMP_Text Text => text;

    public void SetSelected(bool selected)
    {
        text.color = selected ? GlobalSettings.I.HighlightedColor : GlobalSettings.I.DefaultFontColor;
    }
}
