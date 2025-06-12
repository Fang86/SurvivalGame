using System;
using System.Linq.Expressions;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public float timeUntilDestruction = 30f;
    private float bulletDamage = 10f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        CheckCollisions();
    }

    // Update is called once per frame
    void Update()
    {
        timeUntilDestruction -= Time.deltaTime;
        if (timeUntilDestruction <= 0f)
            Destroy(gameObject);
    }

    void CheckCollisions()
    {
        Vector3 nextPosition = transform.position + (rb.linearVelocity * Time.deltaTime);
        float travelDistance = Vector3.Distance(transform.position, nextPosition);

        bool raycastHit = Physics.Raycast(transform.position, rb.linearVelocity.normalized, out RaycastHit hit, travelDistance);
        //Debug.Log(travelDistance, hit.collider);
        Debug.Log(rb.linearVelocity + " | " + transform.position + " | " +  nextPosition + " | " + travelDistance);
        if (raycastHit && !hit.collider.gameObject.CompareTag("Player"))
        {
            // Handle collision before physics step occurs
            // TODO: Consider animating bullet to hit point before handling the collision
            HandleCollision(hit.collider.gameObject);
        }
    }

    void HandleCollision(GameObject hitObject)
    {
        // Your method here
        Debug.Log($"Collision detected with non-player object! {hitObject.name}");

        hitObject.GetComponent<HealthBar>()?.TakeDamage(bulletDamage);

        if (!hitObject.CompareTag("Enemy"))
        {
            Vector3 force = rb.linearVelocity * rb.mass;
            hitObject.GetComponent<Rigidbody>()?.AddForce(force);
        }

        Destroy(gameObject);
    }

    public void SetDamage(float damage)
    {
        bulletDamage = damage;
    }
}
