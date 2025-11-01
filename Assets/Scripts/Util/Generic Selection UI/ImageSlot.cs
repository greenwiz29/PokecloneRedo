using UnityEngine;
using UnityEngine.UI;

public class ImageSlot : MonoBehaviour, ISelectableItem
{
    Image bgImage;
    void Awake()
    {
        bgImage = GetComponent<Image>();
        originalColor = bgImage.color;
    }

    Color originalColor;
    public void Clear()
    {
        bgImage.color = originalColor;
    }

    public void Init()
    {
    }
    
    public void OnSelectionChanged(bool selected)
    {
        bgImage.color = selected ? GlobalSettings.I.HighlightedImageColor : originalColor;
    }
}
