using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollManager : MonoBehaviour
{
    private Collider capsuleCollider;
    private Rigidbody playerRigidbody;
    private Animator animator;

    private Collider[] ragdollColliders;
    private Rigidbody[] limbsRigidbodies;
    
    [SerializeField]
    private GameObject rigSkeleton;

    private void Awake()
    {
        capsuleCollider = GetComponent<Collider>();
        playerRigidbody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        
        GetRagdollBits();
        RagdollModeOff();
    }

    private void GetRagdollBits()
    {
        ragdollColliders = rigSkeleton.GetComponentsInChildren<Collider>();
        limbsRigidbodies = rigSkeleton.GetComponentsInChildren<Rigidbody>();
    }

    public void RagdollModeOn()
    {
        foreach (Collider col in ragdollColliders)
        {
            col.enabled = true;
        }

        foreach (Rigidbody rb in limbsRigidbodies)
        {
            rb.isKinematic = false;
        }
        
        animator.enabled = false;
        capsuleCollider.enabled = false;
        playerRigidbody.isKinematic = true;
    }

    public void RagdollModeOff()
    {
        foreach (Collider col in ragdollColliders)
        {
            col.enabled = false;
        }

        foreach (Rigidbody rb in limbsRigidbodies)
        {
            rb.isKinematic = true;
        }
        
        animator.enabled = true;
        capsuleCollider.enabled = true;
        //playerRigidbody.isKinematic = false;
    }
}
