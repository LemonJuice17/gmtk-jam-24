using UnityEngine;
using UnityEngine.Events;

public class DoOnStart : MonoBehaviour
{
    public UnityEvent DoStuff = new();
    public void Start()
    {
        DoStuff.Invoke();
    }
}