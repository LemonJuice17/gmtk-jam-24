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
        Agent.SetDestination(position);

        if (manualAnimationControl)
        {
            BroadcastMessage("ChangeMoving", true);
        }

        while (Agent.remainingDistance >= Agent.stoppingDistance)
        {
            await Task.Yield();
        }

        if (manualAnimationControl)
        {
            BroadcastMessage("ChangeMoving", false);
        }
    }
}
