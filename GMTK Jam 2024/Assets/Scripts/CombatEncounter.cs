using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static Walkable;

public class CombatEncounter : MonoBehaviour
{
    public Vector3 RelativeCattankPosition;
    public Vector3 RelativePlayerPosition;
    public Vector3 RelativeGilbertPosition;

    public GameObject[] Enemies;
    public Vector3[] RelativeEnemyPositions;

    public float EnemySpawnTime = 1;

    public bool ShowCombatPositionGizmos;

    private PartyMember _cattank;
    private PartyMember _gilbert;

    public async void StartEncounter()
    {
        Player.instance.Input.SwitchCurrentActionMap("Combat");

        await InstantiateEnemies(EnemySpawnTime);

        _cattank = GameManager.instance.CattankReference;
        _gilbert = GameManager.instance.GilbertReference;

        Task[] moveToPositionTasks = new Task[3];

        _cattank.CurrentWalkMode = new WalkToPoint(_cattank, transform.position + RelativeCattankPosition);
        Player.instance.CurrentWalkMode = new WalkToPoint(Player.instance, transform.position + RelativePlayerPosition);
        _gilbert.CurrentWalkMode = new WalkToPoint(_gilbert, transform.position + RelativeGilbertPosition);

        moveToPositionTasks[0] = (_cattank.CurrentWalkMode as WalkToPoint).WaitForCompletion;
        moveToPositionTasks[1] = (Player.instance.CurrentWalkMode as WalkToPoint).WaitForCompletion;
        moveToPositionTasks[2] = (_gilbert.CurrentWalkMode as WalkToPoint).WaitForCompletion;

        await Task.WhenAll(moveToPositionTasks);

        Debug.Log("All Combatants in Position");
    }

    private async Task InstantiateEnemies(float time)
    {
        for(int i = 0; i < Enemies.Length; i++)
        {
            InstantiateCharacter.InstantiateCharacterStatic(Enemies[i], transform.position + RelativeEnemyPositions[i], Quaternion.identity);
        }

        await Task.Delay((int)(time * 1000));
    }

    private void OnDrawGizmos()
    {
        if (ShowCombatPositionGizmos)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawSphere(transform.position + RelativeCattankPosition, 0.25f);
            Gizmos.DrawSphere(transform.position + RelativePlayerPosition, 0.25f);
            Gizmos.DrawSphere(transform.position + RelativeGilbertPosition, 0.25f);

            Gizmos.color = Color.red;
            for (int i = 0; i < Enemies.Length; i++)
            {
                Gizmos.DrawSphere(transform.position + RelativeEnemyPositions[i], 0.25f);
            }
        }
    }
}