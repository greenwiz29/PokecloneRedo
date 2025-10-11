using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GDEUtils.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection, PartySelection, Busy, MoveToForget }

public class InventoryUI : SelectionUI<TextSlot>
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;
    [SerializeField] Image itemIcon;
    [SerializeField] TMP_Text itemDescription;
    [SerializeField] TMP_Text categoryText;
    [SerializeField] Image upArrow, downArrow;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] MoveToForgetUI moveToForgetUI;

    Inventory inventory;
    List<ItemSlotUI> slotUIList;
    RectTransform itemListRect;
    int selectedCategory = 0;
    const int itemsInViewport = 12;
    InventoryUIState state;
    private MoveBase moveToLearn;
    Action<ItemBase> OnItemUsed;

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

        inventory.OnUpdated += UpdateItemList;
    }

    public override void HandleUpdate(SelectionMode mode = SelectionMode.LIST)
    {
        int prevCategory = selectedCategory;

        MenuSelectionMethods.HandleCategorySelection(ref selectedCategory, Inventory.ItemCategories.Count - 1);

        if (selectedCategory != prevCategory)
        {
            ResetSelection();
            UpdateItemList();
            categoryText.text = Inventory.ItemCategories[selectedCategory];
        }

        base.HandleUpdate(mode);
    }

    // public void HandleUpdate(Action onBack, Action<ItemBase> onItemUsed = null)
    // {
    //     OnItemUsed = onItemUsed;

    //     switch (state)
    //     {
    //         case InventoryUIState.ItemSelection:
    //             int prevItem = selection;
    //             int prevCategory = selectedCategory;

    //             MenuSelectionMethods.HandleCategorySelection(ref selectedCategory, Inventory.ItemCategories.Count - 1);
    //             MenuSelectionMethods.HandleListSelection(ref selection, inventory.GetSlotsByCategory((ItemCategories)selectedCategory).Count - 1);

    //             if (selectedCategory != prevCategory)
    //             {
    //                 ResetSelection();
    //                 UpdateItemList();
    //                 categoryText.text = Inventory.ItemCategories[selectedCategory];
    //             }
    //             else if (selection != prevItem)
    //             {
    //                 UpdateItemSelection(selection);
    //             }

    //             if (Input.GetKeyDown(KeyCode.Space))
    //             {
    //                 StartCoroutine(ItemSelected());
    //             }
    //             else if (Input.GetKeyDown(KeyCode.Escape))
    //             {
    //                 onBack?.Invoke();
    //             }
    //             break;
    //         case InventoryUIState.PartySelection:
    //             Action onMemberSelected = () =>
    //             {
    //                 StartCoroutine(UseItem());
    //                 ClosePartyScreen();
    //             };
    //             Action onPartyBack = () =>
    //             {
    //                 ClosePartyScreen();
    //             };
    //             // partyScreen.HandleUpdate(onMemberSelected, onPartyBack);
    //             break;
    //         case InventoryUIState.MoveToForget:
    //             Action<int> onMoveSelected = (moveIndex) =>
    //             {
    //                 StartCoroutine(OnMoveToForgetSelected(moveIndex));
    //             };
    //             moveToForgetUI.HandleUpdate(onMoveSelected);
    //             break;
    //     }

    // }

    private void ResetSelection()
    {
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);

        itemIcon.sprite = null;
        itemDescription.text = "";
    }

    IEnumerator ItemSelected()
    {
        state = InventoryUIState.Busy;

        var item = inventory.GetItem((ItemCategories)selectedCategory, selection);

        if (GameController.I.State == GameState.Shop)
        {
            OnItemUsed?.Invoke(item);
            state = InventoryUIState.ItemSelection;
            yield break;
        }
        else if (GameController.I.State == GameState.Battle)
        {
            // In Battle
            if (!item.CanUseInBattle)
            {
                yield return DialogManager.I.ShowDialogText($"You can't use that right now!");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }
        else
        {
            if (!item.CanUseOutOfBattle)
            {
                yield return DialogManager.I.ShowDialogText($"You can't use that right now!");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }

        if ((ItemCategories)selectedCategory == ItemCategories.POKEBALLS)
        {
            // yield return UseItem();
            state = InventoryUIState.ItemSelection;
        }
        else
        {
            OpenPartyScreen();

            if (item is TMItem)
            {
                // Show if TM is useable
                partyScreen.ShowIfTMIsUseable(item as TMItem);
            }
        }
    }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = InventoryUIState.Busy;

        moveToLearn = newMove;
        yield return DialogManager.I.ShowDialogText($"Choose a move to forget.", true, false);
        moveToForgetUI.gameObject.SetActive(true);
        moveToForgetUI.SetMoveData(pokemon.Moves.Select(m => m.Base).ToList(), newMove);

        state = InventoryUIState.MoveToForget;
    }

    private IEnumerator OnMoveToForgetSelected(int moveIndex)
    {
        state = InventoryUIState.Busy;
        var pokemon = partyScreen.SelectedPokemon;

        DialogManager.I.CloseDialog();
        moveToForgetUI.gameObject.SetActive(false);
        if (moveIndex == Pokemon.maxMoves)
        {
            // new move was selected
            // TODO: prompt if new move should be abandoned
            yield return DialogManager.I.ShowDialogText($"{pokemon.Name} did not learn {moveToLearn.Name}", false);
        }
        else
        {
            // Forget selected move and learn new move
            yield return DialogManager.I.ShowDialogText($"{pokemon.Name} forgot {pokemon.Moves[moveIndex].Base.Name} and learned {moveToLearn.Name}", false);

            pokemon.Moves[moveIndex] = new Move(moveToLearn);
        }

        moveToLearn = null;
        state = InventoryUIState.ItemSelection;
    }

    private void OpenPartyScreen()
    {
        state = InventoryUIState.PartySelection;
        partyScreen.gameObject.SetActive(true);
        partyScreen.SetPartyData();
    }

    private void ClosePartyScreen()
    {
        state = InventoryUIState.ItemSelection;
        partyScreen.gameObject.SetActive(false);
        partyScreen.ClearMessages();
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
