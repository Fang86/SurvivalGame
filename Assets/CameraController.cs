using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private GameObject player;
    private GameObject gun;

    public float sensitivity = 7f;
    public float aimingSensitivity = 5f;
    private float effectiveSensitivity;

    private float pitch = 0f; // rotation around X-axis (up/down)
    private float yaw = 0f;   // rotation around Y-axis (left/right)

    void Start()
    {
        player = transform.parent.parent.gameObject;
        gun = player.GetComponentInChildren<Gun>().gameObject;

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

            // Apply rotation to the player: pitch around X, yaw around Y
            player.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            gun.transform.rotation = Quaternion.Euler(0f, yaw+270, -pitch);  // pitch around z because the gun is rotated 270 degrees around the Y
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
