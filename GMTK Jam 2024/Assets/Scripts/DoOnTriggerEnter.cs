using UnityEngine;
using UnityEngine.Events;

public class DoOnTriggerEnter : MonoBehaviour
{
    public bool DisableColliderAfterTriggering = true;

    public UnityEvent DoStuff = new();
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player") return;
        DoStuff.Invoke();
        if (DisableColliderAfterTriggering)
        {
            GetComponent<Collider>().enabled = false;
        }
    }
    public void DestroySelf() => Destroy(gameObject);
}