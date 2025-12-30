using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ItemCategories { RECOVERY, EVOLUTION, POKEBALLS, TMs, KEY, BADGE }

public class Inventory : MonoBehaviour, ISavable
{
    [SerializeField] List<ItemSlot> recoveryItemSlots;
    [SerializeField] List<ItemSlot> evolutionItemSlots;
    [SerializeField] List<ItemSlot> pokeballSlots;
    [SerializeField] List<ItemSlot> keyItemSlots;
    [SerializeField] List<ItemSlot> TMSlots;
    [SerializeField] List<ItemSlot> BadgeSlots;

    List<List<ItemSlot>> allSlots;

    public static List<string> ItemCategories { get; set; } = new List<string>()
    {
        "RECOVERY", "EVOLUTION", "POKEBALLS","TMs & HMs","KEY ITEMS","GYM BADGES"
    };

    public event Action OnUpdated;

    void Awake()
    {
        allSlots = new List<List<ItemSlot>>() { recoveryItemSlots, evolutionItemSlots, pokeballSlots, TMSlots, keyItemSlots, BadgeSlots };
    }

    public List<ItemSlot> GetSlotsByCategory(ItemCategories category)
    {
        return allSlots[(int)category];
    }

    public static Inventory GetPlayerInventory()
    {
        return GameController.I.Player.GetComponent<Inventory>();
    }

    public ItemBase GetItem(ItemCategories category, int itemIndex)
    {
        var currentSlots = GetSlotsByCategory(category);
        var item = currentSlots[itemIndex].Item;
        return item;
    }

    public ItemBase UseItem(ItemBase item, Pokemon target)
    {
        bool itemUsed = item.Use(target);
        if (itemUsed)
        {
            if (!item.IsReusable)
                RemoveItem(item);
            return item;
        }
        return null;
    }

    public void AddItem(ItemBase item, int count = 1)
    {
        var category = GetCategoryFromItem(item);

        var slots = GetSlotsByCategory(category);

        var itemSlot = slots.FirstOrDefault(slot => slot.Item == item);
        if (itemSlot != null)
        {
            itemSlot.Count += count;
        }
        else
        {
            slots.Add(new ItemSlot()
            {
                Item = item,
                Count = count
            });
        }

        OnUpdated?.Invoke();
    }

    public void RemoveItem(ItemBase item, int countToRemove = 1)
    {
        var selectedCategory = GetCategoryFromItem(item);
        var currentSlots = GetSlotsByCategory(selectedCategory);
        var itemSlot = currentSlots.First(i => i.Item == item);
        itemSlot.Count -= countToRemove;
        if (itemSlot.Count <= 0)
        {
            currentSlots.Remove(itemSlot);
        }

        OnUpdated?.Invoke();
    }

    public bool HasItem(ItemBase item)
    {
        var selectedCategory = GetCategoryFromItem(item);
        var currentSlots = GetSlotsByCategory(selectedCategory);

        return currentSlots.Exists(slot => slot.Item == item);
    }

    public int GetItemCount(ItemBase item)
    {
        var selectedCategory = GetCategoryFromItem(item);
        var currentSlots = GetSlotsByCategory(selectedCategory);
        var itemSlot = currentSlots.FirstOrDefault(slot => slot.Item == item);
        return itemSlot != null ? itemSlot.Count : 0;
    }

    public ItemCategories GetCategoryFromItem(ItemBase item)
    {
        if (item is RecoveryItem)
            return global::ItemCategories.RECOVERY;
        else if (item is EvolutionItem)
            return global::ItemCategories.EVOLUTION;
        else if (item is PokeballItem)
            return global::ItemCategories.POKEBALLS;
        else if (item is KeyItem)
            return global::ItemCategories.KEY;
        else if (item is GymBadge)
            return global::ItemCategories.BADGE;
        else
            return global::ItemCategories.TMs;
    }

    public object CaptureState()
    {
        var saveData = new InventorySaveData()
        {
            items = recoveryItemSlots.Select(i => i.GetSaveData()).ToList(),
            pokeballs = pokeballSlots.Select(i => i.GetSaveData()).ToList(),
            tms = TMSlots.Select(i => i.GetSaveData()).ToList(),
            keyItems = keyItemSlots.Select(i => i.GetSaveData()).ToList(),
            badges = BadgeSlots.Select(i => i.GetSaveData()).ToList(),
        };
        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as InventorySaveData;

        recoveryItemSlots = saveData.items.Select(i => new ItemSlot(i)).ToList();
        pokeballSlots = saveData.pokeballs.Select(i => new ItemSlot(i)).ToList();
        TMSlots = saveData.tms.Select(i => new ItemSlot(i)).ToList();
        keyItemSlots = saveData.keyItems.Select(i => new ItemSlot(i)).ToList();
        BadgeSlots = saveData.badges.Select(i => new ItemSlot(i)).ToList();

        allSlots = new List<List<ItemSlot>>() { recoveryItemSlots, pokeballSlots, TMSlots, keyItemSlots, BadgeSlots };

        OnUpdated?.Invoke();
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemSlot()
    {

    }

    public ItemSlot(ItemSaveData saveData)
    {
        count = saveData.count;
        item = ItemsDB.GetObjectByName(saveData.name);
    }

    public ItemSaveData GetSaveData()
    {
        var saveData = new ItemSaveData()
        {
            name = item.name,
            count = count
        };
        return saveData;
    }

    public ItemBase Item { get => item; set => item = value; }
    public int Count { get => count; set => count = value; }
}

[Serializable]
public class ItemSaveData
{
    public string name;
    public int count;
}

[Serializable]
public class InventorySaveData
{
    public List<ItemSaveData> items;
    public List<ItemSaveData> pokeballs;
    public List<ItemSaveData> tms;
    public List<ItemSaveData> keyItems;
    public List<ItemSaveData> badges;
}