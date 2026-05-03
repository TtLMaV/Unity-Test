using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CS_PlayerMove : MonoBehaviour
{
    // 
    [Header("Movement")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private GameObject playerVisuals;
    private Rigidbody playerRigidbody;
    private Vector2 movementVelocity;
    private Vector3 lastWalkDirection;

    //
    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private GameObject jumpFromObject;
    [SerializeField] private float jumpFromRadius = 5f;
    private bool tryJumping;
    private bool onGround;

    [Header("Camera")]
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject cameraPivot;
    [SerializeField] private float cameraMaxPitchAngle = 90f;
    [SerializeField] private float maxCameraDistance = 5f;
    [SerializeField] private float cameraXOffest = 0.5f;
    private Vector2 lookVelocity;
    private float cameraXRotation;
    private RaycastHit cameraRayHit;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Do Movement Inputs
    public void InputMove(InputAction.CallbackContext context)
    {
        // Find and deliver movement vector from input axis
        movementVelocity = context.ReadValue<Vector2>();
    }

    // Do Look Inputs
    public void InputLook(InputAction.CallbackContext context)
    {
        // Find and deliver Look Delta
        lookVelocity = context.ReadValue<Vector2>();
    }

    // Do Look Inputs
    public void InputJump(InputAction.CallbackContext context)
    {
        // Find and deliver Jump State
        tryJumping = context.ReadValue<float>() > 0.2f;
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
        Vector3 moveVeloBySpeed = ((transform.forward * movementVelocity.y) + (transform.right * movementVelocity.x)) * movementSpeed;
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
        playerVisuals.transform.forward = lastWalkDirection.magnitude > 0 ? lastWalkDirection : playerVisuals.transform.forward;
    }

    void DoPlayerJump()
    {
        print(tryJumping);
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
}