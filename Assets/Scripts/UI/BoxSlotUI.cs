using UnityEngine;
using UnityEngine.UI;

public class BoxSlotUI : MonoBehaviour
{
    
    [SerializeField] Image image;

    public void SetData(Pokemon pokemon)
    {
        image.sprite = pokemon.Base.FrontSprite;
    }

    public void Clear()
    {
        image.sprite = null;
        image.color = Color.clear;
    }
}
