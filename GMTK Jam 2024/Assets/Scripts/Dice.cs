using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    public Vector3[] Sides;

    public float MaximumThrowMagnitude = 3;

    private Rigidbody _rb;

    private void Awake()
    {
        foreach (Vector3 side in Sides) { side.Normalize(); }
        _rb = GetComponent<Rigidbody>();
    }

    public void Start()
    {
        Roll();
    }

    public void Roll() => Roll(Vector3.forward);
    public void Roll(Vector3 throwDirection)
    {
        transform.rotation = Random.rotation;

        throwDirection.Normalize();

        _rb.AddForce(throwDirection * MaximumThrowMagnitude, ForceMode.Impulse);
    }
}