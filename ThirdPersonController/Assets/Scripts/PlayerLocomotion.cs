using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class PlayerLocomotion : MonoBehaviour
{
    private InputManager inputManager;
    private Transform cameraObj;
    private Rigidbody playerRigidbody;
    private PlayerManager playerManager;
    private AnimatorManager animatorManager;
    
    private Vector3 moveDirection;

    [Header("MovementFlags")]
    public bool isSprinting;
    public bool isGrounded;
    public bool isJumping;
    
    [Header("Movement Speeds")]
    [FormerlySerializedAs("movementSpeed")] [SerializeField]
    private float runningSpeed;
    [SerializeField]
    private float rotationSpeed;
    [SerializeField]
    private float walkSpeed;
    [SerializeField] 
    private float sprintingSpeed;
    
    [Header("Falling")]
    [SerializeField] 
    private float inAirTimer;
    [SerializeField] 
    private float leapingVelocity;
    [SerializeField]
    private float fallingVelocity;
    [SerializeField] 
    private float rayCastHeightOffset;
    [SerializeField]
    private LayerMask groundLayer;

    [Header("Jump Speeds")] 
    [SerializeField]
    private float jumpHeight;
    [SerializeField]
    private float gravityIntensity;
    
    private void Awake()
    {
        animatorManager = GetComponent<AnimatorManager>();
        playerManager = GetComponent<PlayerManager>();
        inputManager = GetComponent<InputManager>();   
        playerRigidbody = GetComponent<Rigidbody>();
        cameraObj = Camera.main.transform;
    }

    public void HandleAllMovement()
    {
        HandleFallingAndLanding();
        
        if(playerManager.isInteracting)
            return;
        
        HandleMovement();
        HandleRotation();
    }
    private void HandleMovement()
    {
        if (isJumping)
        {
            return;
        }
        
        moveDirection = cameraObj.forward * inputManager.verticalInput;
        moveDirection = moveDirection + cameraObj.right * inputManager.horizontalInput;
        moveDirection.Normalize();
        moveDirection.y = 0;

        if (isSprinting)
        {
            moveDirection = moveDirection * sprintingSpeed;
        }
        else
        {
            if (inputManager.moveAmount >= 0.5f)
            {
                moveDirection = moveDirection * runningSpeed;
            }
            else
            {
                moveDirection = moveDirection * walkSpeed;
            }
        }
        
        Vector3 movementVelocity = moveDirection;
        playerRigidbody.velocity = movementVelocity;
    }

    private void HandleRotation()
    {
        if (isJumping)
        {
            return;
        }

        Vector3 targetDirection = Vector3.zero;

            targetDirection = cameraObj.forward * inputManager.verticalInput;
            targetDirection = targetDirection + cameraObj.right * inputManager.horizontalInput;
            targetDirection.Normalize();
            targetDirection.y = 0;

            if (targetDirection == Vector3.zero)
            {
                targetDirection = transform.forward;
            }

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            Quaternion playerRotation =
                Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            transform.rotation = playerRotation;
        }

    private void HandleFallingAndLanding()
    {
        RaycastHit hit;
        Vector3 rayCastOrigin = transform.position;
        rayCastOrigin.y = rayCastOrigin.y + rayCastHeightOffset;

        if (!isGrounded && !isJumping)
        {
            if (!playerManager.isInteracting)
            {
                animatorManager.PlayTargetAnimation("Falling Idle", true);
            }

            inAirTimer = inAirTimer + Time.deltaTime;
            playerRigidbody.AddForce(transform.forward * leapingVelocity);
            playerRigidbody.AddForce(-Vector3.up * fallingVelocity * inAirTimer);
        }

        if (Physics.SphereCast(rayCastOrigin, 0.2f, -Vector3.up, out hit, groundLayer))
        {
            Debug.Log(hit.collider);
            
            if (!isGrounded && playerManager.isInteracting)
            {
                animatorManager.PlayTargetAnimation("Hard Landing", true);
                playerRigidbody.velocity = new Vector3(0.3f, playerRigidbody.velocity.y, 0.3f);
            }

            inAirTimer = 0;
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    public void HandleJumping()
    {
        if (!isGrounded)
        {
            return;
        }
        
        Debug.Log("jump");

        animatorManager.animator.SetBool("isJumping", true);
        animatorManager.PlayTargetAnimation("Jumping", false);

        float jumpingVelocity = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
        Vector3 playerVelocity = moveDirection;
        playerVelocity.y = jumpingVelocity;
        playerRigidbody.velocity = playerVelocity;
    }
}
