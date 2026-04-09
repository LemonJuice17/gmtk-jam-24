using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack")]
public class Attack : ScriptableObject
{
    [Header("Attack Multipliers")]
    public float StrengthDamageMultiplier = 0;
    public float CharmDamageMultiplier = 0;
    public float MagicDamageMultiplier = 0;

    [Header("Dice Used")]
    public int D4Damage = 0;
    public int D6Damage = 0;
    public int D8Damage = 0;

    [Header("Descriptions")]
    public string AttackDescription;
    public string AttackMessageDescription;

    [Header("Dialogue Options")]
    [Range(0, 1)]
    public float DialogueChance = 0;
    [TextArea(3, 3)]
    public List<string> DialogueOptions = new();
    public float DialogueDuration = 2;

    [Header("Audio Options")]
    public SoundObject CustomAttackSound;

    private Quaternion _opponentDirection;

    public async virtual Task<int> OnAttack(Combatant attacker, Combatant opponent)
    {
        await FaceOpponent(attacker, opponent);

        float totalDamage =
            attacker.Profile.Power * StrengthDamageMultiplier +
            attacker.Profile.Charm * CharmDamageMultiplier +
            attacker.Profile.Magic * MagicDamageMultiplier;

        Task<int>[] rollResults = new Task<int>[D4Damage + D6Damage + D8Damage];

        for (int i = 0; i < D4Damage; i++)
        {
            rollResults[i] = GameManager.instance.CreateDice(4, attacker.Transform.position + (Vector3.up * 2)).Roll(-attacker.Transform.forward * 1.5f);
            await Task.Delay(200);
        }

        for (int i = D4Damage; i < D4Damage + D6Damage; i++)
        {
            rollResults[i] = GameManager.instance.CreateDice(6, attacker.Transform.position + (Vector3.up * 2)).Roll(-attacker.Transform.forward * 1.5f);
            await Task.Delay(200);
        }

        for (int i = D4Damage + D6Damage; i < D4Damage + D6Damage + D8Damage; i++)
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

        // Display dialogue and wait for it to finish.
        if (DialogueChance > 0 && Random.Range(0f, 1f) <= DialogueChance)
        {
            // Create temporary GameObject for Dialogue component.
            GameObject temporaryDialogueObject = new GameObject("Temporary Dialogue");
            Dialogue temporaryDialogue = temporaryDialogueObject.AddComponent<Dialogue>();
            string selectedDialogue = DialogueOptions[Random.Range(0, DialogueOptions.Count - 1)];
            DialogueText text = new DialogueText(selectedDialogue, attacker.Profile.Character);
            text.Actions.AddListener((dialogue) => dialogue.EndDialogue());
            temporaryDialogue.DialogueList.Add(text);
            GameManager.instance.CombatUIPanelObjectReference.SetActive(false);
            temporaryDialogue.StartDialogue(false, DialogueDuration);

            await temporaryDialogue.DialogueFinished;

            Player.instance.Input.SwitchCurrentActionMap("Combat");
            Destroy(temporaryDialogue.gameObject);
        }

        GameManager.instance.CombatUIPanelObjectReference.SetActive(true);

        // Wait for the attack part of the attack animation.
        CharacterAnimator ca = attacker.Transform.GetComponentInChildren<CharacterAnimator>();
        if(ca != null)
        {
            if(CustomAttackSound != null) ca.AttackSFXOverride = CustomAttackSound;
            ca.Attack();
            await ca.AttackMade;
            ca.AttackSFXOverride = null;
        }

        AttackOpponent(attacker, opponent, roundedDamage);

        return roundedDamage;
    }

    protected async Task FaceOpponent(Combatant attacker, Combatant opponent)
    {
        _opponentDirection = Quaternion.LookRotation(opponent.Transform.position - attacker.Transform.position);
        TweenRotation faceOpponent = new(0.2f, attacker.Transform, _opponentDirection, Easing.inOutSine);
        await faceOpponent.TweenCompletion;
    }
    protected async Task FaceOpponent(Combatant attacker, Vector3 position)
    {
        _opponentDirection = Quaternion.LookRotation(position - attacker.Transform.position);
        TweenRotation faceOpponent = new(0.2f, attacker.Transform, _opponentDirection, Easing.inOutSine);
        await faceOpponent.TweenCompletion;
    }


    protected void AttackOpponent(Combatant attacker, Combatant opponent, int damage) => 
        AttackOpponent(attacker.Transform.position, opponent, damage);
    protected void AttackOpponent(Vector3 attackOrigin, Combatant opponent, int damage) {
        opponent.HP -= damage;

        if (opponent.HP <= 0)
        {
            int overDamage = Mathf.Abs(opponent.HP);
            // Ensures that multiplications with overDamage are at least multiplied by 1, while also ensuring a difference between 0 and 1 overdamage and so on.
            overDamage++;

            opponent.HP = 0;

            if (!opponent.Transform.TryGetComponent(out Rigidbody deadRigidBody)) deadRigidBody = opponent.Transform.gameObject.AddComponent<Rigidbody>();

            deadRigidBody.isKinematic = false;
            deadRigidBody.constraints = RigidbodyConstraints.None;

            Vector3 launchVector = ((opponent.Transform.position - attackOrigin).normalized + Vector3.up).normalized * overDamage;
            Debug.DrawLine(deadRigidBody.position, deadRigidBody.position + launchVector, Color.green, 3);

            deadRigidBody.AddForce(launchVector, ForceMode.Impulse);
            deadRigidBody.AddTorque(Random.rotation.eulerAngles, ForceMode.Impulse);

            opponent.Transform.BroadcastMessage("StopAnimations");
        }

        opponent.OnHPChanged.Invoke(opponent.HP);
    }

    protected Vector3 AverageCombatantPosition(Combatant[] combatants)
    {
        Vector3 sumPos = new();
        foreach (Combatant combatant in combatants) { sumPos += combatant.Transform.position; }
        return sumPos / combatants.Length;
    }

}