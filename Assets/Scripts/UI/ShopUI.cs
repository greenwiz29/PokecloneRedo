using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;
    [SerializeField] Image itemIcon;
    [SerializeField] TMP_Text itemDescription;
    [SerializeField] Image upArrow, downArrow;

    List<ItemBase> availableItems;
    List<ItemSlotUI> slotUIList;
    int selectedItem = 0;
    const int itemsInViewport = 12;
    RectTransform itemListRect;
    Action<ItemBase> onItemSelected;
    Action onBack;

    void Awake()
    {
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    public void Show(List<ItemBase> availableItems, Action<ItemBase> onItemSelected, Action onBack)
    {
        gameObject.SetActive(true);
        this.availableItems = availableItems;
        this.onItemSelected = onItemSelected;
        this.onBack = onBack;
        UpdateItemList();
    }
    public void HandleUpdate()
    {
        int prevItem = selectedItem;

        MenuSelectionMethods.HandleListSelection(ref selectedItem, availableItems.Count);

        if (selectedItem != prevItem)
        {
            UpdateItemSelection(selectedItem);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            onItemSelected?.Invoke(availableItems[selectedItem]);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            onBack?.Invoke();
        }
    }

    private void UpdateItemList()
    {
        // Clear all existing items
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        slotUIList = new List<ItemSlotUI>();
        foreach (var item in availableItems)
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);

            slotUIObj.SetNameAndPrice(item);
            slotUIList.Add(slotUIObj);
        }

        if (selectedItem >= slotUIList.Count)
            selectedItem = slotUIList.Count - 1;

        UpdateItemSelection(selectedItem);
    }

    private void UpdateItemSelection(int selection)
    {
        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i == selection)
            {
                slotUIList[i].NameText.color = GlobalSettings.I.HighlightedTextColor;

                var item = availableItems[i];
                itemIcon.sprite = item.Icon;
                itemDescription.text = item.Desc;

                HandleScrolling();
            }
            else
            {
                slotUIList[i].NameText.color = GlobalSettings.I.DefaultFontColor;
            }
        }
    }

    private void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport) return;

        float scrollPos = Mathf.Clamp(selectedItem - itemsInViewport / 2, 0, selectedItem) * slotUIList[0].Height;

        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = selectedItem > itemsInViewport / 2;
        upArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = (selectedItem + itemsInViewport / 2) < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }
}
