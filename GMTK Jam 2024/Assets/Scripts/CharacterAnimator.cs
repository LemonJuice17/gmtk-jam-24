using JetBrains.Annotations;
using System.Threading.Tasks;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    private Animator _characterAnimator;

    public Task AttackMade => _attackMade.Task;
    private TaskCompletionSource<bool> _attackMade;

    private void Awake()
    {
        _characterAnimator = GetComponent<Animator>();
    }

    [UsedImplicitly]
    public void Attack()
    {
        _characterAnimator.SetTrigger("Attack");
        _attackMade = new();
    }

    [UsedImplicitly]
    public void ChangeMoving(bool isMoving)
    {
        _characterAnimator.SetBool("IsWalking", isMoving);
    }

    [UsedImplicitly]
    public void StopAllAnimations()
    {
        _characterAnimator.enabled = false;
    }

    public void PlayWalkSound()
    {
        if(GameManager.instance.WalkSFX != null) Instantiate(GameManager.instance.WalkSFX);
    }

    public void PlayAttackSound()
    {
        Instantiate(GameManager.instance.AttackSFX);
        _attackMade.SetResult(true);
    }
}
