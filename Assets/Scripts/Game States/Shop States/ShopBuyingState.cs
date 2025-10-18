using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GDEUtils.StateMachine;
using UnityEngine;

public class ShopBuyingState : State<GameController>
{
    [SerializeField] ShopUI shopUI;
    [SerializeField] WalletUI walletUI;
    [SerializeField] CountSelectorUI countSelectorUI;

    public static ShopBuyingState I { get; private set; }

    // Input
    public List<ItemBase> AvailableItems { get; set; }

    void Awake()
    {
        I = this;
    }

    GameController gc;
    Inventory inventory;
    bool browseItems = false;
    public override void Enter(GameController owner)
    {
        gc = owner;
        inventory = Inventory.GetPlayerInventory();

        browseItems = false;
        StartCoroutine(StartBuyingState());
    }

    public override void Execute()
    {
        if (browseItems)
            shopUI.HandleUpdate();
    }

    public override void Exit()
    {
        shopUI.gameObject.SetActive(false);
        walletUI.Close();
    }
    private IEnumerator StartBuyingState()
    {
        walletUI.Show();
        shopUI.Show(AvailableItems.OrderBy(item => item.SellPrice).ThenBy(item => item.Name).ToList(),
            item => StartCoroutine(BuyItem(item)),
            OnBackFromBuying);
        browseItems = true;
        yield break;
    }

    IEnumerator BuyItem(ItemBase item)
    {
        browseItems = false;

        int countToBuy = 1;
        yield return DialogManager.I.ShowDialogText($"How many would you like to buy?", waitForInput: false, autoClose: false);

        int maxCount = Mathf.FloorToInt(Wallet.I.Money / item.SellPrice);
        yield return countSelectorUI.Show(maxCount, item.SellPrice, false, selectedCount =>
        {
            countToBuy = selectedCount;
        },
        () =>
        {
            countToBuy = 0; // Cancelled
        });

        DialogManager.I.CloseDialog();
        if (countToBuy == 0)
        {
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

        browseItems = true;
    }

    void OnBackFromBuying()
    {
        gc.stateMachine.Pop();
    }
}
