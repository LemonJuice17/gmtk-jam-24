using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionDetection : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out IInteractable interactable)) return;

        Player.instance.CurrentInteractable = interactable;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out IInteractable interactable)) return;

        if(Player.instance.CurrentInteractable == interactable) Player.instance.CurrentInteractable = null;
    }
}
