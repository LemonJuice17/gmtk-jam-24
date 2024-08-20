using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyStuff : MonoBehaviour
{
    public Object ObjectToDestroy;

    public void DestroyThing() => Destroy(ObjectToDestroy);
}
