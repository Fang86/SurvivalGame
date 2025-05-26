using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public float timeUntilDestruction = 30f;
    private float bulletDamage = 10f;

    // Update is called once per frame
    void Update()
    {
        timeUntilDestruction -= Time.deltaTime;
        if (timeUntilDestruction <= 0f)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider collider)
    {
        // Check if it's NOT the player
        if (!collider.gameObject.CompareTag("Player"))
        {
            Debug.Log($"Hit object: {collider.gameObject.name}");
            HandleCollision(collider.gameObject);  // Handle the collision for the other collider object
            Destroy(gameObject);
        }
    }

    void HandleCollision(GameObject hitObject)
    {
        // Your method here
        Debug.Log($"Collision detected with non-player object! {hitObject.name}");

        hitObject.GetComponent<HealthBar>()?.TakeDamage(bulletDamage);

    }

    public void SetDamage(float damage)
    {
        bulletDamage = damage;
    }
}
