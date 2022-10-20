using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private PlayerLocomotion playerLocomotion;

    [SerializeField] private string groundTag;
    
    private void Awake()
    {
        playerLocomotion = GetComponentInParent<PlayerLocomotion>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == groundTag)
        {
            playerLocomotion.isGrounded = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.tag == groundTag)
        {
            playerLocomotion.isGrounded = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == groundTag)
        {
            playerLocomotion.isGrounded = false;
        }
    }
}
