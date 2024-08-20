using UnityEngine;
using UnityEngine.Events;

public class DoOnTriggerEnter : MonoBehaviour
{
    public UnityEvent DoStuff = new();
    public void OnTriggerEnter() => DoStuff.Invoke();
    public void DestroySelf() => Destroy(gameObject);
}