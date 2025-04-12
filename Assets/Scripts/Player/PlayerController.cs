using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //temp
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float moveSpeed;
    
    [SerializeField] private InteractionController interactionController;
    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            characterController.Move(transform.forward * (Time.deltaTime * moveSpeed));
        }
        
        if (Input.GetKey(KeyCode.S))
        {
            characterController.Move(-transform.forward * (Time.deltaTime * moveSpeed));
        }
        
        if (Input.GetKey(KeyCode.A))
        {
            characterController.Move(-transform.right * (Time.deltaTime * moveSpeed));
        }
        
        if (Input.GetKey(KeyCode.D))
        {
            characterController.Move(transform.right * (Time.deltaTime * moveSpeed));
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            interactionController.Interact();
        }
    }
}
