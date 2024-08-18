using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CombatEncounter : MonoBehaviour
{
    public Dictionary<Combatant, int> Combatants = new();
    [HideInInspector] public List<Combatant> CombatantOrder = new();
    [HideInInspector] public List<Combatant> AllyCombatants = new();
    public List<Combatant> EnemyCombatants;

    public Vector3 AllyLineOffset = new Vector3(0, 0, -2);
    public Vector3 EnemyLineOffset = new Vector3(0, 0, 2);

    public float CombatantSpacing = 1;

    int _rollsFinished = 0;

    private CinemachineVirtualCamera _camera;

    private void Awake()
    {
        _camera = GetComponentInChildren<CinemachineVirtualCamera>();
    }

    public void StartCombat(Dialogue dialogue)
    {
        StartCombat();
    }

    public void StartCombat()
    {
        Player.instance.Input.SwitchCurrentActionMap("Combat");
        _camera.Priority = 20;

        AllyCombatants.Add(Player.instance.Stats);

        GameObject.FindGameObjectsWithTag("Party Member").ToList().ForEach(obj => AllyCombatants.Add(obj.GetComponent<PartyMember>().Stats));

        AllyCombatants.ForEach((ally) => Combatants.Add(ally, 0));
        EnemyCombatants.ForEach((enemy) => Combatants.Add(enemy, 0));

        Invoke("PositionCombatants", 1);
        Invoke("RollForInitiative", 2);
    }

    public void StopCombat()
    {
        Player.instance.Input.SwitchCurrentActionMap("Overworld");
        _camera.Priority = 1;

        for (int i = 0; i < AllyCombatants.Count; i++)
        {
            if (AllyCombatants[i].OverworldObject.TryGetComponent(out PartyMember pm)) pm.StartFollowLoop();
        }
    }

    private void PositionCombatants()
    {
        Vector3 direction = EnemyLineOffset - AllyLineOffset;
        direction.Normalize();

        for(int i = 0; i < AllyCombatants.Count; i++)
        {
            AllyCombatants[i].OverworldObject.transform.position = transform.position + AllyLineOffset + new Vector3((-AllyCombatants.Count + 1) * (CombatantSpacing * 0.5f) + (i * CombatantSpacing), 0, 0);
            AllyCombatants[i].OverworldObject.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            if (AllyCombatants[i].OverworldObject.TryGetComponent(out PartyMember pm)) pm.StopFollowLoop();
        }

        for (int i = 0; i < EnemyCombatants.Count; i++)
        {
            // Instantiate enemy (if needed)
            if (EnemyCombatants[i].CombatantPrefab != null)
            {
                EnemyCombatants[i].OverworldObject = Instantiate(EnemyCombatants[i].CombatantPrefab).transform;
            }

            // Position enemy
            if (EnemyCombatants[i].OverworldObject != null)
            {
                EnemyCombatants[i].OverworldObject.transform.position = transform.position + EnemyLineOffset + new Vector3((-EnemyCombatants.Count + 1) * (CombatantSpacing * 0.5f) + (i * CombatantSpacing), 0, 0);
                EnemyCombatants[i].OverworldObject.transform.rotation = Quaternion.LookRotation(-direction, Vector3.up);
            }
        }
    }

    public void RollForInitiative()
    {
        Vector3 direction = EnemyLineOffset - AllyLineOffset;
        direction.Normalize();

        foreach(var combatant in AllyCombatants)
        {
            Dice die = Instantiate(GameManager.instance.D6).GetComponent<Dice>();
            die.transform.position = combatant.OverworldObject.transform.position + direction;
            die.Roll(combatant);
            die.RolledValue.AddListener(AddRollResult);
        }

        foreach (var combatant in EnemyCombatants)
        {
            Dice die = Instantiate(GameManager.instance.D6).GetComponent<Dice>();
            die.transform.position = combatant.OverworldObject.transform.position - direction;
            die.Roll(combatant);
            die.RolledValue.AddListener(AddRollResult);
        }
    }

    private void AddRollResult(Combatant combatant, int result)
    {
        Combatants[combatant] = result;
        _rollsFinished++;
        if (_rollsFinished == Combatants.Count) InitiativeDecided();
    }

    private void InitiativeDecided()
    {
        Debug.Log(Combatants);
    }

    public void GenerateIcons(List<Combatant> combatants)
    {
        foreach (Combatant combatant in combatants)
        {
            GameObject newIcon = Instantiate(GameManager.instance.TurnOrderIconPrefab, transform);
            newIcon.transform.GetChild(0).GetComponent<Image>().sprite = combatant.TurnOrderIcon;
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + AllyLineOffset, 0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + EnemyLineOffset, 0.5f);
    }
}

[System.Serializable]
public class Combatant
{
    public int MaxHP = 5;
    public int HP = 5;

    public int Strength = 1;
    public int Magic = 1;
    public int Charm = 1;

    public Sprite TurnOrderIcon;

    public Transform OverworldObject;
    public GameObject CombatantPrefab;
}