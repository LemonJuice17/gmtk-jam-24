using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Dice : MonoBehaviour
{
    public Vector3[] Sides;
    public int[] SideValues;

    public float MaximumThrowMagnitude = 3;

    private Rigidbody _rb;

    public float VelocityMagnitudeStopLimit = 0.05f;
    
    // An event that's called with the rolled value once the roll is finished.
    public UnityEvent<Combatant, int> RolledValue = new();

    Combatant _roller;

    public float DeleteAfterRoll = 1f;

    private void Awake()
    {
        foreach (Vector3 side in Sides) { side.Normalize(); }
        _rb = GetComponent<Rigidbody>();
        transform.position += Vector3.up;
    }

    public void Roll(Combatant combatant) => Roll(combatant, transform.forward);
    public void Roll(Combatant combatant, Vector3 throwDirection)
    {
        InvokeRepeating("StopCheck", 0.5f, 0.1f);

        _roller = combatant;

        if (GameManager.instance.DiceRollup != null) Instantiate(GameManager.instance.DiceRollup);

        transform.rotation = Random.rotation;

        throwDirection.Normalize();

        _rb.AddForce(throwDirection * MaximumThrowMagnitude, ForceMode.Impulse);
    }

    private void StopCheck()
    {
        if (_rb.velocity.magnitude < VelocityMagnitudeStopLimit) StopRoll();
    }

    public void StopRoll()
    {
        _rb.isKinematic = true;

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

        CancelInvoke("StopCheck");

        RolledValue.Invoke(_roller, SideValues[closestIndex]);

        Destroy(gameObject, DeleteAfterRoll);
    }

    public void OnCollisionEnter()
    {
        if (GameManager.instance.DiceRoll != null) Instantiate(GameManager.instance.DiceRoll);
    }
}