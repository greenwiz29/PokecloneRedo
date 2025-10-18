using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] int wordsPerSecond;
    [SerializeField] float dialogDelay;

    [SerializeField] TMP_Text dialogText, ppText, typeText, descText, yesText, noText;
    [SerializeField] GameObject actionSelector, moveSelector, moveDetails, choiceBox;
    [SerializeField] List<TMP_Text> actionTexts, moveTexts;

    public int ActionCount => actionTexts.Count;

    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }

    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach (string word in dialog.Split(' '))
        {
            dialogText.text += word + ' ';
            yield return new WaitForSeconds(1f / wordsPerSecond);
        }

        yield return new WaitForSeconds(dialogDelay);
    }

    public void EnableDialogText(bool enable)
    {
        dialogText.enabled = enable;
    }

    public void EnableActionSelector(bool enable)
    {
        actionSelector.SetActive(enable);
    }

    public void EnableMoveSelector(bool enable)
    {
        moveSelector.SetActive(enable);
        EnableMoveDetails(enable);
    }

    public void EnableMoveDetails(bool enable)
    {
        moveDetails.SetActive(enable);
    }

    public void EnableChoiceBox(bool enable)
    {
        choiceBox.SetActive(enable);
    }

    public bool IsChoiceBoxEnabled => choiceBox.activeSelf;

    public void UpdateActionSelection(int selection)
    {
        for (int i = 0; i < actionTexts.Count; i++)
        {
            if (i == selection)
                actionTexts[i].color = GlobalSettings.I.HighlightedColor;
            else
                actionTexts[i].color = GlobalSettings.I.DefaultFontColor;
        }
    }

    public void UpdateChoiceBoxSelection(bool yesSelected)
    {
        if (yesSelected)
        {
            yesText.color = GlobalSettings.I.HighlightedColor;
            noText.color = GlobalSettings.I.DefaultFontColor;
        }
        else
        {
            yesText.color = GlobalSettings.I.DefaultFontColor;
            noText.color = GlobalSettings.I.HighlightedColor;
        }
    }

    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (i < moves.Count)
                moveTexts[i].text = moves[i].Base.Name;
            else
                moveTexts[i].text = "-";
        }
    }

    internal void UpdateMoveSelection(int selection, Move move)
	{
		for (int i = 0; i < moveTexts.Count; i++)
		{
			if (i == selection)
				moveTexts[i].color = GlobalSettings.I.HighlightedColor;
			else
				moveTexts[i].color = GlobalSettings.I.DefaultFontColor;
		}
		UpdateMoveDetails(move);
	}

	public void UpdateMoveDetails(Move move)
	{
		ppText.text = $"PP {move.PP}/{move.MaxPP}";
		typeText.text = move.Base.Type.ToString();
        descText.text = move.Base.Desc;

		if (move.PP == 0)
            ppText.color = Color.red;
        else if (move.PP < move.MaxPP * 0.25)
            ppText.color = Color.yellow;
        else
            ppText.color = Color.black;
	}
}
