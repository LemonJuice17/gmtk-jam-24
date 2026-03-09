using UnityEngine;

public class GiveAttack : MonoBehaviour
{
    [SerializeField] Attack attack;
    [SerializeField] CombatProfile profile;

    public void AddAttack()
    {
        profile.Attacks.Add(attack);
    }
}
