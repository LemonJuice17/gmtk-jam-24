using UnityEngine;

public class StatAdder : MonoBehaviour
{
    //public Attack AttackToGive;
    [SerializeField] private Stats StatToGive;
    public int StatAmount = 1;

    private enum Stats
    {
        strength,
        charm,
        magic
    }

    public void GiveAttack()
    {
        //Player.instance.Stats.Attacks.Add(AttackToGive);
    }

    public void GiveStat()
    {
        
    }
}