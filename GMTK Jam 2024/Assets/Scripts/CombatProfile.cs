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

    /// <summary>
    /// The current HP of this Combatant. Automatically kills this Combatant if it reaches 0 or below.
    /// </summary>
    public int HP
    {
        get => _hp;
        
        set
        {
            if (value <= 0)
            {
                _hp = 0;
                Die();
            }

            else
            {
                _hp = value;
            }
        }
    }
    private int _hp;

    public Combatant(CombatProfile profile, Transform transform, Team team)
    {
        Profile = profile;
        HP = Profile.MaxHP;
        Transform = transform;
    }

    public void Die()
    {

    }
}

public enum Team
{
    ally,
    enemy
}