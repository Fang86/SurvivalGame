using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Target")]
    public Transform player; // Drag your player here
    
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    public float stoppingDistance = 2f; // How close before stopping
    public float chaseRange = 10f; // How far away player can be before enemy stops chasing
    
    [Header("Ground Following")]
    public LayerMask groundLayerMask = 1;
    public float groundCheckDistance = 2f;
    
    private Rigidbody rb;
    private bool isChasing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Find player automatically if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Check if player is within chase range
        if (distanceToPlayer <= chaseRange)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }
        
        if (isChasing && distanceToPlayer > stoppingDistance)
        {
            ChasePlayer();
        }
        else
        {
            // Stop moving when close enough
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void ChasePlayer()
    {
        // Calculate direction to player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        
        // Remove Y component to keep movement on ground plane
        directionToPlayer.y = 0;
        directionToPlayer = directionToPlayer.normalized;
        
        // Move towards player
        Vector3 moveVelocity = directionToPlayer * moveSpeed;
        rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
        
        // Rotate to face player
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Optional: Stick to ground on uneven terrain
        StickToGround();
    }

    void StickToGround()
    {
        // Cast downward to find ground
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayerMask))
        {
            // If we're above ground, pull down slightly
            float groundHeight = hit.point.y;
            float currentHeight = transform.position.y;
            
            if (currentHeight > groundHeight + 0.1f)
            {
                // Apply downward force to stick to ground
                rb.AddForce(Vector3.down * 20f, ForceMode.Force);
            }
        }
        
        // Debug ray
        Debug.DrawRay(rayStart, Vector3.down * groundCheckDistance, Color.yellow);
    }

    // Visual debugging
    void OnDrawGizmosSelected()
    {
        // Show chase range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        // Show stopping distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
        
        // Show line to player
        if (player != null)
        {
            Gizmos.color = isChasing ? Color.green : Color.gray;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}
