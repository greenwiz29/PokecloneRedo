using System.Collections;
using GDEUtils.StateMachine;
using UnityEngine;

public class UseItemState : State<GameController>
{
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;


    public static UseItemState I { get; private set; }
    void Awake()
    {
        I = this;
    }

    GameController gc;
    Inventory inventory;
    public override void Enter(GameController owner)
    {
        gc = owner;
        StartCoroutine(UseItem());
    }

    private IEnumerator UseItem()
    {
        inventory = Inventory.GetPlayerInventory();

        // Use the item on the selected pokemon
        ItemBase item = inventoryUI.SelectedItem;
        var itemCategory = inventoryUI.Category;
        var pokemon = partyScreen.SelectedPokemon;

        ItemBase usedItem;
        switch (itemCategory)
        {
            case ItemCategories.RECOVERY:
                usedItem = inventory.UseItem(item, pokemon);
                if (usedItem != null)
                    yield return DialogManager.I.ShowDialogText($"You used {usedItem.Name} on {pokemon.Name}.");
                else
                    yield return DialogManager.I.ShowDialogText("It won't have any effect.");
                break;
            case ItemCategories.TMs:
                yield return HandleTMItems();
                usedItem = inventory.UseItem(item, pokemon);
                break;
            case ItemCategories.KEY:
                usedItem = inventory.UseItem(item, pokemon);
                break;
            case ItemCategories.POKEBALLS:
                usedItem = inventory.UseItem(item, null);
                break;
            case ItemCategories.EVOLUTION:
                var evolution = pokemon.CheckForEvolution(item);
                if (evolution != null)
                {
                    yield return EvolutionManager.I.Evolve(pokemon, evolution, null);
                    usedItem = inventory.UseItem(item, pokemon);
                }
                else
                {
                    yield return DialogManager.I.ShowDialogText($"It won't have any effect.");
                }
                break;
            default:
                usedItem = null;
                break;
        }

        gc.stateMachine.Pop();
    }

    IEnumerator HandleTMItems()
    {
        var item = inventoryUI.SelectedItem as TMItem;
        if (item == null)
        {
            yield break;
        }

        var pokemon = partyScreen.SelectedPokemon;
        if (pokemon.HasMove(item.Move))
        {
            yield return DialogManager.I.ShowDialogText($"{pokemon.Name} already knows {item.Move.Name}", false);
            yield break;
        }

        if (!item.CanBeTaught(pokemon))
        {
            yield return DialogManager.I.ShowDialogText($"{pokemon.Name} can't learn {item.Move.Name}", false);
            yield break;
        }

        if (pokemon.TryLearnMove(item.Move))
        {
            yield return DialogManager.I.ShowDialogText($"{pokemon.Name} learned {item.Move.Name}", false);
        }
        else
        {
            yield return DialogManager.I.ShowDialogText($"{pokemon.Name} is trying to learn {item.Move.Name}", false);
            yield return DialogManager.I.ShowDialogText($"But it cannot know more than {Pokemon.maxMoves} moves at once.", false);
            // yield return ChooseMoveToForget(pokemon, item.Move);

            // yield return new WaitUntil(() => state != InventoryUIState.MoveToForget);
        }
    }

}
