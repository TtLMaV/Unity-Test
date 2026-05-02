using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CS_PlayerMove : MonoBehaviour
{
    // 
    [Header("Movement")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private LayerMask groundMask;
    private Rigidbody playerRigidbody;
    private Vector2 movementVelocity;

    [Header("Camera")]
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject cameraPivotObject;
    [SerializeField] private float cameraMaxPitchAngle = 90f;
    [SerializeField] private float minCameraDistance = 1f;
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
        // Find and deliver Look Location, This is filtered into mouseLocation Variable later based on Device
        lookVelocity = context.ReadValue<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {
        DoPlayerMove();

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
    }

    void DoPlayerLook()
    {
        // Apply Player Rotation Yaw (Left / Right)
        Vector3 playerRotation = playerRigidbody.rotation.eulerAngles;
        float newYRotation = playerRotation.y + (lookVelocity.x * mouseSensitivity);
        playerRigidbody.rotation = Quaternion.Euler(playerRotation.x, newYRotation, playerRotation.z);

        // Send Warning if Missing Pivot Object
        if (cameraPivotObject == null)
        {
            Debug.LogWarning("Missing Camera Pivot Object");
            return;
        }

        // Apply Player Rotation Pitch (Up / Down)
        cameraXRotation -= (lookVelocity.y * mouseSensitivity);
        cameraXRotation = Mathf.Clamp(cameraXRotation, -cameraMaxPitchAngle, cameraMaxPitchAngle);
        cameraPivotObject.transform.localRotation = Quaternion.Euler(cameraXRotation, 0, 0);
    }

    void DoCameraArm()
    {
        // 
        playerCamera.transform.localPosition = new Vector3(cameraXOffest, 0f, -maxCameraDistance);

        // Send Warning if Missing Camera Object
        if (playerCamera == null)
        {
            Debug.LogWarning("Missing Camera Object");
            return;
        }

        // Move Camera Backwards Along Pivot Through Raycast
        bool cameraOccluded = Physics.Raycast(cameraPivotObject.transform.position, -cameraPivotObject.transform.forward, out cameraRayHit, maxCameraDistance, groundMask);
        float newCameraZDepth = cameraOccluded ? Mathf.Clamp(-cameraRayHit.distance, -maxCameraDistance, -minCameraDistance) : -5;
        //Debug.DrawRay(cameraPivotObject.transform.position, cameraPivotObject.transform.forward * newCameraZDepth, Color.red, 0.1f);

        //
        Vector3 cameraPositionWithDepth = cameraPivotObject.transform.position + (cameraPivotObject.transform.forward * newCameraZDepth) + (cameraPivotObject.transform.right * Mathf.Sign(cameraXOffest) * -0.1f);
        bool handedSideOccluded = Physics.Raycast(cameraPositionWithDepth, cameraPivotObject.transform.right * cameraXOffest, out cameraRayHit, cameraXOffest, groundMask);
        float newCameraZOffset = handedSideOccluded ? Mathf.Clamp(cameraRayHit.distance, -cameraXOffest, cameraXOffest) : cameraXOffest;
        //Debug.DrawRay(cameraPositionWithDepth, cameraPivotObject.transform.right * newCameraZOffset, Color.red, 0.1f);

        // Apply Final Positions
        playerCamera.transform.localPosition = new Vector3(newCameraZOffset, 0f, newCameraZDepth);
    }
}