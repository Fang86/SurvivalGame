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

    [Header("Gun Position Settings")]
    public Vector3 gunOffset = new Vector3(0.5f, -0.3f, 0.8f); // Right, Down, Forward
    public float smoothSpeed = 10f;

    private bool isKicking = false;
    private Vector3 kickRotationOffset;
    public float returnSpeed = 5f;

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
            transform.localEulerAngles += kickRotationOffset;
            //UpdateWeaponPosition(pitch);
            UpdateWeaponPosition();
            gun.transform.rotation = Quaternion.Euler(0f, yaw + 270, -pitch);  // pitch around z because the gun is rotated 270 degrees around the Y
        }

        if (isKicking)
        {
            // Smoothly return to original rotation
            kickRotationOffset = Vector3.Lerp(
                kickRotationOffset, 
                Vector3.zero, 
                returnSpeed * Time.deltaTime
            );
            
            // Stop kicking when close enough to original
            if (Vector3.Distance(kickRotationOffset, Vector3.zero) < 0.1f)
            {
                kickRotationOffset = Vector3.zero;
                isKicking = false;
            }
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

    void UpdateWeaponPosition()
    {
        Vector3 targetPosition = transform.position + transform.TransformDirection(gunOffset);
        gun.transform.position = Vector3.Lerp(gun.transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }

    public void KickCamera(float verticalKick, float horizontalKick)
    {
        isKicking = true;
        kickRotationOffset += new Vector3(-verticalKick, horizontalKick, 0);
    }
}
