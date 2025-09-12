using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CountSelectorUI : MonoBehaviour
{
    [SerializeField] TMP_Text countText;
    [SerializeField] TMP_Text priceText;

    bool selected;
    bool cancelled;
    int currentCount;

    int maxCount;
    int pricePerItem;
    bool selling;

    public IEnumerator Show(int maxCount, int pricePerItem, bool isSelling, Action<int> onCountSelected, Action onCancelled)
    {
        selected = false;
        currentCount = 1;
        this.maxCount = maxCount;
        this.pricePerItem = pricePerItem;
        selling = isSelling;

        gameObject.SetActive(true);
        SetValues();

        yield return new WaitUntil(() => selected);

        if (!cancelled)
            onCountSelected?.Invoke(currentCount);
        else
            onCancelled?.Invoke();

        gameObject.SetActive(false);
    }

    void Update()
    {
        int prevCount = currentCount;
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentCount++;
            if (currentCount > maxCount) currentCount = 1;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentCount--;
            if (currentCount < 1) currentCount = maxCount;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentCount = Mathf.Max(1, currentCount - 10);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentCount = Mathf.Min(maxCount, currentCount + 10);
        }

        if (prevCount != currentCount)
        {
            SetValues();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            cancelled = false;
            selected = true;
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            cancelled = true;
            selected = true;
        }

    }

    private void SetValues()
    {
        countText.text = "x " + currentCount;
        string priceTextValue = $"${(selling ? (currentCount * pricePerItem * GlobalSettings.I.SellFactor) : (currentCount * pricePerItem))}";
        priceText.text = priceTextValue;
    }
}
