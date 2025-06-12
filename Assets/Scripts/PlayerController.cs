using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    public Transform head;
    public Camera camera;

    [Header("Configurations")]
    public float walkSpeed = 8f;
    public float runSpeed = 16f;
    public float jumpForce = 300f;

    [Header("Attacking")]
    public float attackRate = .5f;   // time between shots
    private float timeUntilAbleToAttack = 0f;
    public Gun playerWeapon;

    [Header("Ground Detection")]
    public LayerMask groundLayerMask = 1; // What counts as ground
    public float groundCheckDistance = 0.2f;
    public float snapDistance = 1f; // How far to snap down to ground
    public float snapForce = 50f; // How strong the snap force is

    [Header("Ground Snapping")]
    public float maxSnapSpeed = 10f; // Don't snap if falling faster than this
    public float slopeLimit = 45f; // Max slope angle to snap to

    private bool isGrounded;
    private bool wasGroundedLastFrame;
    private bool hasJumpedThisFrame;
    private float timeSinceUngrounded = 0f;

    private Inventory inventory;

    void Start()
    {
        inventory = GetComponent<Inventory>();
    }

    void OnGUI()
    {
        GUI.color = Color.black;
        // Create a simple debug display
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"Is Grounded: {isGrounded}");
        GUILayout.Label($"Y Velocity: {rb.linearVelocity.y:F2}");
        GUILayout.Label($"Speed: {new Vector2(rb.linearVelocity.x, rb.linearVelocity.z).magnitude:F2}");
        GUILayout.Label($"timeSinceLastAttack: {timeUntilAbleToAttack:F2}");
        string items = "";
        inventory.GetItems().ForEach(i => items += $", {i.Name}");
        GUILayout.Label($"Inventory: {inventory.IsOpen()} - {items}");
        GUILayout.EndArea();
    }

    void Update()
    {
        HandleJumping();
    }

    void FixedUpdate()
    {
        wasGroundedLastFrame = isGrounded;

        if (!inventory.IsOpen())
        {
            HandleMovement();
            HandleAttacking();
        }
        
        //HandleGroundSnapping();
        TrackAirTime();
    }

    void HandleAttacking()
    {
        bool isAiming = Mouse.current.rightButton.isPressed;
        bool isFiring = Mouse.current.leftButton.isPressed;
        
        playerWeapon.HandleInput(isAiming, isFiring);
    }

    void HandleJumping()
    {
        // Jump input
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            Jump();
            hasJumpedThisFrame = true;
        }
        else
        {
            hasJumpedThisFrame = false;
        }
    }

    void TrackAirTime()
    {
        // Track time since we lost ground contact
        if (!isGrounded && wasGroundedLastFrame)
        {
            timeSinceUngrounded = 0f;
        }
        else if (!isGrounded)
        {
            timeSinceUngrounded += Time.fixedDeltaTime;
        }
    }

    void HandleMovement()
    {
        CheckGrounded();

        Transform playerTransform = GetComponent<Transform>();

        // Crouching - only apply transform changes if we just started crouching
        bool crouching = Keyboard.current.leftCtrlKey.isPressed;
        if (crouching && playerTransform.localScale.y == 1f)
        {
            playerTransform.localScale = new Vector3(playerTransform.localScale.x, 0.5f, playerTransform.localScale.z);
            transform.position -= Vector3.up * (1f - 0.5f);
        }
        else if (!crouching && playerTransform.localScale.y != 1f)
        {
            playerTransform.localScale = new Vector3(playerTransform.localScale.x, 1f, playerTransform.localScale.z);
            transform.position += Vector3.up * (1f - 0.5f);
        }

        float moveSpeed = Keyboard.current.leftShiftKey.isPressed ? runSpeed : walkSpeed;

        float depthMultiplier = 0f;
        float horizontalMultiplier = 0f;

        if (Keyboard.current.wKey.isPressed)
            depthMultiplier = 1;
        else if (Keyboard.current.sKey.isPressed)
            depthMultiplier = -1;

        if (Keyboard.current.aKey.isPressed)
            horizontalMultiplier = -1;
        else if (Keyboard.current.dKey.isPressed)
            horizontalMultiplier = 1;

        Vector3 inputDirection = new Vector3(horizontalMultiplier, 0f, depthMultiplier).normalized;

        Vector3 moveDirection = camera.transform.TransformDirection(inputDirection);

        moveDirection.y = 0f;
        moveDirection = moveDirection.normalized;

        rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
    }

    void HandleGroundSnapping()
    {
        // Don't snap if we just jumped or moving upward
        if (hasJumpedThisFrame || rb.linearVelocity.y > 0.5f)
        {
            return;
        }
        
        // Only try to snap if we recently lost ground contact (within ~0.2 seconds)
        if (!isGrounded && wasGroundedLastFrame && timeSinceUngrounded < 0.2f)
        {
            // Cast from the center of the collider
            Vector3 rayStart = transform.position;
            
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, snapDistance, groundLayerMask))
            {
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                
                if (slopeAngle <= slopeLimit)
                {
                    // Calculate the target position (slightly above the hit point)
                    Vector3 targetPosition = hit.point + Vector3.up * 0.05f;
                    
                    // Smoothly move to the target position
                    Vector3 positionDelta = targetPosition - transform.position;
                    
                    // Apply a strong downward velocity to snap to ground
                    rb.linearVelocity = new Vector3(
                        rb.linearVelocity.x, 
                        -snapForce * positionDelta.y, 
                        rb.linearVelocity.z
                    );
                    
                    Debug.DrawLine(rayStart, hit.point, Color.cyan, 0.1f);
                }
            }
        }
    }

    void CheckGrounded()
    {
        Collider playerCollider = GetComponent<Collider>();
        Vector3 colliderBottom = playerCollider.bounds.min;
        float colliderRadius = playerCollider.bounds.size.x * 0.5f; // Approximate radius
        
        // Multiple raycast points around the bottom of the collider
        Vector3[] checkOffsets = {
            Vector3.zero,                    // Center
            Vector3.forward * colliderRadius * 0.7f,  // Front
            Vector3.back * colliderRadius * 0.7f,     // Back
            Vector3.left * colliderRadius * 0.7f,     // Left
            Vector3.right * colliderRadius * 0.7f,    // Right
            // Diagonal points for even better coverage
            (Vector3.forward + Vector3.right) * colliderRadius * 0.5f,
            (Vector3.forward + Vector3.left) * colliderRadius * 0.5f,
            (Vector3.back + Vector3.right) * colliderRadius * 0.5f,
            (Vector3.back + Vector3.left) * colliderRadius * 0.5f
        };
        
        isGrounded = false;
        float rayDistance = 0.15f; // Slightly longer for uneven terrain
        
        foreach (Vector3 offset in checkOffsets)
        {
            Vector3 rayStart = colliderBottom + offset + Vector3.up * 0.02f;
            
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayDistance, groundLayerMask))
            {
                // Optional: Check if the surface is walkable (not too steep)
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                if (slopeAngle <= 45f) // Adjust slope limit as needed
                {
                    isGrounded = true;
                    Debug.DrawRay(rayStart, Vector3.down * rayDistance, Color.green, 0.1f);
                    break; // Found ground, no need to check more points
                }
            }
            else
            {
                Debug.DrawRay(rayStart, Vector3.down * rayDistance, Color.red, 0.1f);
            }
        }
        
        //Debug.Log($"Ground check - Grounded: {isGrounded}");
    }

    void Jump()
    {
        // Reset Y velocity then add jump force
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    
    void OnDrawGizmosSelected()
    {
        // Show snap distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, snapDistance);
        
        // Show ground check radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}