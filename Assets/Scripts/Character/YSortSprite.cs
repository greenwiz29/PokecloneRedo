using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSortSprite : MonoBehaviour
{
    SpriteRenderer sr;

    // Multiplier lets you tune precision (tile size based)
    const int SortingPrecision = 16;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // Lower Y → render in front
        sr.sortingOrder = Mathf.RoundToInt(-transform.position.y * SortingPrecision);
    }
}
