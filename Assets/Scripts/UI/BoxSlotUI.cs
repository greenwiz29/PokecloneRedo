using UnityEngine;
using UnityEngine.UI;

public class BoxSlotUI : MonoBehaviour
{
    
    [SerializeField] Image image;

    Color originalColor;
    public void SetData(Pokemon pokemon)
    {
        image.sprite = pokemon.FrontSprite;
        image.color = Color.white;
    }

    public void ClearData()
    {
        image.sprite = null;
        image.color = GlobalSettings.I.Transparent;
    }
}
