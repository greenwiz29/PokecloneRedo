using System.Collections.Generic;
using System.Linq;
using GDEUtils.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : SelectionUI<TextSlot>
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;
    [SerializeField] Image itemIcon;
    [SerializeField] TMP_Text itemDescription;
    [SerializeField] TMP_Text categoryText;
    [SerializeField] Image upArrow, downArrow;

    Inventory inventory;
    List<ItemSlotUI> slotUIList;
    RectTransform itemListRect;
    int selectedCategory = 0;
    const int itemsInViewport = 12;

    public ItemCategories Category => (ItemCategories)selectedCategory;
    public ItemBase SelectedItem => inventory.GetItem((ItemCategories)selectedCategory, selection);

    void Awake()
    {
        inventory = Inventory.GetPlayerInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    void Start()
    {
        UpdateItemList();
        SetSelectionSettings(GDEUtils.UI.SelectionMode.LIST);

        inventory.OnUpdated += UpdateItemList;
    }

    public override void HandleUpdate()
    {
        int prevCategory = selectedCategory;

        MenuSelectionMethods.HandleCategorySelection(ref selectedCategory, Inventory.ItemCategories.Count - 1);

        if (selectedCategory != prevCategory)
        {
            ResetSelection();
            UpdateItemList();
            categoryText.text = Inventory.ItemCategories[selectedCategory];
        }

        base.HandleUpdate();
    }

    private void ResetSelection()
    {
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);

        itemIcon.sprite = null;
        itemDescription.text = "";
    }

    private void UpdateItemList()
    {
        // Clear all existing items
        foreach (Transform child in itemList.transform)
        {
            Destroy(child.gameObject);
        }

        slotUIList = new List<ItemSlotUI>();
        foreach (var itemSlot in inventory.GetSlotsByCategory((ItemCategories)selectedCategory))
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);

            slotUIObj.SetData(itemSlot);
            slotUIList.Add(slotUIObj);
        }

        if (selection >= slotUIList.Count)
            selection = slotUIList.Count - 1;

        SetItems(slotUIList.Select(s => s.GetComponent<TextSlot>()).ToList());
        UpdateSelectionUI();
    }

    public override void UpdateSelectionUI()
    {
        base.UpdateSelectionUI();

        var slots = inventory.GetSlotsByCategory((ItemCategories)selectedCategory);
        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i == selection)
            {
                var item = slots[i].Item;
                itemIcon.sprite = item.Icon;
                itemDescription.text = item.Desc;

                HandleScrolling();
            }
        }
    }

    private void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport) return;

        float scrollPos = Mathf.Clamp(selection - itemsInViewport / 2, 0, selection) * slotUIList[0].Height;

        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPos);

        bool showUpArrow = selection > itemsInViewport / 2;
        upArrow.gameObject.SetActive(showUpArrow);

        bool showDownArrow = (selection + itemsInViewport / 2) < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);
    }
}
