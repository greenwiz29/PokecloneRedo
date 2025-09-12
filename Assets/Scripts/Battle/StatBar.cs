using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StatBar : MonoBehaviour
{
    [SerializeField] GameObject stat;
    [SerializeField] Color fullColor;
    [SerializeField] Color emptyColor;

    public bool IsUpdating { get; private set; }

    Image image;

    void Awake()
	{
        image = stat.GetComponent<Image>();
	}
	public void SetStat(float statValueNormalized)
    {
        stat.transform.localScale = new Vector3(statValueNormalized, 1);
        // image.color = Color.Lerp(fullColor, emptyColor, statValueNormalized);
    }

    public IEnumerator SetStatSmooth(float newStatValue)
    {
        IsUpdating = true;
        var sequence = DOTween.Sequence();
        sequence.Append(stat.transform.DOScaleX(newStatValue, 1f));

        yield return sequence.WaitForCompletion();        
        IsUpdating = false;
    }
}
