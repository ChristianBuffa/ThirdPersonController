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
    private float gravityVelocity;
    [SerializeField] 
    private float rayCastHeightOffset;
    [SerializeField] 
    private float rayCastMaxDistance;
    [SerializeField]
    private LayerMask groundLayer;

    [Header("Jump Speeds")] 
    [SerializeField]
    private float jumpHeight;
    [SerializeField]
    private float gravityIntensity;

    [Header("Player Step Climb")] 
    [SerializeField]
    private GameObject stepRayUpper;
    [SerializeField]
    private GameObject stepRayLower;
    [SerializeField]
    private float stepHeight;
    [SerializeField]
    private float stepSmooth;
    [SerializeField]
    private float stepLowRayMaxDistance;
    [SerializeField]
    private float stepUpRayMaxDistance;
    
    private void Awake()
    {
        animatorManager = GetComponent<AnimatorManager>();
        playerManager = GetComponent<PlayerManager>();
        inputManager = GetComponent<InputManager>();   
        playerRigidbody = GetComponent<Rigidbody>();
        cameraObj = Camera.main.transform;

        stepRayUpper.transform.position = new Vector3(stepRayUpper.transform.position.x, stepHeight, stepRayUpper.transform.position.z);
    }

    public void HandleAllMovement()
    {
        HandleFallingAndLanding();
        
        if(playerManager.isInteracting)
            return;
        
        HandleMovement();
        HandleRotation();
        HandleStepClimb();
    }
    private void HandleMovement()
    {
        if (isJumping || playerManager.isInteracting)
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
        else if (isGrounded && !isJumping)
        {
            playerRigidbody.AddForce(-Vector3.up * gravityVelocity);
        }

        if (Physics.Raycast(rayCastOrigin,-Vector3.up, out hit, rayCastMaxDistance, groundLayer))
        {
            Debug.Log(hit.collider);
            
            if (!isGrounded && playerManager.isInteracting)
            {
                animatorManager.PlayTargetAnimation("Hard Landing", true);
                playerRigidbody.velocity = new Vector3(0.3f, playerRigidbody.velocity.y, 0.3f);
            }

            inAirTimer = 0;
            //isGrounded = true;
            isJumping = false;
        }
        else
        {
            //isGrounded = false;
        }
    }

    public void HandleJumping()
    {
        if (!isGrounded)
        {
            return;
        }
        
        animatorManager.animator.SetBool("isJumping", true);
        animatorManager.PlayTargetAnimation("Jumping", true);

        float jumpingVelocity = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
        Vector3 playerVelocity = moveDirection;
        playerVelocity.y = jumpingVelocity;
        playerRigidbody.velocity = playerVelocity;
    }

    private void HandleStepClimb()
    {
        RaycastHit hitLower;
        if (Physics.Raycast(stepRayLower.transform.position, transform.TransformDirection(Vector3.forward), out hitLower, stepLowRayMaxDistance))
        {
            RaycastHit hitUpper;
            if (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(Vector3.forward), out hitUpper, stepUpRayMaxDistance))
            {
                playerRigidbody.position -= new Vector3(0f, -stepSmooth, 0f);
            }
        }
        
        RaycastHit hitLower45;
        if (Physics.Raycast(stepRayLower.transform.position, transform.TransformDirection(1.5f, 0, 1), out hitLower45, stepLowRayMaxDistance))
        {
            RaycastHit hitUpper45;
            if (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(1.5f, 0, 1), out hitUpper45, stepUpRayMaxDistance))
            {
                playerRigidbody.position -= new Vector3(0f, -stepSmooth, 0f);
            }
        }
        
        RaycastHit hitLowerMinus45;
        if (Physics.Raycast(stepRayLower.transform.position, transform.TransformDirection(-1.5f, 0, 1), out hitLowerMinus45, stepLowRayMaxDistance))
        {
            RaycastHit hitUpperMinus45;
            if (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(-1.5f, 0, 1), out hitUpperMinus45, stepUpRayMaxDistance))
            {
                Debug.Log("gianni");
                playerRigidbody.position -= new Vector3(0f, -stepSmooth, 0f);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 raycastOffset = new Vector3(transform.position.x, transform.position.y + rayCastHeightOffset, transform.position.z);
        Vector3 raycastMaxDist = new Vector3(transform.position.x, transform.position.y + rayCastHeightOffset - rayCastMaxDistance, transform.position.z);
        
        Gizmos.DrawLine(raycastOffset, raycastMaxDist);
    }
}
