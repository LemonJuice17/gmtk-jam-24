using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using static Player;
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

    /// <summary>
    /// References to the turn order icon gameobjects.
    /// </summary>
    private readonly List<GameObject> _turnOrderIcons = new();
    /// <summary>
    /// The spacing between turn order icons.
    /// </summary>
    public float TurnIconSpacing = 80;

    private CinemachineVirtualCamera _combatCamera;

    bool _combatInProgress = true;

    public async void StartEncounter()
    {
        instance.Input.SwitchCurrentActionMap("Combat");

        _combatCamera = GetComponentInChildren<CinemachineVirtualCamera>();

        if (_combatCamera != null) _combatCamera.Priority = 100;
        else Debug.LogAssertion("This combat does not have an assigned virtual camera. Create one as a child of this object.");

        // Spawn enemy models.
        await InstantiateEnemies(EnemySpawnTime);

        _cattank = GameManager.instance.CattankReference;
        _gilbert = GameManager.instance.GilbertReference;

        // Move party to correct positions.
        Task[] moveToPositionTasks = new Task[3];

        instance.CurrentWalkMode = new WalkToPoint(instance, transform.position + RelativePlayerPosition);
        _cattank.CurrentWalkMode = new WalkToPoint(_cattank, transform.position + RelativeCattankPosition);
        _gilbert.CurrentWalkMode = new WalkToPoint(_gilbert, transform.position + RelativeGilbertPosition);

        moveToPositionTasks[0] = (instance.CurrentWalkMode as WalkToPoint).WaitForCompletion;
        moveToPositionTasks[1] = (_cattank.CurrentWalkMode as WalkToPoint).WaitForCompletion;
        moveToPositionTasks[2] = (_gilbert.CurrentWalkMode as WalkToPoint).WaitForCompletion;

        await Task.WhenAll(moveToPositionTasks);

        instance.CurrentWalkMode = new StandStill(instance);
        _cattank.CurrentWalkMode = new StandStill(_cattank);
        _gilbert.CurrentWalkMode = new StandStill(_gilbert);

        await Task.Delay(1000);

        // Add all combatants to a single list.
        CombatantList.Add(new Combatant(instance.GetComponent<CombatProfile>(), instance.transform, Team.ally, true));
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

        float spacingStart = (CombatantList.Count - 1) * 0.5f * -TurnIconSpacing;

        for (int i = 0; i < CombatantList.Count; i++)
        {
            Vector3 position = GameManager.instance.CombatTurnOrderObjectReference.transform.position + new Vector3(spacingStart + TurnIconSpacing * i, 0, 0);
            _turnOrderIcons.Add(Instantiate(
                GameManager.instance.CombatTurnOrderIconPrefab,
                position + new Vector3(1080, 0, 0),
                Quaternion.identity,
                GameManager.instance.CombatTurnOrderObjectReference.transform));

            _turnOrderIcons[i].transform.GetChild(0).GetComponent<Image>().sprite = CombatantQueue.Peek().Profile.Character.CharacterSprite;
            _turnOrderIcons[i].name = CombatantQueue.Peek().Profile.Character.name;
            CombatantQueue.Enqueue(CombatantQueue.Peek());
            CombatantQueue.Dequeue();

            new Tween(0.4f, _turnOrderIcons[i].transform, position, Easing.outSine);
            await Task.Delay(400);
        }

        await Task.Delay(1000);

        // Main combat loop.
        while (_combatInProgress)
        {
            await Task.Delay(500);

            Combatant nextCombatant = CombatantQueue.Peek();
            CombatantQueue.Dequeue();
            CombatantQueue.Enqueue(nextCombatant);

            if (nextCombatant.IsPlayer) await PlayerTurn(nextCombatant);
            else await AITurn(nextCombatant);

            await Task.Delay(500);
            
            await CycleTurnOrderUI();
        }
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

    /// <summary>
    /// Automatically plays the given combatant's turn using a random attack from their CombatProfile and random target on the other team.
    /// </summary>
    /// <param name="ai"> The combatant who's turn to automatically play. </param>
    private async Task AITurn(Combatant ai)
    {
        // Enable/disable relevant UI.
        GameManager.instance.CombatUIPanelObjectReference.SetActive(true);
        GameManager.instance.CombatUINameText.text = $"{ai.Profile.Character.CharacterName}'s Turn";
        GameManager.instance.CombatUIPlayerAttackOptionsObjectReference.SetActive(false);
        GameManager.instance.CombatUIDescriptionText.gameObject.SetActive(true);
        GameManager.instance.CombatUIDescriptionText.text = $"";

        await Task.Delay(1000);

        // Randomly choose an attack and an opponent to attack.
        int randomAttackIndex = Random.Range(0, ai.Profile.Attacks.Length - 1);

        List<Combatant> enemies = CombatantList.Where((combatant) => ai.Team != combatant.Team).ToList();
        Combatant opponent = enemies[Random.Range(0, enemies.Count - 1)];

        await Attack(ai, opponent, opponent.Profile.Attacks[randomAttackIndex]);
    }

    Attack selectedAttack = null;
    Combatant selectedCombatant = null;

    Attack[] attackSelectionList = null;
    Combatant[] combatantSelectionList = null;
    TMP_Text[] selectionObjectList = null;

    int playerSelectionIndex = 0;

    bool isPlayerTurn;
    bool selectingAttack;
    bool selectingOpponent;

    private async Task PlayerTurn(Combatant player)
    {
        GameManager.instance.CombatUIPanelObjectReference.SetActive(true);
        GameManager.instance.CombatUINameText.text = "Your Turn";
        GameManager.instance.CombatUIPlayerAttackOptionsObjectReference.SetActive(true);
        GameManager.instance.CombatUIDescriptionText.gameObject.SetActive(false);

        isPlayerTurn = true;
        selectingAttack = true;
        selectingOpponent = false;

        await Task.Delay(1000);

        instance.MoveSelectionLeft.AddListener(PlayerSelectionLeft);
        instance.MoveSelectionRight.AddListener(PlayerSelectionRight);
        instance.EnterSelection.AddListener(PlayerSelectionEnter);
        instance.CancelSelection.AddListener(PlayerSelectionCancel);

        while (isPlayerTurn)
        {
            ShowAvailableAttacks();

            while (selectingAttack)
            {
                await Task.Yield();
            }

            ShowAvailableOpponents();

            while (selectingOpponent)
            {
                await Task.Yield();
            }
        }

        RemoveCurrentPlayerOptions();

        instance.MoveSelectionLeft.RemoveListener(PlayerSelectionLeft);
        instance.MoveSelectionRight.RemoveListener(PlayerSelectionRight);
        instance.EnterSelection.RemoveListener(PlayerSelectionEnter);
        instance.CancelSelection.RemoveListener(PlayerSelectionCancel);

        GameManager.instance.CombatUIDescriptionText.gameObject.SetActive(true);

        await Attack(player, selectedCombatant, selectedAttack);

        await Task.Delay(1000);

        void ShowAvailableAttacks()
        {
            RemoveCurrentPlayerOptions();

            attackSelectionList = player.Profile.Attacks;
            selectionObjectList = new TMP_Text[attackSelectionList.Length];

            for (int i = 0; i < attackSelectionList.Length; i++)
            {
                selectionObjectList[i] = Instantiate(GameManager.instance.CombatUIPlayerOptionsTextPrefab, GameManager.instance.CombatUIPlayerAttackOptionsObjectReference.transform);
                selectionObjectList[i].text = player.Profile.Attacks[i].name;
            }

            playerSelectionIndex = 0;
            selectionObjectList[playerSelectionIndex].color = Color.green;
        }

        void ShowAvailableOpponents()
        {
            RemoveCurrentPlayerOptions();

            combatantSelectionList = CombatantList.Where((combatant) => combatant.Team == Team.enemy).ToArray();
            selectionObjectList = new TMP_Text[combatantSelectionList.Length];

            for (int i = 0; i < combatantSelectionList.Length; i++)
            {
                selectionObjectList[i] = Instantiate(GameManager.instance.CombatUIPlayerOptionsTextPrefab, GameManager.instance.CombatUIPlayerAttackOptionsObjectReference.transform);
                selectionObjectList[i].text = combatantSelectionList[i].Profile.name;
            }

            playerSelectionIndex = 0;
            selectionObjectList[playerSelectionIndex].color = Color.green;
        }

        void RemoveCurrentPlayerOptions()
        {
            if (selectionObjectList == null) return;

            foreach(TMP_Text textObject in selectionObjectList)
            {
                Destroy(textObject.gameObject);
            }

            selectionObjectList = null;
        }
    }

    void PlayerSelectionLeft()
    {
        selectionObjectList[playerSelectionIndex].color = Color.white;

        playerSelectionIndex--;

        if(playerSelectionIndex < 0) playerSelectionIndex = selectionObjectList.Length - 1;

        selectionObjectList[playerSelectionIndex].color = Color.green;
    }

    void PlayerSelectionRight()
    {
        selectionObjectList[playerSelectionIndex].color = Color.white;

        playerSelectionIndex++;

        if (playerSelectionIndex > selectionObjectList.Length - 1) playerSelectionIndex = 0;

        selectionObjectList[playerSelectionIndex].color = Color.green;
    }

    void PlayerSelectionEnter()
    {
        if (selectingAttack)
        {
            selectedAttack = attackSelectionList[playerSelectionIndex];
            selectingAttack = false;
            selectingOpponent = true;
        }

        else if (selectingOpponent)
        {
            selectedCombatant = combatantSelectionList[playerSelectionIndex];
            selectingOpponent = false;
            isPlayerTurn = false;
        }
    }

    void PlayerSelectionCancel()
    {
        if (selectingOpponent)
        {
            selectingAttack = true;
            selectingOpponent = false;
        }
    }

    private async Task Attack(Combatant attacker, Combatant opponent, Attack attack)
    {
        GameManager.instance.CombatUIDescriptionText.text = $"{attacker.Profile.Character.CharacterName} is using {attack.name} on {opponent.Profile.Character.CharacterName}.";

        await Task.Delay(500);

        // Wait for the attack to finish.
        int damageDealt = await attack.OnAttack(attacker, opponent);
        await Task.Delay(1000);

        GameManager.instance.CombatUIDescriptionText.text = $"{attacker.Profile.Character.CharacterName} dealt {damageDealt} damage to {opponent.Profile.Character.CharacterName} using {attack.name}.";


        // If the attack killed the enemy.
        if (opponent.HP == 0)
        {
            await Task.Delay(3000);
            GameManager.instance.CombatUIDescriptionText.text = $"{attacker.Profile.Character.CharacterName} slayed {opponent.Profile.Character.CharacterName}.";
            await RemoveCombatant(opponent);
        }

        await Task.Delay(3000);
    }

    /// <summary>
    /// Animates the cycling of the turn order UI, moving the first combatant to the back of the queue and shuffling the others forward to keep it centered.
    /// </summary>
    /// <returns></returns>
    private async Task CycleTurnOrderUI()
    {
        Vector3 lastIconPosition = _turnOrderIcons[^1].transform.position;

        Tween tweenFirstIconOffscreen = new (0.3f, _turnOrderIcons[0].transform, _turnOrderIcons[0].transform.position - new Vector3(1080, 0), Easing.inSine);

        List<Task> shuffleTasks = new();
        for (int i = 1; i < _turnOrderIcons.Count; i++)
        {
            Tween tween = new(0.6f, _turnOrderIcons[i].transform, _turnOrderIcons[i].transform.position - new Vector3(TurnIconSpacing, 0), Easing.inOutSine);
            shuffleTasks.Add(tween.TweenCompletion);
        }

        await tweenFirstIconOffscreen.TweenCompletion;
        _turnOrderIcons[0].transform.position = lastIconPosition + new Vector3(1080, 0);
        new Tween(0.3f, _turnOrderIcons[0].transform, lastIconPosition, Easing.outSine);

        await Task.WhenAll(shuffleTasks);
        
        GameObject firstIcon = _turnOrderIcons[0];
        _turnOrderIcons.RemoveAt(0);
        _turnOrderIcons.Add(firstIcon);
    }

    /// <summary>
    /// Removes a combatant from the CombatantQueue, Combants list, and animates the removal of their turn order UI. 
    /// </summary>
    /// <param name="combatant"> The combatant to remove. </param>
    private async Task RemoveCombatant(Combatant combatant)
    {
        CombatantList.Remove(combatant);
        GameObject turnIcon = _turnOrderIcons.Find((icon) => icon.name == combatant.Profile.Character.name);

        Tween removalTween = new(0.3f, turnIcon.transform, turnIcon.transform.position + new Vector3(0, 200), Easing.inSine);
        await removalTween.TweenCompletion;

        _turnOrderIcons.Remove(turnIcon);
        Destroy(turnIcon);

        await Task.Delay(1000);

        float spacingStart = (CombatantList.Count - 1) * 0.5f * -TurnIconSpacing;

        List<Task> cycleRemainingIcons = new();
        for (int i = 0; i < _turnOrderIcons.Count; i++)
        {
            Vector3 position = GameManager.instance.CombatTurnOrderObjectReference.transform.position + new Vector3(spacingStart + TurnIconSpacing * i, 0, 0);
            Tween tween = new(0.6f, _turnOrderIcons[i].transform, position, Easing.inOutSine);
            cycleRemainingIcons.Add(tween.TweenCompletion);
        }

        await Task.WhenAll(cycleRemainingIcons);

        // Remove the combatant from the CombatQueue
        for (int i = 0; i <= CombatantList.Count; i++) 
        {
            Combatant nextInQueue = CombatantQueue.Peek();
            CombatantQueue.Dequeue();
            if (nextInQueue != combatant)
            {
                CombatantQueue.Enqueue(nextInQueue);
            }
        }

        // Check if all enemies are dead.
        if (CombatantList.Where((combatant) => combatant.Team == Team.enemy).ToList().Count == 0)
        {
            Debug.Log("All enemies dead.");
            StopEncounter();
            CombatVictory();
        }

        // Check if all allies are dead.
        else if (CombatantList.Where((combatant) => combatant.Team == Team.ally).ToList().Count == 0)
        {
            Debug.Log("All allies dead.");
            StopEncounter();
            CombatLoss();
        }
    }

    public async void StopEncounter()
    {
        _combatInProgress = false;

        foreach(GameObject icon in _turnOrderIcons)
        {
            Destroy(icon);
        }

        GameManager.instance.CombatTurnOrderObjectReference.SetActive(false);
        GameManager.instance.CombatUIObjectReference.SetActive(false);
        GameManager.instance.CombatUIPanelObjectReference.SetActive(false);
        GameManager.instance.CombatUINameText.text = "";
        GameManager.instance.CombatUIPlayerAttackOptionsObjectReference.SetActive(false);
        GameManager.instance.CombatUIDescriptionText.gameObject.SetActive(false);
        GameManager.instance.CombatUIDescriptionText.text = "";

        ResetIfDead(instance.gameObject, transform.position + RelativePlayerPosition);
        ResetIfDead(_gilbert.gameObject, transform.position + RelativeGilbertPosition);
        ResetIfDead(_cattank.gameObject, transform.position + RelativeCattankPosition);

        instance.CurrentWalkMode = new PlayerMovement(instance);
        _gilbert.CurrentWalkMode = new FollowTarget(_gilbert, instance.transform, _gilbert.DistanceBeforeMoving);
        _cattank.CurrentWalkMode = new FollowTarget(_cattank, instance.transform, _cattank.DistanceBeforeMoving);


        instance.Input.SwitchCurrentActionMap("Overworld");

        _combatCamera.Priority = 10;

        await Task.Delay(1000);

        static void ResetIfDead(GameObject partyMember, Vector3 resetPosition)
        {
            if (partyMember.TryGetComponent(out Rigidbody rb) && rb.constraints == RigidbodyConstraints.None)
            {
                rb.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                rb.isKinematic = true;
                GameManager.instance.CreatePoofEffect(partyMember.transform.position); 
                partyMember.transform.SetPositionAndRotation(resetPosition, Quaternion.identity);
                GameManager.instance.CreatePoofEffect(resetPosition);
                partyMember.BroadcastMessage("StartAnimations");
            }
        }
    }

    public void CombatVictory()
    {

    }

    public void CombatLoss()
    {

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