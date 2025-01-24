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

    internal void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
    }

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
}
