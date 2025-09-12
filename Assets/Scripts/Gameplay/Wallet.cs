using System;
using UnityEngine;

public class Wallet : MonoSingleton<Wallet>, ISavable
{
    [SerializeField] int money;
    public int Money => money;

    public event Action OnMoneyChanged;

    public void AddMoney(int amount)
    {
        money += amount;
        OnMoneyChanged?.Invoke();
    }

    public void RemoveMoney(int amount)
    {
        if (amount <= money)
        {
            money -= amount;
            OnMoneyChanged?.Invoke();
        }
        else
        {
            Debug.LogWarning("Not enough money to remove.");
        }
    }

    public bool HasMoney(int amount)
    {
        return money >= amount;
    }

    public object CaptureState()
    {
        return money;
    }

    public void RestoreState(object state)
    {
        money = (int)state;
        OnMoneyChanged?.Invoke();
    }
}
