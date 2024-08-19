using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Combatant Stats;

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
        Stats.OverworldObject = transform;
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
    // ---- Combat Action Map Handling ---- //
    public void OnLeft()
    {

    }
    public void OnRight()
    {

    }
    public void OnSelect()
    {

    }

    private void Move(Vector3 direction)
    {
        if (direction.magnitude == 0)
        {
            BroadcastMessage("ChangeMoving", false);
            return;
        }

        BroadcastMessage("ChangeMoving", true);

        Vector3 adjustedMoveVector = Camera.main.transform.rotation * direction.normalized * MoveSpeed * Time.deltaTime;
        adjustedMoveVector.y = 0;
        transform.position += adjustedMoveVector;
        transform.rotation = Quaternion.LookRotation(adjustedMoveVector, Vector3.up);
    }
}
