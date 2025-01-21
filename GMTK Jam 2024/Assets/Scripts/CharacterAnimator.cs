using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    private Animator _characterAnimator;

    private void Awake()
    {
        _characterAnimator = GetComponent<Animator>();
    }

    void Attack()
    {
        _characterAnimator.SetTrigger("Attack");
    }

    void ChangeMoving(bool isMoving)
    {
        _characterAnimator.SetBool("IsWalking", isMoving);
    }

    public void PlayWalkSound()
    {
        if(GameManager.instance.WalkSFX != null) Instantiate(GameManager.instance.WalkSFX);
    }

    public void PlayAttackSound()
    {
        Instantiate(GameManager.instance.AttackSFX);
    }
}
