using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float sensitivity = 100f;

    private float pitch = 0f; // rotation around X-axis (up/down)
    private float yaw = 0f;   // rotation around Y-axis (left/right)

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        yaw += mouseDelta.x * sensitivity * Time.deltaTime;
        pitch -= mouseDelta.y * sensitivity * Time.deltaTime;

        // Clamp pitch to avoid flipping over
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        // Apply rotation: pitch around X, yaw around Y
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
