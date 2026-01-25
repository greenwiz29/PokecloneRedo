using UnityEngine;

public class ShadowSortFollow : MonoBehaviour
{
    [SerializeField] SpriteRenderer target; // main sprite
    [SerializeField] SpriteRenderer shadowRenderer; 

    void LateUpdate()
    {
        if (target == null) return;

        shadowRenderer.sortingOrder = target.sortingOrder - 1;
    }
}
