using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
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

    [SerializeField] private int _currentTurnIndex;

    private List<TMP_Text> _playerAttackText = new();

    private int _selectedAttackIndex = 0;

    public static UnityEvent InputLeft = new();
    public static UnityEvent InputRight = new();
    public static UnityEvent InputSelect = new();

    private void Awake()
    {
        _camera = GetComponentInChildren<CinemachineVirtualCamera>();

        InputLeft.AddListener(PlayerInputLeft);
        InputRight.AddListener(PlayerInputRight);
        InputSelect.AddListener(PlayerInputSelect);
    }

    private void Start()
    {
        foreach(Attacks attack in Player.instance.Stats.Attacks)
        {
            _playerAttackText.Add(Instantiate(GameManager.instance.CombatUIPlayerOptionsTextPrefab, GameManager.instance.CombatUIPlayerOptionsObjectReference.transform));
            _playerAttackText.Last().text = attack.ToString();
            _playerAttackText.Last().color = GameManager.instance.UnselectedTextColour;
        }

        GameManager.instance.CombatUIPanelObjectReference.SetActive(false);
        GameManager.instance.CombatUIPlayerOptionsObjectReference.SetActive(false);
        GameManager.instance.CombatUIDescriptionText.gameObject.SetActive(false);
        GameManager.instance.CombatUIObjectReference.SetActive(false);
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

        Invoke("PositionCombatants", 1.5f);
        Invoke("RollForInitiative", 3);
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

        InvokeRepeating("FightLoop", FightLoopUpdateTime, FightLoopUpdateTime);
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
        GameManager.instance.CombatUIPanelObjectReference.SetActive(true);

        if (_currentTurnIndex >= CombatantOrder.Count) _currentTurnIndex = 0;

        Combatant currentCombatant = CombatantOrder[_currentTurnIndex];

        GameManager.instance.CombatUINameText.text = $"{currentCombatant.OverworldObject.name}'s Turn";

        if (Player.instance != null && currentCombatant.OverworldObject == Player.instance.transform)
        {
            PlayersTurn();
            return;
        }

        _currentTurnIndex++;

        Attack(currentCombatant, currentCombatant.Attacks[Random.Range(0, currentCombatant.Attacks.Count - 1)]);
    }

    public void PlayersTurn()
    {
        CancelInvoke("FightLoop");
        _currentTurnIndex++;
        GameManager.instance.CombatUIDescriptionText.gameObject.SetActive(false);
        GameManager.instance.CombatUIPlayerOptionsObjectReference.gameObject.SetActive(true);
        _playerAttackText[_selectedAttackIndex].color = GameManager.instance.SelectedTextColour;
    }

    private void PlayerInputLeft()
    {
        if (!GameManager.instance.CombatUIPlayerOptionsObjectReference.activeSelf) return;
        if (_selectedAttackIndex > 0)
        {
            _playerAttackText[_selectedAttackIndex].color = GameManager.instance.UnselectedTextColour;
            _selectedAttackIndex--;
            _playerAttackText[_selectedAttackIndex].color = GameManager.instance.SelectedTextColour;
        }
    }
    private void PlayerInputRight()
    {
        if (!GameManager.instance.CombatUIPlayerOptionsObjectReference.activeSelf) return;
        if (_selectedAttackIndex < Player.instance.Stats.Attacks.Count - 1)
        {
            _playerAttackText[_selectedAttackIndex].color = GameManager.instance.UnselectedTextColour;
            _selectedAttackIndex++;
            _playerAttackText[_selectedAttackIndex].color = GameManager.instance.SelectedTextColour;
        }
    }
    private void PlayerInputSelect()
    {
        if (!GameManager.instance.CombatUIPlayerOptionsObjectReference.activeSelf) return;
        GameManager.instance.CombatUIPlayerOptionsObjectReference.gameObject.SetActive(false);
        Attack(Player.instance.Stats, Player.instance.Stats.Attacks[_selectedAttackIndex]);
        InvokeRepeating("FightLoop", FightLoopUpdateTime, FightLoopUpdateTime);
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
        GameManager.instance.CombatUIDescriptionText.gameObject.SetActive(true);

        switch (attack)
        {
            case Attacks.Stab:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} stabbed {victim.OverworldObject.name}, dealing {attacker.Strength} + [d4] damage!";
                    victim.HP -= attacker.Strength;
                    break;
                }
            case Attacks.Stabs:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} stabbed {victim.OverworldObject.name} several times, dealing {attacker.Strength} + [2 d4] damage!";
                    victim.HP -= attacker.Strength;
                    break;
                }
            case Attacks.Slash:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} slashed {victim.OverworldObject.name}, dealing {attacker.Strength} + [d10] damage!";
                    victim.HP -= attacker.Strength;
                    break;
                }
            case Attacks.Crush:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} crushed {victim.OverworldObject.name}, dealing {attacker.Strength * 2} damage!";
                    victim.HP -= attacker.Strength;
                    break;
                }
            case Attacks.Taunt:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} taunted {victim.OverworldObject.name}, dealing {attacker.Charm} damage!";
                    victim.HP -= attacker.Charm;
                    break;
                }
            case Attacks.Mock:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} mocked {victim.OverworldObject.name}, dealing {attacker.Charm} + [d4] damage!";
                    victim.HP -= attacker.Charm;
                    break;
                }
            case Attacks.Insult:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} insulted {victim.OverworldObject.name}, dealing {attacker.Charm} + [2 d4] damage!";
                    victim.HP -= attacker.Charm;
                    break;
                }
            case Attacks.Seduce:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} seduced {victim.OverworldObject.name}, dealing {attacker.Charm} + [d20] damage!";
                    victim.HP -= attacker.Charm;
                    break;
                }
            case Attacks.Blast:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} blasted {victim.OverworldObject.name}, dealing {attacker.Magic} + [d8] damage!";
                    victim.HP -= attacker.Magic;
                    break;
                }
            case Attacks.Shrink:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} shrank {victim.OverworldObject.name}, halving their size (and HP)!";
                    victim.HP -= attacker.Magic;
                    break;
                }
            case Attacks.Fireball:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} hurled a fireball at {victim.OverworldObject.name}, dealing {attacker.Magic} + [d20] damage!";
                    victim.HP -= attacker.Magic;
                    break;
                }
        }

        if(victim.HP <= 0)
        {
            _killedCombatant = victim;
            CancelInvoke("FightLoop");
            Invoke("CombatantKilled", FightLoopUpdateTime);
        }
    }

    private Combatant _killedCombatant;

    private void CombatantKilled()
    {
        GameManager.instance.CombatUIDescriptionText.text = $"{_killedCombatant.OverworldObject.name} has been slain!";

        _currentTurnIndex--;

        CombatantOrder.Remove(_killedCombatant);

        if (AllyCombatants.Contains(_killedCombatant)) AllyCombatants.Remove(_killedCombatant);
        else EnemyCombatants.Remove(_killedCombatant);

        Destroy(_killedCombatant.OverworldObject.gameObject);

        _killedCombatant = null;

        if (AllyCombatants.Count == 0)
        {
            StopCombat();
        }

        if (EnemyCombatants.Count == 0)
        {
            StopCombat();
        }

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