using JetBrains.Annotations;
using System.Threading.Tasks;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    private Animator _characterAnimator;

    public Task AttackMade => _attackMade.Task;
    private TaskCompletionSource<bool> _attackMade = new();

    private void Awake()
    {
        _characterAnimator = GetComponent<Animator>();
    }

    [UsedImplicitly]
    public void Attack()
    {
        _characterAnimator.SetTrigger("Attack");
    }
    [UsedImplicitly]
    public void ChangeMoving(bool isMoving)
    {
        _characterAnimator.SetBool("IsWalking", isMoving);
    }
    [UsedImplicitly]
    public void StopAnimations()
    {
        _characterAnimator.enabled = false;
    }
    [UsedImplicitly]
    public void StartAnimations()
    {
        _characterAnimator.enabled = true;
    }
    [UsedImplicitly]
    public void PlayWalkSound()
    {
        if(GameManager.instance.WalkSFX != null) Instantiate(GameManager.instance.WalkSFX);
    }
    [UsedImplicitly]
    public void PlayAttackSound()
    {
        try
        {
            Instantiate(GameManager.instance.AttackSFX);
            _attackMade.SetResult(true);
        }
        catch { }
    }
}
