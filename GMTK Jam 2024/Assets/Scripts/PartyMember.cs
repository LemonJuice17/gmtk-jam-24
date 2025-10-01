using System.Collections;
using UnityEngine;

public class PartyMember : Walkable
{
    /// <summary>
    /// How far away does the player have to get before this party member starts moving towards the player.
    /// </summary>
    public float DistanceBeforeMoving = 2.5f;

    public void Start()
    {
        CurrentWalkMode = new FollowTarget(this, Player.instance.transform, DistanceBeforeMoving);
    }
}