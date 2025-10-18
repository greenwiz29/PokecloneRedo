using System.Collections;
using System.Linq;
using GDEUtils.StateMachine;
using UnityEngine;

public class UseItemState : State<GameController>
{
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;

    public static UseItemState I { get; private set; }

    // Output
    public bool ItemUsed { get; private set; }

    void Awake()
    {
        I = this;
    }

    GameController gc;
    Inventory inventory;
    public override void Enter(GameController owner)
    {
        gc = owner;
        ItemUsed = false;
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
                {
                    ItemUsed = true;
                    yield return DialogManager.I.ShowDialogText($"You used {usedItem.Name} on {pokemon.Name}.");
                }
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
            yield return DialogManager.I.ShowDialogText($"Choose a move to forget.", true, false);
            MoveToForgetState.I.NewMove = item.Move;
            MoveToForgetState.I.CurrentMoves = pokemon.Moves.Select(m => m.Base).ToList();
            yield return gc.stateMachine.PushAndWait(MoveToForgetState.I);
            int moveIndex = MoveToForgetState.I.Selection;
            var moveToLearn = item.Move;
            if (moveIndex == -1 || moveIndex == Pokemon.maxMoves)
            {
                // new move was selected, or player canceled out.
                // TODO: prompt if new move should be abandoned
                yield return DialogManager.I.ShowDialogText($"{pokemon.Name} did not learn {moveToLearn.Name}", false);
            }
            else
            {
                // Forget selected move and learn new move
                yield return DialogManager.I.ShowDialogText($"{pokemon.Name} forgot {pokemon.Moves[moveIndex].Base.Name} and learned {moveToLearn.Name}", false);

                pokemon.Moves[moveIndex] = new Move(moveToLearn);
            }
        }
    }

}
