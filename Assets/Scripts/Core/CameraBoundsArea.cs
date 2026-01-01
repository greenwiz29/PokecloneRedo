using UnityEngine;

public class CameraBoundsArea : MonoBehaviour
{
    public Bounds Bounds => GetComponent<Collider2D>().bounds;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CameraController.I.SetBounds(this);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CameraController.I.ClearBounds();
        }
    }

}
