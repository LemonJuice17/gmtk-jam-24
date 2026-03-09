using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Roll Over")]
public class RollOver : Attack
{
    public RollOver() => AttackDescription = "Roll Over";

    public async Task OnAttack(Combatant attacker, Combatant[] opponents)
    {
        await FaceOpponent(attacker, AverageCombatantPosition(opponents));

        await Task.Delay(1000);

        // Wait for the attack part of the attack animation.
        attacker.Transform.BroadcastMessage("Attack");
        CharacterAnimator ca = attacker.Transform.GetComponentInChildren<CharacterAnimator>();
        if (ca != null) await ca.AttackMade;

        foreach (Combatant opponent in opponents) 
        { 
            opponent.HP = 1;
            opponent.OnHPChanged.Invoke(opponent.HP);
        }
    }
}