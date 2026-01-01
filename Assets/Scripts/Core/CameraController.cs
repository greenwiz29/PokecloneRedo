using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform target;
    CameraBoundsArea currentBounds;
    public static CameraController I { get; private set; }

    void Awake()
    {
        I = this;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desired = target.position;
        desired.z = transform.position.z;

        if (currentBounds != null)
        {
            desired = ClampToBounds(desired, currentBounds.Bounds);
        }

        transform.position = desired;
    }

    Vector3 ClampToBounds(Vector3 desired, Bounds bounds)
    {
        float camHalfHeight = Camera.main.orthographicSize;
        float camHalfWidth = camHalfHeight * Camera.main.aspect;

        // --- X AXIS ---
        float boundsWidth = bounds.size.x;
        if (boundsWidth <= camHalfWidth * 2f)
        {
            // Room narrower than screen → lock X
            desired.x = bounds.center.x;
        }
        else
        {
            desired.x = Mathf.Clamp(
                desired.x,
                bounds.min.x + camHalfWidth,
                bounds.max.x - camHalfWidth
            );
        }

        // --- Y AXIS ---
        float boundsHeight = bounds.size.y;
        if (boundsHeight <= camHalfHeight * 2f)
        {
            // Room shorter than screen → lock Y
            desired.y = bounds.center.y;
        }
        else
        {
            desired.y = Mathf.Clamp(
                desired.y,
                bounds.min.y + camHalfHeight,
                bounds.max.y - camHalfHeight
            );
        }

        desired.z = transform.position.z;
        transform.position = desired;

        return desired;
    }

    public void SetBounds(CameraBoundsArea bounds)
    {
        currentBounds = bounds;
    }

    public void ClearBounds()
    {
        currentBounds = null;
    }
}
