using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Dialogue : MonoBehaviour, IInteractable
{
    public DialogueBox BoxPrefab;

    public List<DialogueText> DialogueList;
    [SerializeField] private int _currentDialogueIndex = 0;
    public DialogueText CurrentDialogue { get => DialogueList[_currentDialogueIndex]; }

    public bool BoolUseStaticTime;

    public float CharsPerSecond = 20f;
    public float TimeToShowFullText = 10f;

    private DialogueBox _curentDialogueBox;

    // When switching action map, the interaction button is still held so the action gets reapeated, once for each action map.
    // This bool allows the action map to switch, but otherwise suppresses the first input to prevent double inputs.
    private bool _supressActionMapSwitch = true;

    private bool WaitForContinue = false;

    public UnityEvent<Dialogue> AfterDialogue;

    public void OnInteract()
    {
        if (WaitForContinue) return;

        // This gives an error but works anyway?
        // The try catch stops it from showing up in the console.
        try
        {
            if (Player.instance.Input.currentActionMap.name != "Dialogue")
            {
                Player.instance.Input.SwitchCurrentActionMap("Dialogue");
            }
        }

        catch { }

        if (_supressActionMapSwitch)
        {
            _supressActionMapSwitch = false;
            return;
        }

        NextDialogue();
    }

    public void NextDialogue()
    {
        WaitForContinue = false;

        if (DialogueList.Count == 0 || DialogueList.Count < _currentDialogueIndex + 1)
        {
            EndDialogue();
            return;
        }

        if(_curentDialogueBox == null) _curentDialogueBox = Instantiate(BoxPrefab, UICanvas.Transform);
        LoadDialogue(CurrentDialogue);

        if(CurrentDialogue.AutomaticContinueOnly) WaitForContinue = true;

        CurrentDialogue.Actions.Invoke(this);

        _currentDialogueIndex++;
    }

    public void EndDialogue()
    {
        Player.instance.Input.SwitchCurrentActionMap("Overworld");
        _currentDialogueIndex = 0;
        _supressActionMapSwitch = true;
        AfterDialogue.Invoke(this);
        Destroy(_curentDialogueBox.gameObject, 0);
    }

    public void LoadDialogue(DialogueText dialogue)
    {
        if (dialogue.UseCustomPositioning)
        {
            _curentDialogueBox.transform.position = dialogue.CustomPosition;
            _curentDialogueBox.transform.localScale = dialogue.CustomScale;
        }

        _curentDialogueBox.CharacterSprite.sprite = dialogue.character.CharacterSprite;
        _curentDialogueBox.CharacterName.text = dialogue.character.CharacterName;
        _curentDialogueBox.CharacterName.color = dialogue.character.CharacterColor;
        _curentDialogueBox.Dialogue.text = dialogue.Text;
    }
}

[System.Serializable]
public class DialogueText
{
    public string Text;

    public Character character;

    public bool UseCustomPositioning;

    public Vector3 CustomPosition;
    public Vector3 CustomScale;

    public bool AutomaticContinueOnly = false;

    public UnityEvent<Dialogue> Actions = new();
}