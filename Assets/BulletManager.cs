using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public float timeUntilDestruction = 80f;

    // Update is called once per frame
    void Update()
    {
        timeUntilDestruction -= Time.deltaTime;
        if (timeUntilDestruction <= 0f)
            Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if it's NOT the player
        if (!collision.gameObject.CompareTag("Player"))
        {
            Debug.Log($"Hit object: {collision.gameObject.name}");
            HandleCollision(collision.gameObject);  // Handle the collision for the entitythe bullet hit
            Destroy(gameObject);
        }
    }

    void HandleCollision(GameObject hitObject)
    {
        // Your method here
        Debug.Log($"Collision detected with non-player object! {hitObject.name}");

        // Example actions:
        // Destroy(gameObject); // Destroy this object
        // hitObject.GetComponent<Health>()?.TakeDamage(10);
        
    }
}
