using System;
using TMPro;
using UnityEngine;

public class WalletUI : MonoBehaviour
{
    [SerializeField] TMP_Text moneyText;

	void Start()
	{
		Wallet.I.OnMoneyChanged += SetMoneyText;
	}
	public void Show()
    {
        gameObject.SetActive(true);
        SetMoneyText();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void SetMoneyText()
    {
        moneyText.text = "$" + Wallet.I.Money;
    }
}
