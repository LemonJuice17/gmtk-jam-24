using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue : MonoBehaviour, IInteractable
{
    public DialogueBox BoxPrefab;

    public List<DialogueText> DialogueList;
    private int _currentDialogue = 0;

    public bool BoolUseStaticTime;

    public float CharsPerSecond = 20f;
    public float TimeToShowFullText = 10f;

    private DialogueBox _curentDialogueBox;

    public void OnInteract()
    {
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

        NextDialogue();
    }

    public void NextDialogue()
    {
        if (DialogueList.Count == 0 || DialogueList.Count < _currentDialogue + 1)
        {
            EndDialogue();
            return;
        }

        if(_curentDialogueBox == null) _curentDialogueBox = Instantiate(BoxPrefab, UICanvas.Transform);
        LoadDialogue(DialogueList[_currentDialogue]);

        _currentDialogue++;
    }

    public void EndDialogue()
    {
        Player.instance.Input.SwitchCurrentActionMap("Overworld");
        _currentDialogue = 0;
        Destroy(_curentDialogueBox.gameObject, 0);
    }

    public void LoadDialogue(DialogueText dialogue)
    {
        if (dialogue.UseCustomPositioning)
        {
            _curentDialogueBox.transform.position = dialogue.CustomPosition;
            _curentDialogueBox.transform.localScale = dialogue.CustomScale;
        }

        _curentDialogueBox.CharacterSprite.sprite = dialogue.CharacterSprite;
        _curentDialogueBox.CharacterName.text = dialogue.CharacterName;
        _curentDialogueBox.Dialogue.text = dialogue.Text;
    }
}

[System.Serializable]
public class DialogueText
{
    public string Text;

    public string CharacterName;
    public Sprite CharacterSprite;

    public bool UseCustomPositioning;

    public Vector3 CustomPosition;
    public Vector3 CustomScale;
}