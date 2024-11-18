using UnityEngine;

public class StatAdder : MonoBehaviour
{
    public Attack AttackToGive;
    [SerializeField] private Stats StatToGive;
    public int StatAmmount = 1;

    private enum Stats
    {
        strength,
        charm,
        magic
    }

    public void GiveAttack()
    {
        Player.instance.Stats.Attacks.Add(AttackToGive);
    }

    public void GiveStat()
    {
        switch (StatToGive)
        {
            case Stats.strength:
                Player.instance.Stats.Strength += StatAmmount;
                break;
            case Stats.charm:
                Player.instance.Stats.Charm += StatAmmount;
                break;
            case Stats.magic:
                Player.instance.Stats.Magic += StatAmmount;
                break;
        }
    }
}