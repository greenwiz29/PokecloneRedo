using System.Collections;
using GDEUtils.StateMachine;
using UnityEngine;

public class InventoryState : State<GameController>
{
    [SerializeField] InventoryUI inventoryUI;

    public static InventoryState I { get; private set; }

    // Output
    public ItemBase SelectedItem { get; private set; }

    void Awake()
    {
        I = this;
    }

    void Start()
    {
        inventory = Inventory.GetPlayerInventory();
    }

    GameController gc;
    Inventory inventory;
    public override void Enter(GameController owner)
    {
        gc = owner;
        SelectedItem = null;
        inventoryUI.gameObject.SetActive(true);
        inventoryUI.OnSelected += OnItemSelected;
        inventoryUI.OnBack += OnBack;
    }

    public override void Execute()
    {
        inventoryUI.HandleUpdate();
    }

    public override void Exit()
    {
        inventoryUI.gameObject.SetActive(false);
        inventoryUI.OnSelected -= OnItemSelected;
        inventoryUI.OnBack -= OnBack;
    }

    private void OnBack()
    {
        SelectedItem = null;
        gc.stateMachine.Pop();
    }

    private void OnItemSelected(int obj)
    {
        if (inventoryUI.Category == ItemCategories.KEY)
            return; // For now, key items can't be used directly
        else
        {
            SelectedItem = inventoryUI.SelectedItem;
            if (gc.stateMachine.GetPrevState() != ShopSellingState.I)
                StartCoroutine(SelectPokemonAndUseItem());
            else
                gc.stateMachine.Pop();
        }
    }

    IEnumerator SelectPokemonAndUseItem()
    {
        var prevState = gc.stateMachine.GetPrevState();

        if (prevState == BattleState.I)
        {
            if (!SelectedItem.CanUseInBattle)
            {
                yield return DialogManager.I.ShowDialogText("You can't use that here!");
                yield break;
            }
        }
        else
        {
            if (!SelectedItem.CanUseOutOfBattle)
            {
                yield return DialogManager.I.ShowDialogText("You can't use that here!");
                yield break;
            }
        }

        if (SelectedItem is PokeballItem)
        {
            inventory.UseItem(SelectedItem, null);
            gc.stateMachine.Pop();
            yield break;
        }

        yield return gc.stateMachine.PushAndWait(PartyState.I);

        if (prevState == BattleState.I)
        {
            if (UseItemState.I.ItemUsed)
            {
                gc.stateMachine.Pop();
            }
        }
    }
}
