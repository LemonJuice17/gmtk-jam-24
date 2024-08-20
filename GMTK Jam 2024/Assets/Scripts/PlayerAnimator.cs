using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator _playerAnimator;

    private void Awake()
    {
        _playerAnimator = GetComponent<Animator>();
    }

    void Attack()
    {
        _playerAnimator.SetTrigger("Attack");
    }

    void ChangeMoving(bool isMoving)
    {
        _playerAnimator.SetBool("IsWalking", isMoving);
    }

    public void PlayWalkSound()
    {
        Instantiate(GameManager.instance?.WalkSFX);
    }

    public void PlayAttackSound()
    {
        Instantiate(GameManager.instance.AttackSFX);
    }
}
