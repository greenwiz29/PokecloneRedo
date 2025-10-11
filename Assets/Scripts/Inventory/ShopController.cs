using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ShopState { Menu, Buying, Selling, Busy }
public class ShopController : MonoSingleton<ShopController>
{
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] ShopUI shopUI;
    [SerializeField] WalletUI walletUI;
    [SerializeField] CountSelectorUI countSelectorUI;

    public event Action OnStart;
    public event Action OnEnd;

    Merchant currentMerchant;
    ShopState state;

    Inventory inventory;
    void Start()
    {
        inventory = Inventory.GetPlayerInventory();
    }

    public IEnumerator StartTrading(Merchant merchant)
    {
        currentMerchant = merchant;
        OnStart?.Invoke();
        yield return StartCoroutine(StartMenuState());
    }

    IEnumerator StartMenuState()
    {
        state = ShopState.Menu;
        int selectedChoice = 0;
        yield return DialogManager.I.ShowDialogText("How can I help you today?", waitForInput: false, choices: new List<string> { "Buy", "Sell", "Leave" }, onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        switch (selectedChoice)
        {
            case 0: // Buy
                walletUI.Show();
                shopUI.Show(currentMerchant.AvailableItems.OrderBy(item => item.SellPrice).ThenBy(item => item.Name).ToList(),
                    item => StartCoroutine(BuyItem(item)),
                    OnBackFromBuying);

                state = ShopState.Buying;
                break;
            case 1: // Sell
                // Show player's inventory
                inventoryUI.gameObject.SetActive(true);
                state = ShopState.Selling;
                break;
            case 2: // Leave
                yield return DialogManager.I.ShowDialogText("Thank you for your business!");
                OnEnd?.Invoke();
                yield break;
        }
    }

    public void HandleUpdate()
    {
        switch (state)
        {
            case ShopState.Selling:
                // Handle selling logic
                // inventoryUI.HandleUpdate(OnBackFromSelling, (selectedItem) =>
                // {
                //     StartCoroutine(SellItem(selectedItem));
                // });
                break;
            case ShopState.Buying:
                shopUI.HandleUpdate();
                break;
        }
    }

    void OnBackFromSelling()
    {
        StartCoroutine(StartMenuState());
        inventoryUI.gameObject.SetActive(false);
    }

    IEnumerator SellItem(ItemBase item)
    {
        state = ShopState.Busy;

        if (!item.CanSell)
        {
            yield return DialogManager.I.ShowDialogText("You can't sell this item.");
            state = ShopState.Selling;
            yield break;
        }

        int itemCount = inventory.GetItemCount(item);
        int countToSell = 1;
        if (itemCount > 1)
        {
            yield return DialogManager.I.ShowDialogText($"How many would you like to sell?", waitForInput: false, autoClose: false);

            yield return countSelectorUI.Show(itemCount, item.SellPrice, true, selectedCount =>
            {
                countToSell = selectedCount;
            }, () => {
                countToSell = 0; // Cancelled
            });
        }
        if(countToSell == 0)
        {
            DialogManager.I.CloseDialog();
            state = ShopState.Selling;
            yield break;
        }

        walletUI.Show();

        int sellPrice = Mathf.RoundToInt(item.SellPrice * GlobalSettings.I.SellFactor) * countToSell;
        int selectedChoice = 0;
        yield return DialogManager.I.ShowDialogText($"I can give you ${sellPrice} for that. Do you still want to sell {(countToSell > 1 ? "them" : "it")}?", waitForInput: false, choices: new List<string> { "Yes", "No" }, onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        if (selectedChoice == 0) // Yes
        {
            inventory.RemoveItem(item, countToSell);
            Wallet.I.AddMoney(sellPrice);
            yield return DialogManager.I.ShowDialogText($"Sold {item.Name} for ${sellPrice}.");
        }
        else // No
        {
            yield return DialogManager.I.ShowDialogText("Alright, maybe next time.");
        }

        walletUI.Close();
        state = ShopState.Selling;
    }

    IEnumerator BuyItem(ItemBase item)
    {
        state = ShopState.Busy;

        int countToBuy = 1;
        yield return DialogManager.I.ShowDialogText($"How many would you like to buy?", waitForInput: false, autoClose: false);

        int maxCount = Mathf.FloorToInt(Wallet.I.Money / item.SellPrice);
        yield return countSelectorUI.Show(maxCount, item.SellPrice, false, selectedCount =>
        {
            countToBuy = selectedCount;
        },
        () => {
            countToBuy = 0; // Cancelled
        });

        DialogManager.I.CloseDialog();
        if(countToBuy == 0)
        {
            state = ShopState.Buying;
            yield break;
        }

        int totalPrice = item.SellPrice * countToBuy;
        if (Wallet.I.HasMoney(totalPrice))
        {
            walletUI.Show();
            int selectedChoice = 0;
            yield return DialogManager.I.ShowDialogText($"That will be ${totalPrice}. Do you still want to buy {(countToBuy > 1 ? "them" : "it")}?", waitForInput: false, choices: new List<string> { "Yes", "No" }, onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

            if (selectedChoice == 0) // Yes
            {
                Wallet.I.RemoveMoney(totalPrice);
                inventory.AddItem(item, countToBuy);
                yield return DialogManager.I.ShowDialogText($"Bought {item.Name} x{countToBuy} for ${totalPrice}.");
            }
            else // No
            {
                yield return DialogManager.I.ShowDialogText("Alright, maybe next time.");
            }
            walletUI.Close();
        }
        else
        {
            yield return DialogManager.I.ShowDialogText("You don't have enough money for that.");
        }

        state = ShopState.Buying;
    }

    void OnBackFromBuying()
    {
        StartCoroutine(StartMenuState());
        shopUI.gameObject.SetActive(false);
        walletUI.Close();
    }
}
