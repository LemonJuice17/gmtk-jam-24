using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Class to be inherited for gameobjects that can walk.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class Walkable : MonoBehaviour
{
    public Vector3 TargetPosition;
    public bool AutoWalkToTarget = true;

    public NavMeshAgent Agent { get; private set; }

    internal void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        _ = AutoWalk();
    }

    private async Task AutoWalk()
    {
        while (AutoWalkToTarget) 
        {
            Agent.SetDestination(TargetPosition);

            BroadcastMessage("ChangeMoving", !Agent.pathStatus.HasFlag(NavMeshPathStatus.PathComplete));

            await Task.Delay(250);
        }
    }

    public async Task WalkToPosition(Vector3 position)
    {
        bool autoWalk = AutoWalkToTarget;
        AutoWalkToTarget = false;

        TargetPosition = position;

        Agent.SetDestination(position);

        BroadcastMessage("ChangeMoving", true);
        Debug.Log("ChangeMoving set to true");

        while (!Agent.pathStatus.HasFlag(NavMeshPathStatus.PathComplete))
        {
            await Task.Yield();
        }

        BroadcastMessage("ChangeMoving", false);
        Debug.Log("ChangeMoving set to false");

        AutoWalkToTarget = autoWalk;
    }
}
