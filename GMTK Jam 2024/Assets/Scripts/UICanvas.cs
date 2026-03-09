using UnityEngine;

public class UICanvas : MonoBehaviour
{
    public static Transform Transform;

    private void Awake()
    {
        Transform = transform;
    }
}
