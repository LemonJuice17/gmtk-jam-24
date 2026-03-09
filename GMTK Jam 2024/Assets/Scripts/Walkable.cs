using System.Collections;
using System.Threading.Tasks;
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
    internal Coroutine _walkModeCoroutine;

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
            Walker.Agent.enabled = true;
            Walker.Agent.updateRotation = true;
            Walker.Agent.updatePosition = true;
            Walker.Agent.updateUpAxis = true;
            Walker.StopAllCoroutines();
            if (Walker.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
        }
    }

    /// <summary>
    /// Forces this character to immediately stop walking and stand still.
    /// </summary>
    public class StandStill : WalkMode
    {
        public StandStill(Walkable walker) : base(walker)
        {
            Walker.Agent.isStopped = true;
            Walker.Agent.updateRotation = false;
            Walker.Agent.updatePosition = false;
            Walker.Agent.updateUpAxis = false;
            Walker.BroadcastMessage("ChangeMoving", false);
        }
    }

    public class WalkToPoint : WalkMode
    {
        public readonly Vector3 TargetPoint;
        public Task WaitForCompletion => _completionSource.Task;
        private readonly TaskCompletionSource<bool> _completionSource = new();

        private readonly bool _agentEnabledStatus;

        public WalkToPoint(Walkable walker, Vector3 targetPoint) : base(walker)
        {
            _agentEnabledStatus = Walker.Agent.enabled;
            Walker.Agent.enabled = true;

            TargetPoint = targetPoint;

            Walker.Agent.SetDestination(targetPoint);
            Walker.Agent.isStopped = false;
            Walker.BroadcastMessage("ChangeMoving", true);
            Walker._walkModeCoroutine = Walker.StartCoroutine(CheckForArrival());
        }

        private IEnumerator CheckForArrival()
        {
            // For some reason, the y value of the destination is different from the target point.
            // I'm guessing agents automatically find the nearest y point on the terrain?
            // Either way, this offset is used instead of 0 to account for this.
            float destinationYOffset = Walker.Agent.destination.y - TargetPoint.y;

            while (Vector3.Distance(Walker.transform.position, TargetPoint) > destinationYOffset) yield return null;

            Walker.Agent.isStopped = true;
            Walker.BroadcastMessage("ChangeMoving", false);
            _completionSource.SetResult(true);
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

            Walker._walkModeCoroutine = Walker.StartCoroutine(DistanceCheck());
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
}
