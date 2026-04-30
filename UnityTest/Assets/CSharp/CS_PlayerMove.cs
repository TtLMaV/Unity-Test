using UnityEngine;
using UnityEngine.InputSystem;

public class CS_PlayerMove : MonoBehaviour
{
    // 
    [SerializeField] private float movementSpeed;
    private Rigidbody playerRigidbody;
    private Vector2 movementVelocity;
    private Vector2 lookVelocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
    }

    // Do Movement Inputs
    public void PlayerMovementInput(InputAction.CallbackContext context)
    {
        // Find and deliver movement vector from input axis
        movementVelocity = context.ReadValue<Vector2>();
    }

    // Do Look Inputs
    public void PlayerLookInput(InputAction.CallbackContext context)
    {
        // Find and deliver Look Location, This is filtered into mouseLocation Variable later based on Device
        lookVelocity = context.ReadValue<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {
        // Apply player movement
        Vector3 moveInput3D = new Vector3 (movementVelocity.x * movementSpeed, playerRigidbody.linearVelocity.y, movementVelocity.y * movementSpeed);
        playerRigidbody.linearVelocity = moveInput3D;

        print(lookVelocity);
    }
}
