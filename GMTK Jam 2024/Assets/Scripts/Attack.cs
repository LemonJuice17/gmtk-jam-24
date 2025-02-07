using System.Threading.Tasks;
using Unity.VisualScripting;
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

    private Quaternion _opponentDirection;

    public async Task<int> OnAttack(Combatant attacker, Combatant opponent)
    {
        _opponentDirection = Quaternion.LookRotation(opponent.Transform.position - attacker.Transform.position);
        TweenRotation faceOpponent = new TweenRotation(0.2f, attacker.Transform, _opponentDirection, Easing.inOutSine);
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

        // Wait for the attack part of the attack animation.
        attacker.Transform.BroadcastMessage("Attack");
        await attacker.Transform.GetComponentInChildren<CharacterAnimator>().AttackMade;

        // Kill opponent if less than 0 HP.
        opponent.HP -= roundedDamage;

        if (opponent.HP < 0) 
        {
            int overDamage = Mathf.Abs(opponent.HP);
            // Ensures that multiplications with overDamage are at least multiplied by 1, while also ensuring a difference between 0 and 1 overdamage and so on.
            overDamage++;

            opponent.HP = 0;

            Rigidbody deadRigidBody;

            if (!opponent.Transform.TryGetComponent(out deadRigidBody)) deadRigidBody = opponent.Transform.AddComponent<Rigidbody>();

            deadRigidBody.isKinematic = false;
            deadRigidBody.constraints = RigidbodyConstraints.None;

            Vector3 launchVector = ((opponent.Transform.position - attacker.Transform.position).normalized + Vector3.up).normalized * overDamage;
            Debug.DrawLine(deadRigidBody.position, deadRigidBody.position + launchVector, Color.green, 3);

            deadRigidBody.AddForce(launchVector, ForceMode.Impulse);
            deadRigidBody.AddTorque(Random.rotation.eulerAngles, ForceMode.Impulse);

            opponent.Transform.BroadcastMessage("StopAllAnimations");
        }

        return roundedDamage;
    }
}
