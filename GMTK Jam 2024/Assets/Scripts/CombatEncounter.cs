using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CombatEncounter : MonoBehaviour
{
    public Dictionary<Combatant, int> Combatants = new();
    [HideInInspector] public List<Combatant> CombatantOrder = new();
    [HideInInspector] public List<Combatant> AllyCombatants = new();
    public List<Combatant> EnemyCombatants;

    public Vector3 AllyLineOffset = new Vector3(0, 0, -2);
    public Vector3 EnemyLineOffset = new Vector3(0, 0, 2);

    public float CombatantSpacing = 1;

    int _rollsFinished = 0;

    private CinemachineVirtualCamera _camera;

    public float DicePositionMultiplier = 2.5f;

    public float FightLoopUpdateTime = 0.1f;

    private List<Image> _turnOrderImages = new();

    [SerializeField] private int _currentTurnIndex;

    private void Awake()
    {
        _camera = GetComponentInChildren<CinemachineVirtualCamera>();
    }

    private void Start()
    {
        if (GameManager.instance.CombatUIObjectReference.activeSelf) GameManager.instance.CombatUIObjectReference.SetActive(false);
    }

    public void StartCombat(Dialogue dialogue)
    {
        StartCombat();
    }

    public void StartCombat()
    {
        Player.instance.Input.SwitchCurrentActionMap("Combat");
        _camera.Priority = 20;

        AllyCombatants.Add(Player.instance.Stats);

        GameObject.FindGameObjectsWithTag("Party Member").ToList().ForEach(obj => AllyCombatants.Add(obj.GetComponent<PartyMember>().Stats));

        AllyCombatants.ForEach((ally) => Combatants.Add(ally, 0));
        EnemyCombatants.ForEach((enemy) => Combatants.Add(enemy, 0));

        Invoke("PositionCombatants", 1);
        Invoke("RollForInitiative", 2);
    }

    public void StopCombat()
    {
        Player.instance.Input.SwitchCurrentActionMap("Overworld");
        _camera.Priority = 1;

        for (int i = 0; i < AllyCombatants.Count; i++)
        {
            AllyCombatants[i].OverworldObject.GetComponent<Rigidbody>().isKinematic = false;
            if (AllyCombatants[i].OverworldObject.TryGetComponent(out PartyMember pm)) pm.StartFollowLoop();
        }

        GameManager.instance.CombatUIObjectReference.SetActive(false);

        CancelInvoke("FightLoop");
    }

    private void PositionCombatants()
    {
        Vector3 direction = EnemyLineOffset - AllyLineOffset;
        direction.Normalize();

        for(int i = 0; i < AllyCombatants.Count; i++)
        {
            AllyCombatants[i].OverworldObject.transform.position = transform.position + AllyLineOffset + new Vector3((-AllyCombatants.Count + 1) * (CombatantSpacing * 0.5f) + (i * CombatantSpacing), 0, 0);
            AllyCombatants[i].OverworldObject.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            AllyCombatants[i].OverworldObject.GetComponent<Rigidbody>().isKinematic = true;
            if (AllyCombatants[i].OverworldObject.TryGetComponent(out PartyMember pm)) pm.StopFollowLoop();
        }

        for (int i = 0; i < EnemyCombatants.Count; i++)
        {
            // Instantiate enemy (if needed)
            if (EnemyCombatants[i].CombatantPrefab != null)
            {
                EnemyCombatants[i].OverworldObject = Instantiate(EnemyCombatants[i].CombatantPrefab).transform;
            }

            // Position enemy
            if (EnemyCombatants[i].OverworldObject != null)
            {
                EnemyCombatants[i].OverworldObject.transform.position = transform.position + EnemyLineOffset + new Vector3((-EnemyCombatants.Count + 1) * (CombatantSpacing * 0.5f) + (i * CombatantSpacing), 0, 0);
                EnemyCombatants[i].OverworldObject.transform.rotation = Quaternion.LookRotation(-direction, Vector3.up);
            }
        }
    }

    public void RollForInitiative()
    {
        Vector3 direction = EnemyLineOffset - AllyLineOffset;
        direction.Normalize();

        foreach(var combatant in AllyCombatants)
        {
            Dice die = Instantiate(GameManager.instance.D6).GetComponent<Dice>();
            die.transform.position = combatant.OverworldObject.transform.position - (direction * DicePositionMultiplier);
            die.Roll(combatant);
            die.RolledValue.AddListener(AddRollResult);
        }

        foreach (var combatant in EnemyCombatants)
        {
            Dice die = Instantiate(GameManager.instance.D6).GetComponent<Dice>();
            die.transform.position = combatant.OverworldObject.transform.position + (direction * DicePositionMultiplier);
            die.Roll(combatant);
            die.RolledValue.AddListener(AddRollResult);
        }
    }

    private void AddRollResult(Combatant combatant, int result)
    {
        Combatants[combatant] = result;
        _rollsFinished++;
        if (_rollsFinished == Combatants.Count) StartFightLoop();
    }

    private void StartFightLoop()
    {
        CombatantOrder = Combatants.OrderByDescending(pair => pair.Value)
                                   .Select(pair => pair.Key)
                                   .ToList();

        GameManager.instance.CombatUIObjectReference.SetActive(true);
        GenerateIcons(CombatantOrder);

        InvokeRepeating("FightLoop", 0, FightLoopUpdateTime);
    }

    public void GenerateIcons(List<Combatant> combatants)
    {
        foreach (Combatant combatant in combatants)
        {
            GameObject newIcon = Instantiate(GameManager.instance.TurnOrderIconPrefab, GameManager.instance.TurnOrderObjectReference);
            if(combatant.TurnOrderIcon != null) newIcon.transform.GetChild(0).GetComponent<Image>().sprite = combatant.TurnOrderIcon;
        }
    }

    private void FightLoop()
    {
        if(_currentTurnIndex >= CombatantOrder.Count) _currentTurnIndex = 0;

        Combatant currentCombatant = CombatantOrder[_currentTurnIndex];

        GameManager.instance.CombatUINameText.text = $"{currentCombatant.OverworldObject.name}'s Turn";

        if (currentCombatant.OverworldObject == Player.instance.gameObject)
        {
            PlayersTurn();
            CancelInvoke("FightLoop");
            return;
        }

        _currentTurnIndex++;

        Attack(currentCombatant, currentCombatant.Attacks[Random.Range(0, currentCombatant.Attacks.Count - 1)]);
    }

    public void PlayersTurn()
    {
        _currentTurnIndex++;
        //InvokeRepeating("FightLoop", 0, FightLoopUpdateTime);
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + AllyLineOffset, 0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + EnemyLineOffset, 0.5f);
    }

    #region Attacks
    public void Attack(Combatant attacker, Attacks attack, Combatant victim = null)
    {
        switch(attack)
        {
            case Attacks.Stab:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} stabbed {victim.OverworldObject.name}, dealing {attacker.Strength} + [d4] damage!";
                    break;
                }
            case Attacks.Stabs:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} stabbed {victim.OverworldObject.name} several times, dealing {attacker.Strength} + [2 d4] damage!";
                    break;
                }
            case Attacks.Slash:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} slashed {victim.OverworldObject.name}, dealing {attacker.Strength} + [d10] damage!";
                    break;
                }
            case Attacks.Crush:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} crushed {victim.OverworldObject.name}, dealing {attacker.Strength * 2} damage!";
                    break;
                }
            case Attacks.Taunt:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} taunted {victim.OverworldObject.name}, dealing {attacker.Charm} damage!";
                    break;
                }
            case Attacks.Mock:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} mocked {victim.OverworldObject.name}, dealing {attacker.Charm} + [d4] damage!";
                    break;
                }
            case Attacks.Insult:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} insulted {victim.OverworldObject.name}, dealing {attacker.Charm} + [2 d4] damage!";
                    break;
                }
            case Attacks.Seduce:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} seduced {victim.OverworldObject.name}, dealing {attacker.Charm} + [d20] damage!";
                    break;
                }
            case Attacks.Blast:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} blasted {victim.OverworldObject.name}, dealing {attacker.Magic} + [d8] damage!";
                    break;
                }
            case Attacks.Shrink:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} shrank {victim.OverworldObject.name}, halving their size (and HP)!";
                    break;
                }
            case Attacks.Fireball:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} hurled a fireball at {victim.OverworldObject.name}, dealing {attacker.Magic} + [d20] damage!";
                    break;
                }
        }

        if(victim.HP <= 0)
        {
            CancelInvoke("FightLoop");
            Invoke("CombatantKilled", FightLoopUpdateTime);
        }
    }

    private Combatant _killedCombatant;

    private void CombatantKilled()
    {
        GameManager.instance.CombatUIDescriptionText.text = $"{_killedCombatant} has been slain!";

        _currentTurnIndex--;

        CombatantOrder.RemoveAt(_currentTurnIndex);
        CombatantOrder.Remove(_killedCombatant);
        Destroy(_killedCombatant.OverworldObject);

        InvokeRepeating("FightLoop", FightLoopUpdateTime, FightLoopUpdateTime);
    }

    private Combatant GetRandomOpponent(Combatant attacker)
    {
        if (AllyCombatants.Contains(attacker))
        {
            return EnemyCombatants[Random.Range(0, EnemyCombatants.Count - 1)];
        }

        else
        {
            return AllyCombatants[Random.Range(0, AllyCombatants.Count - 1)];
        }
    }

    private List<Combatant> GetAllOpponents(Combatant attacker)
    {
        return null;
    }

    private Combatant GetRandomAlly(Combatant attacker)
    {
        return null;
    }

    private List<Combatant> GetAllAllies(Combatant attacker)
    {
        return null;
    }
    #endregion Attacks
}

[System.Serializable]
public class Combatant
{
    public int MaxHP = 5;
    public int HP = 5;

    public int Strength = 1;
    public int Magic = 1;
    public int Charm = 1;

    public Sprite TurnOrderIcon;

    public Transform OverworldObject;
    public GameObject CombatantPrefab;

    public List<Attacks> Attacks;
}

public enum Attacks
{
    Stab,
    Stabs,
    Slash,
    Crush,
    Taunt,
    Mock,
    Insult,
    Seduce,
    Blast,
    Shrink,
    Fireball
}