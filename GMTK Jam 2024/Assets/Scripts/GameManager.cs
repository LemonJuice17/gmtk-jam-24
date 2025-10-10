using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Party Member References")]
    public PartyMember CattankReference;
    public PartyMember GilbertReference;

    [Header("Combat References")]
    public GameObject CombatUIObjectReference; // The entire combat UI
    public GameObject CombatUIPanelObjectReference; // The panel at the bottom during combat
    public TMP_Text CombatUINameText; // The name of the current combatant
    public TMP_Text CombatUIDescriptionText; // The text of the current decription
    /// <summary>
    ///  The UI for displaying the player's available attack options
    /// </summary>
    public GameObject CombatUIPlayerAttackOptionsObjectReference;
    public TMP_Text CombatUIPlayerOptionsTextPrefab; // The prefab for player attacks
    public GameObject CombatTurnOrderObjectReference; // Where the turn order is shown
    /// <summary>
    /// The prefab for each combatant, used to show who's turn it currently is at the top of the screen.
    /// Should contain an Image as a child gameobject that is switched out with the combatant's sprite.
    /// </summary>
    public GameObject CombatTurnOrderIconPrefab;

    [Header("Selection Colours")]
    public Color UnselectedTextColour = Color.white;
    public Color SelectedTextColour = Color.green;

    [Header("Dice Prefabs")]
    public DiceObject D4;
    public DiceObject D6;
    public DiceObject D8;

    [Header("Audio")]
    public SoundObject DiceRollupSFX;
    public SoundObject DiceRollSFX;

    public SoundObject WalkSFX;
    public SoundObject AttackSFX;

    public AudioMixerSnapshot SnapshotNormal;
    public AudioMixerSnapshot SnapshotFight;

    public float MusicTransitionTime = 2;

    [Header("Misc References")]
    public Image BlackScreenReference;

    /// <summary>
    /// A "poof" particle system effect for when things are instantiated.
    /// </summary>
    public ParticleSystem InstantiationPoofEffect;

    /// <summary>
    /// The main camera that follows the player.
    /// </summary>
    public CinemachineVirtualCamera PlayerCamera;

    public static GameManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public void NormalMusic() => SnapshotNormal.TransitionTo(MusicTransitionTime);
    public void FightMusic() => SnapshotFight.TransitionTo(MusicTransitionTime);

    public DiceObject CreateDice(int sides, Vector3 position, Transform parent = null)
    {
        switch (sides)
        {
            case 4:
                if (D4 == null) throw new Exception($"An attempt to make a D4 has been made when the prefab for the D4 in the GameManager has not been set.");
                return Instantiate(D4, position, Quaternion.identity, parent);
            case 6:
                if (D6 == null) throw new Exception($"An attempt to make a D6 has been made when the prefab for the D6 in the GameManager has not been set.");
                return Instantiate(D6, position, Quaternion.identity, parent);
            case 8:
                if (D8 == null) throw new Exception($"An attempt to make a D8 has been made when the prefab for the D8 in the GameManager has not been set.");
                return Instantiate(D8, position, Quaternion.identity, parent);
            default:
                throw new Exception($"A dice with {sides} sides does not currently exist within the GameManager.");
        }
    }

    public ParticleSystem CreatePoofEffect(Vector3 position, Transform parent = null) => Instantiate(InstantiationPoofEffect, position, Quaternion.identity, parent);
}
