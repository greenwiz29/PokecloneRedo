using TMPro;
using UnityEngine;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] TMP_Text nameText, countText;

    public TMP_Text NameText => nameText;
    public TMP_Text CountText => countText;
    public float Height { get; private set; }

    RectTransform rect;

    public void SetData(ItemSlot itemSlot)
    {
        nameText.text = itemSlot.Item.Name;
        countText.text = $"x {itemSlot.Count}";
        rect = GetComponent<RectTransform>();
        Height = rect.rect.height;
    }
    
    public void SetNameAndPrice(ItemBase item)
    {
        nameText.text = item.Name;
        countText.text = $"${item.SellPrice}";
        rect = GetComponent<RectTransform>();
        Height = rect.rect.height;
    }
}
