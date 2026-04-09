using System.Threading.Tasks;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    private Animator _characterAnimator;

    public Task AttackMade => _attackMade.Task;
    private TaskCompletionSource<bool> _attackMade;

    public SoundObject AttackSFXOverride;

    private void Awake()
    {
        _characterAnimator = GetComponent<Animator>();
    }

    public void Attack()
    {
        _attackMade = new();
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
        if(AttackSFXOverride != null) Instantiate(AttackSFXOverride);
        else Instantiate(GameManager.instance.AttackSFX);
        _attackMade.TrySetResult(true);
    }
}
