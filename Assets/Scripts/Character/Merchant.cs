using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Merchant : MonoBehaviour
{
    [SerializeField] List<ItemBase> availableItems;

    public List<ItemBase> AvailableItems => availableItems;
    
    public IEnumerator Trade()
    {
        ShopMenuState.I.AvailableItems = AvailableItems;
        yield return GameController.I.stateMachine.PushAndWait(ShopMenuState.I);
    }
}
