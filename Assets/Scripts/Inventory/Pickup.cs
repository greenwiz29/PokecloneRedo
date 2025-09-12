using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class Pickup : MonoBehaviour, IInteractable, ISavable
{
    [SerializeField] ItemBase item;

    public bool Used { get; set; }

    SpriteRenderer spriteRenderer;
    BoxCollider2D boxCollider;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public IEnumerator Interact(Transform initiator)
    {
        if (!Used)
        {
            initiator.GetComponent<Inventory>().AddItem(item);
            Used = true;

            HideItem();

            yield return DialogManager.I.ShowDialogText($"You found {item.Name}!");
        }

        yield break;
    }

    private void HideItem()
    {
        spriteRenderer.enabled = false;
        boxCollider.enabled = false;
    }

    public object CaptureState()
    {
        return Used;
    }

    public void RestoreState(object state)
    {
        Used = (bool)state;
        if (Used)
        {
            HideItem();
        }
    }
}
