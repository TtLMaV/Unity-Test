using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CS_PlayerMove : MonoBehaviour
{
    // Movement Set Variables
    [Header("Movement")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.4f;
    [SerializeField] private GameObject playerVisuals;
    [SerializeField] private float characterTurnSpeed = 5f;
    private Rigidbody playerRigidbody;
    private Vector2 movementVelocity;
    private bool playerSprinting;
    private Vector3 lastWalkDirection;

    // Jumping Set Variables
    [Header("Jumping")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private GameObject groundCheckObject;
    [SerializeField] private float groundCheckRadius = 5f;
    private bool tryJumping;
    private bool onGround;

    // Camera set variables
    [Header("Camera")]
    [SerializeField] private float mouseSensitivity = 1.0f;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject cameraPivot;
    [SerializeField] private float cameraMaxPitchAngle = 90f;
    [SerializeField] private float maxCameraDistance = 5f;
    [SerializeField] private float cameraXOffest = 0.5f;
    [SerializeField] private float cameraAimZoomFOV = 60f;
    [SerializeField] private float cameraFOVLerpSpeed = 5f;
    private Vector2 lookVelocity;
    private float cameraXRotation;
    private RaycastHit cameraRayHit;
    private bool cameraAiming;
    private float cameraNormalFOV;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;

        if(playerCamera != null)
        {
            cameraNormalFOV = playerCamera.fieldOfView;
        }
    }

    // Do Movement Input
    public void InputMove(InputAction.CallbackContext context)
    {
        // Find and deliver movement vector from input axis
        movementVelocity = context.ReadValue<Vector2>();
    }

    // Do Look Input
    public void InputLook(InputAction.CallbackContext context)
    {
        // Find and deliver Look Delta
        lookVelocity = context.ReadValue<Vector2>();
    }

    // Do Jump Input
    public void InputJump(InputAction.CallbackContext context)
    {
        // Find and deliver Jump State
        tryJumping = context.performed;
    }

    // Do Attack Input
    public void InputAttack(InputAction.CallbackContext context)
    {
        // Find and deliver Jump State
            //tryJumping = context.performed;
    }

    // Do Interact Input
    public void InputInteract(InputAction.CallbackContext context)
    {
        // Find and deliver Jump State
            //tryJumping = context.performed;
    }

    // Do Crouch Input
    public void InputCrouch(InputAction.CallbackContext context)
    {
        // Find and deliver Jump State
            //tryJumping = context.performed;
    }

    // Do Sprint Input
    public void InputSprint(InputAction.CallbackContext context)
    {
        // Find and deliver Jump State
        playerSprinting = context.performed;
    }

    // Do Aim Input
    public void InputAim(InputAction.CallbackContext context)
    {
        // Find and deliver Jump State
        cameraAiming = context.performed;
    }

    // Update is called once per frame
    void Update()
    {
        DoPlayerMove();

        DoPlayerJump();

        DoPlayerLook();

        DoCameraArm();
    }

    void DoPlayerMove()
    {
        // Build Movement Vector From Inputs and Look Direction
        Vector3 playersVelo = playerRigidbody.linearVelocity;
        float wantedSpeed = playerSprinting ? movementSpeed * sprintMultiplier  : movementSpeed;
        Vector3 moveVeloBySpeed = ((transform.forward * movementVelocity.y) + (transform.right * movementVelocity.x)) * wantedSpeed;
        Vector3 moveInput3D = new Vector3(moveVeloBySpeed.x, playersVelo.y, moveVeloBySpeed.z);

        // Apply Move Inputs
        playerRigidbody.linearVelocity = moveInput3D;

        // Send Warning if Missing Player Visuals Object
        if (playerVisuals == null)
        {
            Debug.LogWarning("Missing Player Visuals Object");
            return;
        }

        // Apply Player Walk Transform Forward Direction
        lastWalkDirection = movementVelocity.magnitude >= 0.2f ? moveVeloBySpeed : lastWalkDirection;
        Vector3 wantedDirection = lastWalkDirection.magnitude > 0 ? lastWalkDirection : playerVisuals.transform.forward;
        Vector3 lerpPlayerDirection = Vector3.Lerp(playerVisuals.transform.forward, wantedDirection.normalized, Time.deltaTime * characterTurnSpeed);
        playerVisuals.transform.forward = lerpPlayerDirection;
    }

    void DoPlayerJump()
    {
        // Send Warning if Missing Pivot Object
        if (groundCheckObject == null)
        {
            Debug.LogWarning("Missing Ground Check Object");
            return;
        }
        
        //
        onGround = Physics.CheckSphere(groundCheckObject.transform.position, groundCheckRadius, groundMask);

        //
        if (!tryJumping) { return; }

        //
        tryJumping = false;

        if (!onGround) { return; }
        
        Vector3 playersVelo = playerRigidbody.linearVelocity;
        float jumpVeloBySpeed = Mathf.Sqrt(Mathf.Abs(2 * Physics.gravity.y * jumpHeight));
        Vector3 moveInput3D = new Vector3(playersVelo.x, jumpVeloBySpeed, playersVelo.z);

        // Apply Move Inputs
        playerRigidbody.linearVelocity = moveInput3D;
    }

    void DoPlayerLook()
    {
        // Apply Player Rotation Yaw (Left / Right)
        Vector3 playerRotation = playerRigidbody.rotation.eulerAngles;
        float newYRotation = playerRotation.y + (lookVelocity.x * mouseSensitivity);
        playerRigidbody.rotation = Quaternion.Euler(playerRotation.x, newYRotation, playerRotation.z);

        // Send Warning if Missing Pivot Object
        if (cameraPivot == null)
        {
            Debug.LogWarning("Missing Camera Pivot Object");
            return;
        }

        // Apply Player Rotation Pitch (Up / Down)
        cameraXRotation -= (lookVelocity.y * mouseSensitivity);
        cameraXRotation = Mathf.Clamp(cameraXRotation, -cameraMaxPitchAngle, cameraMaxPitchAngle);
        cameraPivot.transform.localRotation = Quaternion.Euler(cameraXRotation, 0, 0);
    }

    void DoCameraArm()
    {
        // Deal With FOV Based On FOV Input
        float wantedCameraFOV = cameraAiming ? cameraAimZoomFOV : cameraNormalFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, wantedCameraFOV, Time.deltaTime * cameraFOVLerpSpeed);

        // Send Warning if Missing Camera Object
        if (playerCamera == null)
        {
            Debug.LogWarning("Missing Player Camera Object");
            return;
        }

        // Set Deffault Position For Camera
        playerCamera.transform.localPosition = new Vector3(cameraXOffest, 0f, -maxCameraDistance);

        // Move Camera Backwards and Right Through Raycast
        Vector3 BackwardsVector = (cameraPivot.transform.right * cameraXOffest) - (cameraPivot.transform.forward * maxCameraDistance);
        bool cameraOccluded = Physics.Raycast(cameraPivot.transform.position, BackwardsVector, out cameraRayHit, maxCameraDistance, groundMask);
        Vector3 newCameraPoint = cameraOccluded ? cameraRayHit.point : playerCamera.transform.position;

        //
        float cameraCheckDistance = 0.5f;
        Vector3 posForRightCheck = newCameraPoint + (cameraPivot.transform.right * -0.1f) + (cameraPivot.transform.forward * 0.2f);
        bool rightHandOccluded = Physics.Raycast(posForRightCheck, cameraPivot.transform.right * cameraCheckDistance, out cameraRayHit, cameraCheckDistance, groundMask);
        float rightHandDistance = rightHandOccluded ? Mathf.Clamp(cameraRayHit.distance + 0.1f, 0, cameraCheckDistance) : cameraCheckDistance;

        // 
        Vector3 posForLeftCheck = newCameraPoint + (cameraPivot.transform.right * 0.1f) + (cameraPivot.transform.forward * 0.2f);
        bool leftHandOccluded = Physics.Raycast(posForLeftCheck, cameraPivot.transform.right * -cameraCheckDistance, out cameraRayHit, cameraCheckDistance, groundMask);
        float leftHandDistance = leftHandOccluded ? Mathf.Clamp(cameraRayHit.distance + 0.1f, 0, cameraCheckDistance) : cameraCheckDistance;

        //
        float SidedMotion = rightHandDistance - leftHandDistance;

        // Apply Final Positions
        playerCamera.transform.position = newCameraPoint + (cameraPivot.transform.right * SidedMotion);
    }

    private void OnDrawGizmos()
    {
        // Draw Ground Object Radius
        if (groundCheckObject != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheckObject.transform.position, groundCheckRadius);
        }
    }

}