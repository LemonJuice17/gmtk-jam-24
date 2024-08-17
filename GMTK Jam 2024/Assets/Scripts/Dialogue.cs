using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    public DialogueBox BoxPrefab;

    public List<DialogueText> DialogueList;
    private int _currentDialogue = 0;

    public bool BoolUseStaticTime;

    public float CharsPerSecond = 20f;
    public float TimeToShowFullText = 10f;

    private DialogueBox _curentDialogueBox;

    private void Start()
    {
        StartDialogue();
    }

    public void StartDialogue()
    {
        if (DialogueList.Count == 0)
        {
            EndDialogue();
        }

        _curentDialogueBox = LoadDialogue(DialogueList[_currentDialogue]);
    }

    public void Continue()
    {
        _currentDialogue++;

        if(DialogueList.Count < _currentDialogue + 1 ) 
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        Destroy(_curentDialogueBox);
    }

    public DialogueBox LoadDialogue(DialogueText dialogue)
    {
        DialogueBox box = Instantiate(BoxPrefab, UICanvas.Transform);

        if (dialogue.UseCustomPositioning)
        {
            box.transform.position = dialogue.CustomPosition;
            box.transform.localScale = dialogue.CustomScale;
        }

        box.CharacterSprite.sprite = dialogue.CharacterSprite;
        box.CharacterName.text = dialogue.CharacterName;
        box.Dialogue.text = dialogue.Text;

        return box;
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