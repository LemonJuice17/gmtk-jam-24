using MoreMountains.Tools;
using System.Collections;
using System.Threading.Tasks;
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

    private Coroutine _distanceCheck;

    public void Start()
    {
        _distanceCheck = StartCoroutine(DistanceCheck());
    }

    private IEnumerator DistanceCheck()
    {
        yield return null;

        bool stand = true;
        while (stand)
        {
            if (GetDistanceFromPlayer() >= DistanceBeforeMoving)
            {
                stand = false;
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
        await WalkToPosition(Player.instance.transform.position + RelativePlayerFollowPosition, true);
        _distanceCheck = StartCoroutine(DistanceCheck());
    }

    public float GetDistanceFromPlayer() => Vector3.Distance(transform.position, Player.instance.transform.position);
}