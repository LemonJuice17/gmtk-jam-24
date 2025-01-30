using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack")]
public class Attack : ScriptableObject
{
    public float StrengthDamageMultiplier = 0;
    public float CharmDamageMultiplier = 0;
    public float MagicDamageMultiplier = 0;

    [Space]
    public int D6Damage = 0;
    public int D8Damage = 0;

    public void OnAttack(Combatant attacker, Combatant victim)
    {
        float totalDamage =
            attacker.Profile.Strenth * StrengthDamageMultiplier +
            attacker.Profile.Charm * CharmDamageMultiplier +
            attacker.Profile.Magic * MagicDamageMultiplier;

        if (D6Damage > 0)
        {
            
        }

        if (D8Damage > 0)
        {

        }
    }
}
