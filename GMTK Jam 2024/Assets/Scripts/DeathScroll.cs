using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeathScroll : MonoBehaviour
{
    [Multiline]
    public List<string> Dialogue = new();

    public float TimeToSlideOnScreen = 0.5f;
    private TMP_Text text;

    private InputAction interact;

    public bool DialogueFinished = false;

    public async void SetDialogue()
    {
        RectTransform rectPos = GetComponent<RectTransform>();
        rectPos.position = new Vector3(0, -1080, 0);
        TweenRect slideOnScreen = new TweenRect(TimeToSlideOnScreen, rectPos, Vector3.zero, Easing.outSine);
        await slideOnScreen.TweenCompletion;
        
        if(text == null) text = GetComponentInChildren<TMP_Text>();
        Player.instance.Input.SwitchCurrentActionMap("Dialogue");

        interact = InputSystem.actions.FindAction("Continue");
        interact.performed += _ => ExitDialogue();

        int selected = Random.Range(0, Dialogue.Count);
        text.text = Dialogue[selected];
        
    }
    
    public async void ExitDialogue()
    {
        if(!DialogueFinished) return;
        
        DialogueFinished = false;
        
        TweenRect slideOffScreen = new TweenRect(TimeToSlideOnScreen, GetComponent<RectTransform>(), new Vector3(0, -1080, 0), Easing.outSine);
        await slideOffScreen.TweenCompletion;

        Player.instance.Input.SwitchCurrentActionMap("Overworld");
        text.text = "";
        interact = null;

        gameObject.SetActive(false);
    }

    public void SetDialogueFinished() => DialogueFinished = true;
}
