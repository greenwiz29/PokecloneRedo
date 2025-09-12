using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceBox : MonoBehaviour
{
    [SerializeField] ChoiceText choiceTextPrefab;

    List<ChoiceText> choiceTexts;

    bool choiceSelected = false;
    int currentChoice;

    public IEnumerator ShowChoices(List<string> choices, Action<int> onChoiceSelected)
    {
        currentChoice = 0;
        choiceSelected = false;
        gameObject.SetActive(true);

        // Delete existing choices
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        choiceTexts = new List<ChoiceText>();
        foreach (var choice in choices)
        {
            var choiceText = Instantiate(choiceTextPrefab, transform);
            choiceText.Text.text = choice;

            choiceTexts.Add(choiceText);
        }
        UpdateMenuSelection(currentChoice);

        yield return new WaitUntil(() => choiceSelected == true);

        onChoiceSelected?.Invoke(currentChoice);
        gameObject.SetActive(false);
    }

    void Update()
    {
        int prev = currentChoice;
        MenuSelectionMethods.HandleListSelection(ref currentChoice, choiceTexts.Count - 1);

        if (currentChoice != prev)
            UpdateMenuSelection(currentChoice);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            choiceSelected = true;
        }
    }

    public void UpdateMenuSelection(int selection)
    {
        for (int i = 0; i < choiceTexts.Count; i++)
        {
            choiceTexts[i].SetSelected(i == selection);
        }
    }

}
