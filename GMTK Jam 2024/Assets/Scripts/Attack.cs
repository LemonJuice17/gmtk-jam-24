using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "New Attack")]
public class Attack : ScriptableObject
{
    public float StrengthDamageMultiplier = 0;
    public float CharmDamageMultiplier = 0;
    public float MagicDamageMultiplier = 0;

    [Space]
    public int D6Damage = 0;
    public int D8Damage = 0;

    public async Task<int> OnAttack(Combatant attacker, Combatant opponent)
    {
        TweenRotation faceOpponent = new TweenRotation(0.2f, attacker.Transform, Quaternion.LookRotation(opponent.Transform.position - attacker.Transform.position), Easing.inOutSine);
        await faceOpponent.TweenCompletion;

        float totalDamage =
            attacker.Profile.Strenth * StrengthDamageMultiplier +
            attacker.Profile.Charm * CharmDamageMultiplier +
            attacker.Profile.Magic * MagicDamageMultiplier;

        Task<int>[] rollResults = new Task<int>[D6Damage + D8Damage];

        for (int i = 0; i < D6Damage; i++)
        {
            rollResults[i] = GameManager.instance.CreateDice(6, attacker.Transform.position + (Vector3.up * 2)).Roll(-attacker.Transform.forward * 1.5f);
            await Task.Delay(200);
        }

        for (int i = 0; i < D8Damage; i++)
        {
            rollResults[i] = GameManager.instance.CreateDice(8, attacker.Transform.position + (Vector3.up * 2)).Roll(-attacker.Transform.forward * 1.5f);
            await Task.Delay(200);
        }

        await Task.WhenAll(rollResults);

        foreach (Task<int> rollResult in rollResults)
        {
            totalDamage += rollResult.Result;
        }

        int roundedDamage = Mathf.RoundToInt(totalDamage);

        await Task.Delay(1000);

        attacker.Transform.BroadcastMessage("Attack");
        opponent.HP -= roundedDamage;

        return roundedDamage;
    }
}
