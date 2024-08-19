using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject CombatUIObjectReference; // The entire combat UI
    public GameObject CombatUIPanelObjectReference; // The panel at the bottom during combat
    public TMP_Text CombatUINameText; // The name of the current combatant
    public TMP_Text CombatUIDescriptionText; // The text of the current decription
    public GameObject CombatUIPlayerOptionsObjectReference; // The player-specific UI
    public TMP_Text CombatUIPlayerOptionsTextPrefab; // The prefab for player attacks
    public Transform TurnOrderObjectReference; // Where the turn order is shown
    /// <summary>
    /// The prefab for each combatant, used to show who's turn it currently is at the top of the screen.
    /// Should contain an Image as a child gameobject that is switched out with the combatant's sprite.
    /// </summary>
    public GameObject TurnOrderIconPrefab;

    public Color UnselectedTextColour = Color.white;
    public Color SelectedTextColour = Color.green;

    public GameObject D6;
    public GameObject D8;

    public SoundObject DiceRollup;
    public SoundObject DiceRoll;

    public static GameManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }
}
