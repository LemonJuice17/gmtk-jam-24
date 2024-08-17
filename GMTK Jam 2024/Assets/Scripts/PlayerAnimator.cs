using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerAnimator : MonoBehaviour
{
    private Animator _playerAnimator;

    public static UnityEvent<bool> MovementUpdate = new();

    private void Awake()
    {
        _playerAnimator = GetComponent<Animator>();
        MovementUpdate.AddListener(ChangeMoving);
    }

    void ChangeMoving(bool isMoving)
    {
        _playerAnimator.SetBool("IsWalking", isMoving);
    }
}
