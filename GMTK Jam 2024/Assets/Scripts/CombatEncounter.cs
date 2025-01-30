using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
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

    /// <summary>
    /// The list of all current combatants. Dead combatants are removed from this list.
    /// </summary>
    public List<Combatant> CombatantList { get; private set; } = new();
    /// <summary>
    /// The queue for combatants to attack in. Dead combatants are only removed when (what would be) their turn is reached.
    /// </summary>
    public Queue<Combatant> CombatantQueue { get; private set; } = new();

    private List<GameObject> _turnOrderIcons = new();
    public float turnIconSpacing = 60;

    public async void StartEncounter()
    {
        Player.instance.Input.SwitchCurrentActionMap("Combat");

        // Spawn enemy models.
        await InstantiateEnemies(EnemySpawnTime);

        _cattank = GameManager.instance.CattankReference;
        _gilbert = GameManager.instance.GilbertReference;

        // Move party to correct positions.
        Task[] moveToPositionTasks = new Task[3];

        _cattank.CurrentWalkMode = new WalkToPoint(_cattank, transform.position + RelativeCattankPosition);
        Player.instance.CurrentWalkMode = new WalkToPoint(Player.instance, transform.position + RelativePlayerPosition);
        _gilbert.CurrentWalkMode = new WalkToPoint(_gilbert, transform.position + RelativeGilbertPosition);

        moveToPositionTasks[0] = (_cattank.CurrentWalkMode as WalkToPoint).WaitForCompletion;
        moveToPositionTasks[1] = (Player.instance.CurrentWalkMode as WalkToPoint).WaitForCompletion;
        moveToPositionTasks[2] = (_gilbert.CurrentWalkMode as WalkToPoint).WaitForCompletion;

        await Task.WhenAll(moveToPositionTasks);

        await Task.Delay(1000);

        // Add all combatants to a single list.
        CombatantList.Add(new Combatant(Player.instance.GetComponent<CombatProfile>(), Player.instance.transform, Team.ally));
        CombatantList.Add(new Combatant(_cattank.GetComponent<CombatProfile>(), _cattank.transform, Team.ally));
        CombatantList.Add(new Combatant(_gilbert.GetComponent<CombatProfile>(), _gilbert.transform, Team.ally));

        for (int i = 0; i < Enemies.Length; i++)
        {
            try
            {
                CombatantList.Add(new Combatant(Enemies[i].GetComponent<CombatProfile>(), _enemyTransforms[i], Team.enemy));
            }
            catch
            {
                throw new System.Exception($"The enemy {Enemies[i].name} does not have an associated combat profile. Add a CombatProfile component to this enemy prefab try again.");
            }
        }

        // Roll dice to get the order of combat.
        Task<int>[] rollResults = new Task<int>[CombatantList.Count];

        for (int i = 0; i < CombatantList.Count; i++)
        {
            rollResults[i] = GameManager.instance.CreateDice(6, CombatantList[i].Transform.position + (Vector3.up * 2)).Roll(-CombatantList[i].Transform.forward * 1.5f);
            await Task.Delay(200);
        }

        await Task.WhenAll(rollResults);

        await Task.Delay(3000);

        // Calculate the order of combat from the previously calculated rolls.
        Dictionary<Combatant, int> combatantRolls = new();

        for (int i = 0; i < rollResults.Length; i++)
        {
            combatantRolls.Add(CombatantList[i], rollResults[i].Result);
        }

        CombatantQueue = new Queue<Combatant>(
            combatantRolls.OrderByDescending(roll => roll.Value)
            .Select(roll => roll.Key)
            .ToList());

        // Create the UI for showing turn order.
        GameManager.instance.CombatUIObjectReference.SetActive(true);
        GameManager.instance.CombatTurnOrderObjectReference.SetActive(true);

        float spacingStart = (CombatantList.Count - 1) * 0.5f * -turnIconSpacing;

        for (int i = 0; i < CombatantList.Count; i++)
        {
            Vector3 position = GameManager.instance.CombatTurnOrderObjectReference.transform.position + new Vector3(spacingStart + turnIconSpacing * i, 0, 0);
            _turnOrderIcons.Add(Instantiate(
                GameManager.instance.CombatTurnOrderIconPrefab,
                position + new Vector3(1080, 0, 0),
                Quaternion.identity,
                GameManager.instance.CombatTurnOrderObjectReference.transform));

            _turnOrderIcons[i].transform.GetChild(0).GetComponent<Image>().sprite = CombatantQueue.Peek().Profile.Character.CharacterSprite;
            CombatantQueue.Enqueue(CombatantQueue.Peek());
            CombatantQueue.Dequeue();

            new Tween(0.4f, _turnOrderIcons[i].transform, position, Easing.outSine);
            await Task.Delay(400);
        }
    }

    public void StopEncounter()
    {
        GameManager.instance.CombatTurnOrderObjectReference.SetActive(false);
        GameManager.instance.CombatUIObjectReference.SetActive(false);
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