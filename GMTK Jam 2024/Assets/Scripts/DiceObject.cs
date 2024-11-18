using UnityEngine;
using UnityEngine.Events;

public class DiceObject : MonoBehaviour
{
    public Vector3[] Sides;
    public int[] SideValues;

    /// <summary>
    /// The magnitude that the dice is thrown with.
    /// </summary>
    public float ThrowMagnitude = 3;

    private Rigidbody _rb;

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
        _rb = GetComponent<Rigidbody>();
    }

    public void Roll(Combatant combatant) => Roll(combatant, transform.forward);
    public void Roll(Combatant combatant, Vector3 throwDirection)
    {
        Invoke(nameof(StopRoll), ForceStopTimeout);
        InvokeRepeating(nameof(StopCheck), 0.5f, 0.1f);

        if (GameManager.instance.DiceRollupSFX != null) Instantiate(GameManager.instance.DiceRollupSFX);

        transform.rotation = Random.rotation;

        _rb.AddForce(throwDirection.normalized * ThrowMagnitude, ForceMode.Impulse);
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

        CancelInvoke(nameof(StopCheck));

        Destroy(gameObject, DeleteAfterRoll);
    }

    public void OnCollisionEnter()
    {
        if (GameManager.instance.DiceRollSFX != null) Instantiate(GameManager.instance.DiceRollSFX);
    }
}