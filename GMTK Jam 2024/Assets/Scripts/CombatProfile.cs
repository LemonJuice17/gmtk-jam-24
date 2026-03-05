using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CombatProfile : MonoBehaviour
{
    public Character Character;

    public int MaxHP;

    public int Power;
    public int Charm;
    public int Magic;

    public List<Attack> Attacks = new List<Attack>();

    public int Level = 0;

    public List<LevelUp> LevelUps = new();

    public void LevelUp()
    {
        Level++;
        if (LevelUps.Count < Level) return;
        LevelUp thisLevel = LevelUps[Level - 1];

        MaxHP += thisLevel.HP;
        foreach(Stats stat in thisLevel.Stats)
        {
            switch (stat)
            {
                case Stats.power:
                    Power++; break;
                case Stats.magic:
                    Magic++; break;
                case Stats.charm:
                    Charm++; break;
            }
        }
    }
}

[Serializable]
public struct LevelUp
{
    public int HP;
    public Stats[] Stats;

    public LevelUp(Stats[] stat, int hp = 5)
    {
        Stats = stat;
        HP = hp;
    }
}

public class Combatant
{
    public readonly CombatProfile Profile;
    public readonly Transform Transform;
    public readonly Team Team;
    public readonly bool IsPlayer;
    public UnityEvent<int> OnHPChanged = new();

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

public enum Stats
{
    none,
    power,
    charm,
    magic
}

public enum Team
{
    ally,
    enemy
}