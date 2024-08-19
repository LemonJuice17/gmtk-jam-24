using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Transform TurnOrderObjectReference;
    /// <summary>
    /// The prefab for each combatant, used to show who's turn it currently is at the top of the screen.
    /// Should contain an Image as a child gameobject that is switched out with the combatant's sprite.
    /// </summary>
    public GameObject TurnOrderIconPrefab;

    public GameObject D6;

    public SoundObject DiceRollup;
    public SoundObject DiceRoll;

    public static GameManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }
}
