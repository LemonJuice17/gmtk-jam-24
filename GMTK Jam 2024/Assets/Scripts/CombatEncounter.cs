using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CombatEncounter : MonoBehaviour
{
    [HideInInspector] public List<Combatant> Combatants = new();
    [HideInInspector] public List<Combatant> CombatantOrder = new();
    public List<Combatant> EnemyCombatants;

    public void StartCombat()
    {
        Combatants.Add(Player.instance.Stats);

        GameObject.FindGameObjectsWithTag("Party Member").ToList().ForEach(obj => Combatants.Add(obj.GetComponent<PartyMember>().Stats));

        EnemyCombatants.ForEach((enemy) => Combatants.Add(enemy));  
    }

    public void DiceRoll()
    {

    }

    public void GenerateIcons(List<Combatant> combatants)
    {
        foreach (Combatant combatant in combatants)
        {
            GameObject newIcon = Instantiate(GameManager.instance.TurnOrderIconPrefab, transform);
            newIcon.transform.GetChild(0).GetComponent<Image>().sprite = combatant.TurnOrderIcon;
        }
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

    [HideInInspector] public Transform OverworldObject;
    public GameObject CombatantPrefab;
}