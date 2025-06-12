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

    [Header("Weapon Settings")]
    public float sphereRadius = 1.5f;
    public Vector3 sphereOffset = new Vector3(0, -0.2f, 0); // Slightly below head
    public float baseAzimuthAngle = 45f; // Base angle around Y-axis (in degrees)
    public float basePolarAngle = 15f;   // Base angle from horizontal (in degrees)
    private Vector3 sphereCenter;

    [Header("Gun Position Settings")]
    public Vector3 gunOffset = new Vector3(0.5f, -0.3f, 0.8f); // Right, Down, Forward
    public float smoothSpeed = 10f;

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
            //UpdateWeaponPosition(pitch);
            UpdateWeaponPosition();
            gun.transform.rotation = Quaternion.Euler(0f, yaw + 270, -pitch);  // pitch around z because the gun is rotated 270 degrees around the Y
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

    void UpdateWeaponPosition_Old(float cameraPitch)
    {
        // Calculate sphere center relative to player
        sphereCenter = player.transform.position + player.transform.TransformDirection(sphereOffset);
        
        // Get camera pitch (X rotation, clamped to reasonable range)
        //float cameraPitch = Mathf.Clamp(transform.eulerAngles.x, -60f, 60f);
        if (cameraPitch > 180f) cameraPitch -= 360f; // Handle wrap-around
        
        // Calculate spherical coordinates
        float azimuth = baseAzimuthAngle * Mathf.Deg2Rad;
        float polar = (basePolarAngle + cameraPitch) * Mathf.Deg2Rad;
        
        // Convert to Cartesian coordinates on sphere
        Vector3 localPosition = new Vector3(
            sphereRadius * Mathf.Sin(polar) * Mathf.Cos(azimuth),
            sphereRadius * Mathf.Cos(polar),
            sphereRadius * Mathf.Sin(polar) * Mathf.Sin(azimuth)
        );
        
        // Transform to world space (rotate with player body)
        Vector3 worldPosition = sphereCenter + player.transform.TransformDirection(localPosition);
        
        // Position the weapon
        gun.transform.position = worldPosition;
        
        // Calculate weapon rotation to point toward sphere center
        Vector3 directionToCenter = (sphereCenter - gun.transform.position).normalized;
        gun.transform.rotation = Quaternion.LookRotation(directionToCenter, player.transform.up);
    }
}
