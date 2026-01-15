using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class Dialogue : MonoBehaviour, IInteractable
{
    public List<DialogueText> DialogueList = new();
    [SerializeField] private int _currentDialogueIndex = 0;

    public DialogueText CurrentDialogue { get => DialogueList[_currentDialogueIndex]; }
    private DialogueBox _curentDialogueBox;

    private bool WaitForContinue = false;

    public Task DialogueFinished => _dialogueFinished.Task;
    private TaskCompletionSource<bool> _dialogueFinished = new();

    public UnityEvent<Dialogue> AfterDialogue = new();

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
                // When switching action map, the interaction button is still held so the action gets reapeated, once for each action map.
                // This allows the action map to switch, but otherwise suppresses the first input to prevent double inputs.
                return;
            }
        }

        catch { }

        NextDialogue();
    }

    public void StartDialogue() => StartDialogue(true);
    public void StartDialogue(bool setInputActions = true, float timer = 0)
    {
        Player.instance.CurrentInteractable = this;

        try
        {
            if (setInputActions && Player.instance.Input.currentActionMap.name != "Dialogue")
            {
                Player.instance.Input.SwitchCurrentActionMap("Dialogue");
            }
        }

        catch (Exception e)
        {
            Debug.LogError(e);
        }

        NextDialogue(timer);
    }

    public async void NextDialogue(float timer = 0)
    {
        WaitForContinue = false;

        if (DialogueList.Count == 0 || DialogueList.Count < _currentDialogueIndex + 1)
        {
            EndDialogue();
            return;
        }

        if(_curentDialogueBox == null) _curentDialogueBox = Instantiate(GameManager.instance.DialogueBoxPrefab, UICanvas.Transform);
        LoadDialogue(CurrentDialogue);

        if (CurrentDialogue.AutomaticContinueOnly) WaitForContinue = true;

        if (timer > 0) await Task.Delay((int)(timer * 1000));

        CurrentDialogue.Actions.Invoke(this);

        _currentDialogueIndex++;
    }

    public void EndDialogue()
    {
        Player.instance.Input.SwitchCurrentActionMap("Overworld");
        Player.instance.CurrentInteractable = null;
        _currentDialogueIndex = 0;
        Destroy(_curentDialogueBox.gameObject, 0);

        _dialogueFinished.SetResult(true);
        AfterDialogue.Invoke(this);
    }

    public void LoadDialogue(DialogueText dialogue)
    {
        if (dialogue.UseCustomPositioning)
        {
            _curentDialogueBox.transform.position = dialogue.CustomPosition;
            _curentDialogueBox.transform.localScale = dialogue.CustomScale;
        }

        _curentDialogueBox.Dialogue.text = dialogue.Text;

        if (dialogue.Character == null) return;

        if (dialogue.Character.CharacterSprite != null) _curentDialogueBox.CharacterSprite.sprite = dialogue.Character.CharacterSprite;
        if (dialogue.Character.CharacterBackgroundColor != null) _curentDialogueBox.CharacterName.GetComponentInParent<Image>().color = dialogue.Character.CharacterBackgroundColor;
        if (dialogue.Character.CharacterMainColor != null) _curentDialogueBox.CharacterName.color = dialogue.Character.CharacterMainColor;
        _curentDialogueBox.CharacterName.text = dialogue.Character.CharacterName; 
    }
}

[Serializable]
public class DialogueText
{
    [TextArea(3,3)]
    public string Text;

    public Character Character;

    public bool UseCustomPositioning;

    public Vector3 CustomPosition;
    public Vector3 CustomScale;

    public bool AutomaticContinueOnly = false;

    public UnityEvent<Dialogue> Actions = new();

    public DialogueText(string text, Character character = null, bool autoContinue = false)
    {
        Text = text;
        Character = character;
        AutomaticContinueOnly = autoContinue;
    }
}