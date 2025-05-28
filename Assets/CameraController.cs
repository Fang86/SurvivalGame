using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float sensitivity = 7f;
    public float aimingSensitivity = 5f;

    private float effectiveSensitivity;

    private float pitch = 0f; // rotation around X-axis (up/down)
    private float yaw = 0f;   // rotation around Y-axis (left/right)

    void Start()
    {
        effectiveSensitivity = sensitivity;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            yaw += mouseDelta.x * effectiveSensitivity * Time.deltaTime;
            pitch -= mouseDelta.y * effectiveSensitivity * Time.deltaTime;

            // Clamp pitch to avoid flipping over
            pitch = Mathf.Clamp(pitch, -89.9f, 89.9f);

            // Apply rotation: pitch around X, yaw around Y
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }

    public void SetAimingSensitivity()
    {
        effectiveSensitivity = aimingSensitivity;
    }

    public void SetEffectiveSensitivity(float sens)
    {
        effectiveSensitivity = sens;
    }

    public void SetDefaultSensitivity()
    {
        effectiveSensitivity = sensitivity;
    }
}
