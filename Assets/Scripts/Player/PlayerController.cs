using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IInteractor
{
    //temp
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float moveSpeed;
    
    [SerializeField] private InteractionController interactionController;
    [SerializeField] private WeaponHandler weaponHandler;
    
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        
    }

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

        if (Input.GetMouseButtonDown(0))
        {
            _animator.CrossFade("Skill_A", 0.2f);
        }
    }

    public void AttackStart()
    {
        weaponHandler.AttackStart();
    }

    public void AttackEnd()
    {
        weaponHandler.AttackEnd();
    }

    public void SetCurrentWeapon(WeaponType weaponType)
    {
        weaponHandler.SetCurrentWeapon(weaponType);
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
