using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogManager : MonoSingleton<DialogManager>
{
    [SerializeField] GameObject dialogBox;
    [SerializeField] TMP_Text dialogText;
    [SerializeField] int lettersPerSecond;
    [SerializeField] ChoiceBox choiceBox;

    public event Action OnShowDialog;
    public event Action OnDialogFinished;
    public bool IsShowing { get; private set; }

    private void SetDialogActive(bool active)
    {
        dialogBox.SetActive(active);
        IsShowing = active;
    }

    public void OpenDialog()
    {
        SetDialogActive(true);
        OnShowDialog?.Invoke();
    }

    public void CloseDialog()
    {
        SetDialogActive(false);
    }

    public IEnumerator ShowDialogText(string message, bool waitForInput = true, bool autoClose = true, List<string> choices = null, Action<int> onChoiceSelected = null)
    {
        yield return new WaitForEndOfFrame();

        OpenDialog();

        yield return TypeDialog(message);
        if (waitForInput)
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));


        if (choices != null && choices.Count > 1)
        {
            yield return choiceBox.ShowChoices(choices, onChoiceSelected);
        }

        if (autoClose)
        {
            CloseDialog();
        }
        OnDialogFinished?.Invoke();
    }

    public IEnumerator ShowDialog(Dialog dialog, List<string> choices = null, Action<int> onChoiceSelected = null)
    {
        yield return new WaitForEndOfFrame();

        OpenDialog();

        foreach (var line in dialog.Lines)
        {
            yield return TypeDialog(line);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        }

        if (choices != null && choices.Count > 1)
        {
            yield return choiceBox.ShowChoices(choices, onChoiceSelected);
        }

        CloseDialog();
        OnDialogFinished?.Invoke();
    }

    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
    }

    public void HandleUpdate()
    {

    }
}
