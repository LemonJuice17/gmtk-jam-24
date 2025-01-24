using System.Collections;
using UnityEngine;

public class PartyMember : Walkable
{
    /// <summary>
    /// The position relative to the player that this party member will try to follow.
    /// </summary>
    public Vector3 RelativePlayerFollowPosition;

    /// <summary>
    /// How far away does the player have to get before this party member starts moving towards the player.
    /// </summary>
    public float DistanceBeforeMoving = 2.5f;

    public bool FollowingPlayer = true;

    public void Start()
    {
        StartCoroutine(DistanceCheck());
    }

    private IEnumerator DistanceCheck()
    {
        yield return null;

        bool notMoving = true;
        while (notMoving)
        {
            if (!FollowingPlayer)
            {
                notMoving = true;
                BroadcastMessage("ChangeMoving", false);
                Agent.isStopped = true;
                break;
            }

            if (GetDistanceFromPlayer() >= DistanceBeforeMoving)
            {
                notMoving = false;
                FollowPlayer();
            }

            else
            {
                BroadcastMessage("ChangeMoving", false);
                yield return null;
            }
        }
    }

    private async void FollowPlayer()
    {
        BroadcastMessage("ChangeMoving", true);
        await WalkToPosition(Player.instance.transform.position + RelativePlayerFollowPosition);
        StartCoroutine(DistanceCheck());
    }

    public float GetDistanceFromPlayer() => Vector3.Distance(transform.position, Player.instance.transform.position);
}