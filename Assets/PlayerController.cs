using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    public Transform head;
    public Camera camera;

    [Header("Configurations")]
    public float walkSpeed;
    public float runSpeed;

    void Start()
    {

    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        float initialYVelocity = rb.linearVelocity.y;
        float moveSpeed = Keyboard.current.leftShiftKey.isPressed ? walkSpeed : runSpeed;

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
        moveDirection.y = initialYVelocity;

        rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
    }
}
