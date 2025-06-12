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

        // Immediate raycast to get distance and target
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, range))
        {
            // Calculate travel time based on distance
            float distance = hit.distance;
            float travelTime = distance / visualSpeed;

            // Delay damage based on travel time
            if (hit.collider.CompareTag("Enemy"))
            {
                StartCoroutine(DelayedDamage(hit.collider, travelTime));
            }

            CreateVisualBullet(bulletOrigin, hit.point);
        }
        else
        {
            CreateVisualBullet(bulletOrigin, bulletOrigin + rayDirection * range);
        }

        // Play sound
        if (audioSource && fireSound)
        {
            audioSource.PlayOneShot(fireSound, 0.1f);
        }
    }
    
    System.Collections.IEnumerator DelayedDamage(Collider target, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Apply damage after travel time
        if (target != null && target.CompareTag("Enemy"))
        {
            HealthBar enemyHealth = target.GetComponent<HealthBar>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
        }
    }
    
    void CreateVisualBullet(Vector3 start, Vector3 end)
    {
        GameObject bullet = Instantiate(bulletPrefab, start, Quaternion.LookRotation(end - start));
        StartCoroutine(AnimateBullet(bullet, start, end));
    }
    
    System.Collections.IEnumerator AnimateBullet(GameObject bullet, Vector3 start, Vector3 end)
    {
        float journey = 0f;
        float distance = Vector3.Distance(start, end);
        
        while (journey <= 1f && bullet != null)
        {
            journey += (visualSpeed / distance) * Time.deltaTime;
            bullet.transform.position = Vector3.Lerp(start, end, journey);
            yield return null;
        }
        
        if (bullet != null) Destroy(bullet);
    }
}