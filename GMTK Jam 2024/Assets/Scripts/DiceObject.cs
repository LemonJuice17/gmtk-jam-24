using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class DiceObject : MonoBehaviour
{
    /// <summary>
    /// A list of vectors representing the normal direction of each side.
    /// </summary>
    public Vector3[] Sides;
    /// <summary>
    /// The value of each side from the Sides list by corresponding index.
    /// </summary>
    public int[] SideValues;

    /// <summary>
    /// The magnitude that the dice is thrown with.
    /// </summary>
    public float ThrowMagnitude = 3;

    /// <summary>
    /// The Rigidbody attached to this GameObject.
    /// </summary>
    public new Rigidbody rigidbody { get; private set; }

    /// <summary>
    /// Stops the dice from rolling once it's velocity's magnitude is lower than this value.
    /// </summary>
    public float VelocityMagnitudeStopLimit = 0.05f;
    /// <summary>
    /// If the dice is still rolling after this many seconds, it is forcefully stopped.
    /// </summary>
    public float ForceStopTimeout = 3f;
    
    /// <summary>
    /// An event that's called with the rolled value once the roll is finished.
    /// </summary>
    public UnityEvent<int> RolledValue = new();

    /// <summary>
    /// How many seconds after being the rolled the dice is deleted.
    /// </summary>
    public float DeleteAfterRoll = 1f;

    private void Awake()
    {
        foreach (Vector3 side in Sides) { side.Normalize(); }
        rigidbody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Begins rolling the die.
    /// </summary>
    /// <param name="combatant"> The combatant that threw this die. </param>
    public Task<int> Roll() => Roll(transform.forward);
    /// <summary>
    /// Begins rolling the die.
    /// </summary>
    /// <param name="throwDirection"> The direction the die is thrown in. </param>
    /// <param name="combatant"> The combatant that threw this die. </param>
    public async Task<int> Roll(Vector3 throwDirection)
    {
        if (GameManager.instance.DiceRollupSFX != null) Instantiate(GameManager.instance.DiceRollupSFX);

        transform.rotation = Random.rotation;

        rigidbody.AddForce(throwDirection.normalized * ThrowMagnitude, ForceMode.Impulse);

        StopCheck();
        await Task.Delay((int)(ForceStopTimeout * 1000));
        return StopRoll();
    }

    /// <summary>
    /// Checks the die's curent velocity magnitude and stops it if it's below the VelocityMagnitudeStopLimit.
    /// </summary>
    private async void StopCheck()
    {
        while (rigidbody.velocity.magnitude > VelocityMagnitudeStopLimit)
        {
            await Task.Yield();
        }

        StopRoll();
    }

    /// <summary>
    /// Stops the roll, calculates the side that's facing up, and then deletes the die GameObject.
    /// </summary>
    public int StopRoll()
    {
        rigidbody.isKinematic = true;

        // Dot product: 1 is same direction, 0 is perpendicular, -1 is opposite.
        // Closest to 1 is closest to the same direction.

        int closestIndex = -1;
        float closestDot = -1;

        for (int i = 0; i < Sides.Length; i++)
        {
            float dot = Vector3.Dot(Vector3.up, transform.rotation * Sides[i]);

            if(dot > closestDot)
            {
                closestIndex = i;
                closestDot = dot;
            }
        }

        Destroy(gameObject, DeleteAfterRoll);

        return SideValues[closestIndex];
    }

    // Plays roll SFX when touching surfaces.
    public void OnCollisionEnter()
    {
        if (GameManager.instance.DiceRollSFX != null) Instantiate(GameManager.instance.DiceRollSFX);
    }
}