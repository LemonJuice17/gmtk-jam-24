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

    public void Attack()
    {
        _characterAnimator.SetTrigger("Attack");
    }
    public void ChangeMoving(bool isMoving)
    {
        _characterAnimator.SetBool("IsWalking", isMoving);
    }
    public void StopAnimations()
    {
        _characterAnimator.enabled = false;
    }
    public void StartAnimations()
    {
        _characterAnimator.enabled = true;
    }
    public void PlayWalkSound()
    {
        if(GameManager.instance.WalkSFX != null) Instantiate(GameManager.instance.WalkSFX);
    }
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
