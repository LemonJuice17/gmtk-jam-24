using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    public List<Combatant> Combatants = new();

    public void StartCombat(Vector3 position)
    {

    }
}

public class Combatant
{
    public int MaxHP = 5;
    public int HP = 5;

    public int Strength = 1;
    public int Magic = 1;
    public int Charm = 1;
}