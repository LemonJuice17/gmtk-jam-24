using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class PartyMember : MonoBehaviour
{
    public Combatant Stats;

    public Vector3 PlayerFollowPosition = new Vector3(1, 0, -1.5f);
    public Vector3 MaxFollowDeviation = new Vector3(0.5f, 0, 1f);

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

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        Stats.OverworldObject = transform;
    }

    private void Start()
    {
        StartFollowLoop();
    }

    private void Update()
    {
        if (_stayStill) return;

        if (_agent.remainingDistance > 0) BroadcastMessage("ChangeMoving", true);
        else BroadcastMessage("ChangeMoving", false);
    }

    public void StartFollowLoop()
    {
        _agent.isStopped = false;
        _stayStill = false;
        _targetPosition = GetNewTargetPosition();
        InvokeRepeating("FollowLoop", FollowLoopRepetitionTime, FollowLoopRepetitionTime);
    }

    public void StopFollowLoop()
    {
        CancelInvoke("FollowLoop");
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
        return
            Player.instance.transform.position +
            PlayerFollowPosition +
            new Vector3(Random.Range(-MaxFollowDeviation.x, MaxFollowDeviation.x), 0, Random.Range(-MaxFollowDeviation.z, MaxFollowDeviation.z));
    }

    public float GetDistanceFromPlayer() => Vector3.Distance(transform.position, Player.instance.transform.position);
    public float GetDistanceFromTargetPosition() => Vector3.Distance(_targetPosition, Player.instance.transform.position);
}