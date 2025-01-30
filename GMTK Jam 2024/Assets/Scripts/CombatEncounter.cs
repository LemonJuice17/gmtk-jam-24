using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static Walkable;

public class CombatEncounter : MonoBehaviour
{
    // The relative positions (from this Transform) the party will move to when the fight starts.
    public Vector3 RelativeCattankPosition;
    public Vector3 RelativePlayerPosition;
    public Vector3 RelativeGilbertPosition;

    // The enemy GameObjects and their relative positions (from this Transform) the'll be spawned in when the fight starts.
    public GameObject[] Enemies;
    public Vector3[] RelativeEnemyPositions;
    private Transform[] _enemyTransforms;

    /// <summary>
    /// How many seconds enemies will be given to spawn.
    /// </summary>
    public float EnemySpawnTime = 1;

    /// <summary>
    /// A toggle for showing the positions that players and enemies will move to/be spawned in.
    /// </summary>
    public bool ShowCombatPositionGizmos;

    // References to circumvent referencing their singletons every time.
    private PartyMember _cattank;
    private PartyMember _gilbert;

    public Queue<Combatant> CombatantOrder { get; private set; }

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

        await Task.Delay(1000);

        List<Combatant> unorderedCombatants = new();

        unorderedCombatants.Add(new Combatant(Player.instance.GetComponent<CombatProfile>(), Player.instance.transform));
        unorderedCombatants.Add(new Combatant(_cattank.GetComponent<CombatProfile>(), _cattank.transform));
        unorderedCombatants.Add(new Combatant(_gilbert.GetComponent<CombatProfile>(), _gilbert.transform));

        for (int i = 0; i < Enemies.Length; i++)
        {
            try
            {
                unorderedCombatants.Add(new Combatant(Enemies[i].GetComponent<CombatProfile>(), _enemyTransforms[i]));
            }
            catch
            {
                throw new System.Exception($"The enemy {Enemies[i].name} does not have an associated combat profile. Add a CombatProfile component to this enemy prefab try again.");
            }
        }

        Task<int>[] rollResults = new Task<int>[unorderedCombatants.Count];

        for (int i = 0; i < unorderedCombatants.Count; i++)
        {
            rollResults[i] = GameManager.instance.CreateDice(6, unorderedCombatants[i].Transform.position + (Vector3.up * 3)).Roll(-unorderedCombatants[i].Transform.forward);
        }

        await Task.WhenAll(rollResults);

        // calculate turn order
    }

    private async Task InstantiateEnemies(float time)
    {
        _enemyTransforms = new Transform[Enemies.Length];

        for (int i = 0; i < Enemies.Length; i++)
        {
            _enemyTransforms[i] = InstantiateCharacter.InstantiateCharacterStatic(
                Enemies[i],
                transform.position + RelativeEnemyPositions[i],
                Quaternion.LookRotation(transform.position))
                .transform;
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