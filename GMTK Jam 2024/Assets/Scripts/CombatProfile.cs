using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatProfile : MonoBehaviour
{
    public Character Character;

    public int MaxHP;

    public int Strenth;
    public int Charm;
    public int Magic;

    public Attack[] Attacks;
}

public class Combatant
{
    public readonly CombatProfile Profile;
    public readonly Transform Transform;
    public readonly Team Team;
    public readonly bool IsPlayer;

    /// <summary>
    /// The current HP of this Combatant.
    /// </summary>
    public int HP;

    public Combatant(CombatProfile profile, Transform transform, Team team, bool isPlayer = false)
    {
        Profile = profile;
        HP = Profile.MaxHP;
        Transform = transform;
        Team = team;
        IsPlayer = isPlayer;
    }
}

public enum Team
{
    ally,
    enemy
}