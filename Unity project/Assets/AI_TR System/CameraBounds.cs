using UnityEngine;

public class CameraBounds : MonoBehaviour
{
    public float moveSpeed = 150f;

    public Vector3 minBounds = new Vector3(-10, 5, -10);
    public Vector3 maxBounds = new Vector3(10, 20, 10);

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = (minBounds + maxBounds) / 2f;
        Vector3 size = maxBounds - minBounds;
        Gizmos.DrawWireCube(center, size);
    }


    void Update()
    {
        // // 1. Get input (WASD or arrow keys)
        // float h = Input.GetAxis("Horizontal");  // A/D or Left/Right
        // float v = Input.GetAxis("Vertical");    // W/S or Up/Down

        // // 2. Move the camera
        // Vector3 move = new Vector3(h, 0, v) * moveSpeed * Time.deltaTime;
        // transform.position += move;

        // 3. Clamp position within bounds
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, minBounds.x, maxBounds.x),
            Mathf.Clamp(transform.position.y, minBounds.y, maxBounds.y),
            Mathf.Clamp(transform.position.z, minBounds.z, maxBounds.z)
        );
    }
}
