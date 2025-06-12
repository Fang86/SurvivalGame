using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Gun Settings")]
    public GameObject bulletPrefab;
    public float damage = 10f;
    public float range = 1000f;
    public float visualSpeed = 50f;
    public float attackRate = 0.5f;
    public AudioClip fireSound;
    
    [Header("Scope Settings")]
    public float baseFOV = 65f;
    public float scopeMagnification = 2f;

    [Header("Other")]
    public Transform barrelEnd;

    private AudioSource audioSource;
    private Camera playerCamera;
    private CameraController playerCameraController;
    private float timeSinceLastAttack = 0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        playerCamera = Camera.main;
        playerCameraController = playerCamera.GetComponent<CameraController>();
    }
    
    void Update()
    {
        // Update cooldown timer
        if (timeSinceLastAttack > 0)
        {
            timeSinceLastAttack -= Time.deltaTime;
        }
    }
    
    public void HandleInput(bool isAiming, bool isFiring)
    {
        // Scope handling
        if (isAiming)
        {
            playerCameraController.SetAimingSensitivity();
            float scopeFOV = baseFOV / scopeMagnification;
            playerCamera.fieldOfView = scopeFOV;
        }
        else
        {
            playerCameraController.SetDefaultSensitivity();
            playerCamera.fieldOfView = baseFOV;
        }
        
        // Firing
        if (isFiring && timeSinceLastAttack <= 0)
        {
            Fire();
            timeSinceLastAttack = attackRate;
        }
    }
    
    void Fire()
    {
        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;
        Vector3 bulletOrigin = barrelEnd.position;
        Vector3 bulletDirection = rayDirection;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, range))
        {
            bulletDirection = (hit.point - bulletOrigin).normalized;
        }

        GameObject bullet = Instantiate(bulletPrefab, bulletOrigin - (bulletDirection * visualSpeed/50), transform.rotation);
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        bulletRb.linearVelocity = bulletDirection * visualSpeed;

        // Play sound
        if (audioSource && fireSound)
        {
            audioSource.PlayOneShot(fireSound, 0.1f);
        }
    }
}