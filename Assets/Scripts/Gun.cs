using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Gun Settings")]
    public GameObject bulletPrefab;
    public float damage = 10f;
    public float range = 1000f;
    public float visualSpeed = 50f;
    public float attackRate = 0.5f;
    public int magazineSize = 18;
    public FiringMode firingMode = FiringMode.SEMI;
    public AudioClip fireSound;

    [Header("Scope Settings")]
    public float baseFOV = 65f;
    public float scopeMagnification = 2f;

    [Header("Recoil")]
    public float verticalKickBase = 2f;
    public float verticalKickVariance = 0.1f;
    public float horizontalKickBase = 0f;
    public float horizontalKickVariance = 0.2f;

    public float kickDuration = 0.1f;
    public float returnSpeed = 5f;
    private bool isKicking = false;
    private Vector3 originalRotation;

    [Header("Other")]
    public Transform barrelEnd;

    private AudioSource audioSource;
    private ParticleSystem particleSystem;
    private Camera playerCamera;
    private CameraController playerCameraController;

    private float timeSinceLastAttack = 0f;
    private bool readyToAttack = true;
    private int ammoInMagazine;
    private int kills = 0;

    private PlayerHUDManager hudManager;

    public enum FiringMode
    {
        SEMI,
        BURST,
        AUTOMATIC
    }

    void Start()
    {
        hudManager = GetComponentInParent<PlayerHUDManager>();
        audioSource = GetComponent<AudioSource>();
        particleSystem = GetComponent<ParticleSystem>();
        playerCamera = Camera.main;
        playerCameraController = playerCamera.GetComponent<CameraController>();
        originalRotation = playerCamera.transform.localEulerAngles;

        ammoInMagazine = magazineSize;
    }

    void Update()
    {
        // Update cooldown timer
        if (timeSinceLastAttack > 0)
        {
            timeSinceLastAttack -= Time.deltaTime;
        }
    }

    // Called by PlayerController every frame
    public void HandleInput(bool pressingAim, bool pressingFire, bool pressedReload)
    {
        if (!readyToAttack)
        {
            readyToAttack = (firingMode == FiringMode.AUTOMATIC) || !pressingFire;
        }

        // Scope handling
        if (pressingAim)
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

        // Reloading & firing
        bool allowedToFire = timeSinceLastAttack <= 0 && ammoInMagazine > 0 && readyToAttack;
        if (pressedReload && ammoInMagazine < magazineSize)
        {
            Reload();
        }
        else if (pressingFire && allowedToFire)
        {
            Fire();
            timeSinceLastAttack = attackRate;
        }
        hudManager.UpdateAmmo(ammoInMagazine, magazineSize);
    }

    void Fire()
    {
        ammoInMagazine -= 1;
        readyToAttack = false;
        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;
        Vector3 bulletOrigin = barrelEnd.position;
        Vector3 bulletDirection = rayDirection;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, range))
        {
            bulletDirection = (hit.point - bulletOrigin).normalized;
        }

        GameObject bullet = Instantiate(bulletPrefab, bulletOrigin - (bulletDirection * visualSpeed / 50), transform.rotation);
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        bullet.GetComponent<Bullet>().SetDamage(damage);
        bullet.GetComponent<Bullet>().SetKillCallback(() =>
        {
            IncrementKills();
        });
        bulletRb.linearVelocity = bulletDirection * visualSpeed;

        // Play sound
        if (audioSource && fireSound)
        {
            audioSource.PlayOneShot(fireSound, 0.1f);
        }

        particleSystem.Play();

        // Recoil
        float verticalKick = Random.Range(verticalKickBase - verticalKickVariance, verticalKickBase + verticalKickVariance);
        float horizontalKick = Random.Range(horizontalKickBase - horizontalKickVariance, horizontalKickBase + horizontalKickVariance);
        playerCameraController.KickCamera(verticalKick, horizontalKick);
    }

    void Reload()
    {
        ammoInMagazine = magazineSize;
    }

    public int GetAmmoInMagazine()
    {
        return ammoInMagazine;
    }

    public float GetDamage()
    {
        return damage;
    }

    void IncrementKills()
    {
        kills += 1;
        hudManager.UpdateKills(kills);
    }
}