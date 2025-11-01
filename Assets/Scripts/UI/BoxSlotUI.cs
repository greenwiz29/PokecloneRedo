using UnityEngine;
using UnityEngine.UI;

public class BoxSlotUI : MonoBehaviour
{
    
    [SerializeField] Image image;

    Color originalColor;
    public void SetData(Pokemon pokemon)
    {
        image.sprite = pokemon.Base.FrontSprite;
        image.color = Color.white;
    }

    public void ClearData()
    {
        image.sprite = null;
        image.color = new Color(255, 255, 255, 0);
    }
}
