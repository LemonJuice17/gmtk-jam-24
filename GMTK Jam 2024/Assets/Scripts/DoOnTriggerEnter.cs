using UnityEngine;
using UnityEngine.Events;

public class DoOnTriggerEnter : MonoBehaviour
{
    public UnityEvent DoStuff = new();
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player") return;
        DoStuff.Invoke();
    }
    public void DestroySelf() => Destroy(gameObject);
}