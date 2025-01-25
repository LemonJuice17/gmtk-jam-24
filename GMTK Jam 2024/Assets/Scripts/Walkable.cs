using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Class to be inherited for gameobjects that can walk.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class Walkable : MonoBehaviour
{
    public NavMeshAgent Agent { get; private set; }
    public WalkMode CurrentWalkMode;

    internal void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// Walk Mode state machine base class.
    /// </summary>
    public abstract class WalkMode
    {
        public Walkable Walker;

        public WalkMode(Walkable walker)
        {
            Walker = walker;

            if (Walker.Agent.enabled)
            {
                Walker.Agent.isStopped = true;
            }
        }
    }

    /// <summary>
    /// Forces this character to immediately stop walking and stand still.
    /// </summary>
    public class StandStill : WalkMode
    {
        public StandStill(Walkable walker) : base(walker)
        {
            Walker.Agent.SetDestination(Walker.transform.position);
            Walker.BroadcastMessage("ChangeMoving", false);
        }
    }

    public class WalkToPoint : WalkMode
    {
        public readonly Vector3 TargetPoint;
        public Task WaitForCompletion => _completionSource.Task;
        private readonly TaskCompletionSource<bool> _completionSource = new();

        private bool _agentEnabledStatus;

        public WalkToPoint(Walkable walker, Vector3 targetPoint) : base(walker)
        {
            _agentEnabledStatus = Walker.Agent.enabled;
            Walker.Agent.enabled = true;

            TargetPoint = targetPoint;

            Walker.Agent.SetDestination(targetPoint);
            Walker.Agent.isStopped = false;
            Walker.BroadcastMessage("ChangeMoving", true);
            Walker.StartCoroutine(CheckForArrival());
        }

        private IEnumerator CheckForArrival()
        {
            while(Vector3.Distance(Walker.transform.position, TargetPoint) > 0)
            {
                Debug.Log($"{Walker.name} is trying to go to {Walker.Agent.destination} and is {Walker.Agent.remainingDistance} away.");
                yield return null;
            }

            Walker.Agent.isStopped = true;
            Walker.BroadcastMessage("ChangeMoving", false);
            _completionSource.SetResult(true);
            Debug.Log($"{Walker.name} has reached their destination of {Walker.Agent.destination} and is {Walker.Agent.remainingDistance} away.");
            Walker.Agent.enabled = _agentEnabledStatus;
        }
    }

    public class FollowTarget : WalkMode 
    {
        public Transform TargetToFollow;
        public float FollowDistance;

        public FollowTarget(Walkable walker, Transform targetToFollow, float followDistance = 1) : base(walker)
        {
            TargetToFollow = targetToFollow;
            FollowDistance = followDistance;

            Walker.StartCoroutine(DistanceCheck());
        }

        private IEnumerator DistanceCheck()
        {
            while (true)
            {
                if (Vector3.Distance(Walker.transform.position, TargetToFollow.transform.position) > FollowDistance)
                {
                    Walker.Agent.SetDestination(TargetToFollow.transform.position);

                    if (Walker.Agent.isStopped == true)
                    {
                        Walker.Agent.isStopped = false;
                        Walker.BroadcastMessage("ChangeMoving", true);
                    }
                }

                else
                {
                    Walker.Agent.isStopped = true;
                    Walker.BroadcastMessage("ChangeMoving", false);
                }

                yield return null;
            }
        }
    }

    public class Wander : WalkMode
    {
        public Wander(Walkable walker) : base(walker)
        {

        }
    }

    public class Patrol : WalkMode
    {
        public Patrol(Walkable walker) : base(walker)
        {

        }
    }

    /*
    public async Task WalkToPosition(Vector3 position, bool manualAnimationControl = false)
    {
        // If the agent component is disabled, enable it and then re-disable it after moving.
        bool reDisableAgent = false;
        if (Agent.enabled == false)
        {
            Agent.enabled = true;
            reDisableAgent = true;
        }

        Agent.SetDestination(position);
        Agent.isStopped = false;

        // Enable walking animation if manualAnimationControl isn't being used.
        if (manualAnimationControl)
        {
            BroadcastMessage("ChangeMoving", true);
        }

        // Wait until the agent has reached its destination
        while (Agent.remainingDistance >= Agent.stoppingDistance)
        {
            await Task.Yield();
        }

        // Disable walking animation if manualAnimationControl isn't being used.
        if (manualAnimationControl)
        {
            BroadcastMessage("ChangeMoving", false);
        }

        if (reDisableAgent)
        {
            Agent.enabled = false;
        }
    }
    */
}
