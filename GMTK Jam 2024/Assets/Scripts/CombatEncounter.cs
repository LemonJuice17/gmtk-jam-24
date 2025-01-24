using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CombatEncounter : MonoBehaviour
{
    public Vector3 RelativeCattankPosition;
    public Vector3 RelativePlayerPosition;
    public Vector3 RelativeGilbertPosition;

    public GameObject[] Enemies;
    public Vector3[] RelativeEnemyPositions;

    public float EnemySpawnTime = 1;

    public bool ShowCombatPositionGizmos;

    public async void StartEncounter()
    {
        Player.instance.Input.SwitchCurrentActionMap("combat");

        await InstantiateEnemies(EnemySpawnTime);

        GameManager.instance.CattankReference.FollowingPlayer = false;
        GameManager.instance.GilbertReference.FollowingPlayer = false;

        Task[] moveToPositionTasks = new Task[3];

        moveToPositionTasks[0] = GameManager.instance.CattankReference.WalkToPosition(transform.position + RelativeCattankPosition);
        moveToPositionTasks[1] = Player.instance.WalkToPosition(transform.position + RelativePlayerPosition);
        moveToPositionTasks[2] = GameManager.instance.GilbertReference.WalkToPosition(transform.position + RelativeGilbertPosition);

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