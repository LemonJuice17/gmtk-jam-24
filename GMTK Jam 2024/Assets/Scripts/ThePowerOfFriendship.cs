using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "The Power Of Friendship")]
public class PowerOfFriendship : Attack
{
    public PowerOfFriendship() => AttackDescription = "The Power of Friendship";

    public async Task<int> OnAttack(Combatant[] attackers, Combatant opponent)
    {
        List<Task> faceEnemyTasks = new();
        foreach (Combatant attacker in attackers) { faceEnemyTasks.Add(FaceOpponent(attacker, opponent)); }
        await Task.WhenAll(faceEnemyTasks);

        List<Task<int>> rollResults = new();
        foreach (Combatant attacker in attackers) 
        {
            rollResults.Add(GameManager.instance.CreateDice(8, attacker.Transform.position + (Vector3.up * 2)).Roll(-attacker.Transform.forward * 1.5f));
            await Task.Delay(200);
        }
        await Task.WhenAll(rollResults);

        int damage = 0;
        foreach (Task<int> rollResult in rollResults) { damage += rollResult.Result; }

        await Task.Delay(1000);

        List<Task> attackAnimations = new();
        foreach (Combatant attacker in attackers)
        {
            attacker.Transform.BroadcastMessage("Attack");
            CharacterAnimator ca = attacker.Transform.GetComponentInChildren<CharacterAnimator>();
            if (ca != null) attackAnimations.Add(ca.AttackMade);
        }
        await Task.WhenAll(attackAnimations);

        AttackOpponent(AverageCombatantPosition(attackers), opponent, damage);

        return damage;
    }
}