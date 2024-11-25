using UnityEngine;
using UnityEngine.AI;

public class PartyMember : MonoBehaviour
{
    public Combatant Stats;

    public Vector3 PlayerFollowPosition = new (1, 0, -1.5f);
    public Vector3 MaxFollowDeviation = new (0.5f, 0, 1f);

    /// <summary>
    /// What distance away from the player does the member have to be before recalculating their position.
    /// </summary>
    public float FollowDistance = 2;

    private Vector3 _targetPosition;

    /// <summary>
    /// How many seconds between each repetition of the follow loop.
    /// </summary>
    public float FollowLoopRepetitionTime = 0.2f;

    private NavMeshAgent _agent;

    private bool _stayStill = false;

    /// <summary>
    /// Stops the walking animation once the agent reaches this distance from their target destination.
    /// </summary>
    private static readonly float _stopWalkingAnimationCutoffDistance = 0.25f;

    private bool _isWalking;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        Stats.OverworldObject = transform;
        Stats.IsEnemy = false;
    }

    private void Start()
    {
        StartFollowLoop();
    }

    private void Update()
    {
        if (_stayStill) return;

        // Ensure ChangeMoving is only broadcast once when changing from one state to another.
        bool wasWalking = _isWalking;
        _isWalking = _agent.remainingDistance > _stopWalkingAnimationCutoffDistance;

        if (wasWalking != _isWalking)
        {
            BroadcastMessage("ChangeMoving", _isWalking);
            //Debug.Log($"Moving is {_isWalking} because {Stats.Name} is {_agent.remainingDistance} from it's target location of {_agent.destination}.");
        }
    }

    public void StartFollowLoop()
    {
        _agent.isStopped = false;
        _stayStill = false;
        _targetPosition = GetNewTargetPosition();
        InvokeRepeating(nameof(FollowLoop), FollowLoopRepetitionTime, FollowLoopRepetitionTime);
    }

    public void StopFollowLoop()
    {
        // For some reason it suddenly started teleporting to the destination when stopping.
        // No fucking idea why but this fixes it so :P
        _agent.SetDestination(transform.position);
        CancelInvoke(nameof(FollowLoop));
        _agent.isStopped = true;
        _stayStill = true;
        BroadcastMessage("ChangeMoving", false);
    }

    private void FollowLoop()
    {
        if(GetDistanceFromTargetPosition() > FollowDistance)
        {
            _targetPosition = GetNewTargetPosition();
            _agent.SetDestination(_targetPosition);
        }
    }

    private Vector3 GetNewTargetPosition()
    {
        Vector3 newTargetPosition =
            Player.instance.transform.position +
            PlayerFollowPosition +
            new Vector3(Random.Range(-MaxFollowDeviation.x, MaxFollowDeviation.x), 0, Random.Range(-MaxFollowDeviation.z, MaxFollowDeviation.z));

        if ((newTargetPosition - transform.position).magnitude > 0.5f)
        {
            return newTargetPosition;
        }

        else
        {
            return transform.position;
        }
    }

    public float GetDistanceFromPlayer() => Vector3.Distance(transform.position, Player.instance.transform.position);
    public float GetDistanceFromTargetPosition() => Vector3.Distance(_targetPosition, Player.instance.transform.position);
}