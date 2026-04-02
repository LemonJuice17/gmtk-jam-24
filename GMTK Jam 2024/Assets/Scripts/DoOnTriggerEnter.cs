using UnityEngine;
using UnityEngine.Events;

public class DoOnTriggerEnter : MonoBehaviour
{
    public bool DisableCollidersAfterTriggering = true;

    public UnityEvent DoStuff = new();
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player") return;
        DoStuff.Invoke();

        if (DisableCollidersAfterTriggering)
        {
            Collider[] colliders = GetComponents<Collider>();

            foreach (Collider collider in colliders) 
            {
                collider.enabled = false;
            }
        }
    }

    //Might need updating for multiple colliders?
    public void ReEnable() { if (TryGetComponent(out Collider collider)) collider.enabled = true; }

    public void DestroySelf() => Destroy(gameObject);
}