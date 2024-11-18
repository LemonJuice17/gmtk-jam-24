using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class Dice
{
    public int Sides;

    public Dice (int sides = 6)
    {
        if (AllowedSideCounts.Contains(sides))
        {
            Sides = sides;
        }

        else
        {
            throw new Exception($"A D{sides} has tried to be made. Dice with this number of faces are not currently allowed. Only the following are allowed: {AllowedSideCounts}");
        }
    }

    public static int[] AllowedSideCounts = { 6, 8 };

    /// <summary>
    /// Spawns a physical instance of this dice.
    /// </summary>
    /// <param name="position"> The position of the dice. </param>
    /// <param name="parent"> The parent of the dice (optional) </param>
    /// <returns></returns>
    public DiceObject SpawnDice(Vector3 position, Transform parent = null) => GameManager.instance.CreateDice(Sides, position, parent);
}