using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public int MaxHP = 5;
    public int HP = 5;

    public int Strength = 1;
    public int Magic = 1;
    public int Charm = 1;

    public float MoveSpeed = 2;

    private Vector3 _currentMoveDirection;

    public IInteractable CurrentInteractable;

    public static Player instance;

    public PlayerInput Input { get; private set; }

    public void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        Input = GetComponent<PlayerInput>();

        HP = MaxHP;
    }

    public void Start()
    {
        
    }

    public void Update()
    {
        Move(_currentMoveDirection);
    }

    // ---- Overworld Action Map Handling ---- //
    public void OnMove(InputValue value)
    {
        _currentMoveDirection = value.Get<Vector3>();
    }

    public void OnInteract() 
    {
        CurrentInteractable?.OnInteract();
    }
    // ---- Dialogue Action Map Handling ---- //
    public void OnContinue()
    {
        CurrentInteractable?.OnInteract();
    }

    private void Move(Vector3 direction)
    {
        if (direction.magnitude == 0)
        {
            PlayerAnimator.MovementUpdate.Invoke(false);
            return;
        }

        PlayerAnimator.MovementUpdate.Invoke(true);

        Vector3 adjustedMoveVector = Camera.main.transform.rotation * direction * MoveSpeed * Time.deltaTime;
        adjustedMoveVector.y = 0;
        transform.position += adjustedMoveVector;
        transform.rotation = Quaternion.LookRotation(adjustedMoveVector, Vector3.up);
    }
}
