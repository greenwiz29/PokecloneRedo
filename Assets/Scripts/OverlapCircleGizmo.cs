using UnityEngine;

public class OverlapCircleGizmos : MonoBehaviour {

    public float radius = 1f;

    void OnDrawGizmos() {
        Gizmos.color = Color.yellow; // Set the color of the Gizmos
        Gizmos.DrawWireSphere(transform.position, radius); // Draw the circle at the position of the game object
    }
}