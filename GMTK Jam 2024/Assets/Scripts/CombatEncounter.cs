using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CombatEncounter : MonoBehaviour
{
    public Dictionary<Combatant, int> Combatants = new();
    [HideInInspector] public List<Combatant> CombatantOrder = new();
    [HideInInspector] public List<Combatant> AllyCombatants = new();
    public List<Combatant> EnemyCombatants;
    // These are copied from the two lists above, but don't get changed.
    // Used for references after combat is done.
    [HideInInspector] public List<Combatant> Allies;
    [HideInInspector] public List<Combatant> Enemies;

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

    public UnityEvent OnVictory = new();
    public UnityEvent OnLoss = new();

    public float DeathExplosionForce = 1000;

    private void Awake()
    {
        _camera = GetComponentInChildren<CinemachineVirtualCamera>();

        InputLeft.AddListener(PlayerInputLeft);
        InputRight.AddListener(PlayerInputRight);
        InputSelect.AddListener(PlayerInputSelect);
    }

    private void Start()
    {
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

        foreach (Transform child in GameManager.instance.CombatUIPlayerOptionsObjectReference.transform)
        {
            Destroy(child.gameObject);
        };

        foreach (Attacks attack in Player.instance.Stats.Attacks)
        {
            _playerAttackText.Add(Instantiate(GameManager.instance.CombatUIPlayerOptionsTextPrefab, GameManager.instance.CombatUIPlayerOptionsObjectReference.transform));
            _playerAttackText.Last().text = attack.ToString();
            _playerAttackText.Last().color = GameManager.instance.UnselectedTextColour;
        }

        CombatantOrder.Clear();
        Combatants.Clear();
        AllyCombatants.Clear();
        if (Enemies.Count != 0) EnemyCombatants = Enemies;

        AllyCombatants.Add(Player.instance.Stats);

        GameObject.FindGameObjectsWithTag("Party Member").ToList().ForEach(obj => AllyCombatants.Add(obj.GetComponent<PartyMember>().Stats));

        AllyCombatants.ForEach((ally) => Combatants.Add(ally, 0));
        EnemyCombatants.ForEach((enemy) => Combatants.Add(enemy, 0));

        if (Allies.Count == 0) AllyCombatants.ForEach((ally) => Allies.Add(ally));
        if (Enemies.Count == 0) EnemyCombatants.ForEach((enemy) => Enemies.Add(enemy));

        foreach (var pair in Combatants)
        {
            pair.Key.HP = pair.Key.MaxHP;
        }

        Invoke("PositionCombatants", 1);
        Invoke("RollForInitiative", 2);
    }

    public void StopCombat()
    {
        if (Player.instance.Input.currentActionMap.name == "Combat")
        {
            Player.instance.Input.SwitchCurrentActionMap("Overworld");
        }
        
        _camera.Priority = 1;

        for (int i = 0; i < Allies.Count; i++)
        {
            Rigidbody rb = Allies[i].OverworldObject.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.freezeRotation = true;
            rb.velocity = Vector3.zero;
            Allies[i].OverworldObject.transform.position = transform.position + (transform.rotation * (AllyLineOffset + new Vector3((-Allies.Count + 1) * (CombatantSpacing * 0.5f) + (i * CombatantSpacing), 0, 0)));
            Allies[i].OverworldObject.transform.rotation = Quaternion.identity;
            if (Allies[i].OverworldObject.TryGetComponent(out PartyMember pm)) pm.StartFollowLoop();
        }

        GameManager.instance.CombatUIPanelObjectReference.SetActive(false);
        GameManager.instance.CombatUIPlayerOptionsObjectReference.SetActive(false);
        GameManager.instance.CombatUIDescriptionText.gameObject.SetActive(false);
        GameManager.instance.CombatUIObjectReference.SetActive(false);

        CancelInvoke();
    }

    private void PositionCombatants()
    {
        Vector3 direction = EnemyLineOffset - AllyLineOffset;
        direction.Normalize();

        for(int i = 0; i < AllyCombatants.Count; i++)
        {
            AllyCombatants[i].OverworldObject.transform.position = transform.position + (transform.rotation * (AllyLineOffset + new Vector3((-AllyCombatants.Count + 1) * (CombatantSpacing * 0.5f) + (i * CombatantSpacing), 0, 0)));
            AllyCombatants[i].OverworldObject.transform.rotation = Quaternion.LookRotation(transform.rotation * direction, Vector3.up);
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
                EnemyCombatants[i].OverworldObject.transform.position = transform.position + (transform.rotation * (EnemyLineOffset + new Vector3((-EnemyCombatants.Count + 1) * (CombatantSpacing * 0.5f) + (i * CombatantSpacing), 0, 0)));
                EnemyCombatants[i].OverworldObject.transform.rotation = Quaternion.LookRotation(transform.rotation * -direction, Vector3.up);
            }
        }
    }

    public void RollForInitiative()
    {
        Vector3 direction = EnemyLineOffset - AllyLineOffset;
        direction.Normalize();

        _rollsFinished = 0;

        foreach (var combatant in AllyCombatants)
        {
            Dice die = Instantiate(GameManager.instance.D6).GetComponent<Dice>();
            die.transform.position = combatant.OverworldObject.transform.position + (transform.rotation * direction * DicePositionMultiplier);
            die.Roll(combatant);
            die.RolledValue.AddListener(AddRollResult);
        }

        foreach (var combatant in EnemyCombatants)
        {
            Dice die = Instantiate(GameManager.instance.D6).GetComponent<Dice>();
            die.transform.position = combatant.OverworldObject.transform.position - (transform.rotation * direction * DicePositionMultiplier);
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

        InvokeRepeating("FightLoop", FightLoopUpdateTime * 0.5f, FightLoopUpdateTime);
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

        StartAttack(currentCombatant, currentCombatant.Attacks[Random.Range(0, currentCombatant.Attacks.Count - 1)]);
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
        StartAttack(Player.instance.Stats, Player.instance.Stats.Attacks[_selectedAttackIndex]);
        InvokeRepeating("FightLoop", FightLoopUpdateTime, FightLoopUpdateTime);
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + (transform.rotation * AllyLineOffset), 0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + (transform.rotation * EnemyLineOffset), 0.5f);
    }

    #region Attacks

    private Attacks _currentAttack;
    public void StartAttack(Combatant attacker, Attacks attack, Combatant victim = null)
    {
        _currentAttack = 0;
        _currentRollTotal = 0;

        switch (attack)
        {
            case Attacks.Stab:
                RollDice(attacker, GameManager.instance.D6);
                break;
            case Attacks.Stabs:
                RollDice(attacker, GameManager.instance.D6, 2);
                break;
            case Attacks.Slash:
                RollDice(attacker, GameManager.instance.D8);
                break;
            case Attacks.Crush:
                Attack(attacker, attack);
                break;
            case Attacks.Taunt:
                Attack(attacker, attack);
                break;
            case Attacks.Mock:
                RollDice(attacker, GameManager.instance.D6);
                break;
            case Attacks.Insult:
                RollDice(attacker, GameManager.instance.D6, 2);
                break;
            case Attacks.Seduce:
                RollDice(attacker, GameManager.instance.D8, 3);
                break;
            case Attacks.Blast:
                RollDice(attacker, GameManager.instance.D8);
                break;
            case Attacks.Shrink:
                Attack(attacker, attack);
                break;
            case Attacks.Fireball:
                RollDice(attacker, GameManager.instance.D8, 3);
                break;
        }

        _currentAttack = attack;
    }

    public void Attack(Combatant attacker, Attacks attack, int diceModifier = 0, Combatant victim = null)
    {
        GameManager.instance.CombatUIDescriptionText.gameObject.SetActive(true);

        switch (attack)
        {
            case Attacks.Stab:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} stabbed {victim.OverworldObject.name}, dealing {attacker.Strength} + {diceModifier} damage!";
                    victim.HP -= attacker.Strength + diceModifier;
                    break;
                }
            case Attacks.Stabs:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} stabbed {victim.OverworldObject.name} several times, dealing {attacker.Strength} + {diceModifier} damage!";
                    victim.HP -= attacker.Strength + diceModifier;
                    break;
                }
            case Attacks.Slash:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} slashed {victim.OverworldObject.name}, dealing {attacker.Strength * 2} + {diceModifier} damage!";
                    victim.HP -= attacker.Strength * 2 + diceModifier; ;
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
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} mocked {victim.OverworldObject.name}, dealing {attacker.Charm} + {diceModifier} damage!";
                    victim.HP -= attacker.Charm + diceModifier;
                    break;
                }
            case Attacks.Insult:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} insulted {victim.OverworldObject.name}, dealing {attacker.Charm} + {diceModifier} damage!";
                    victim.HP -= attacker.Charm + diceModifier;
                    break;
                }
            case Attacks.Seduce:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} seduced {victim.OverworldObject.name}, dealing {attacker.Charm} +  {diceModifier} damage!";
                    victim.HP -= attacker.Charm + diceModifier;
                    break;
                }
            case Attacks.Blast:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} blasted {victim.OverworldObject.name}, dealing {attacker.Magic} +  {diceModifier} damage!";
                    victim.HP -= attacker.Magic + diceModifier;
                    break;
                }
            case Attacks.Shrink:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} shrank {victim.OverworldObject.name}, halving their size (and HP)!";
                    victim.HP = (int)Math.Ceiling(victim.HP * 0.5f);
                    victim.OverworldObject.localScale *= 0.5f;
                    break;
                }
            case Attacks.Fireball:
                {
                    if (victim == null) victim = GetRandomOpponent(attacker);
                    GameManager.instance.CombatUIDescriptionText.text = $"{attacker.OverworldObject.name} hurled a fireball at {victim.OverworldObject.name}, dealing {attacker.Magic} +  {diceModifier} damage!";
                    victim.HP -= attacker.Magic + diceModifier;
                    break;
                }
        }

        attacker.OverworldObject.BroadcastMessage("Attack", SendMessageOptions.DontRequireReceiver);

        if(victim.HP <= 0)
        {
            _killedCombatant = victim;
            CancelInvoke("FightLoop");
            Invoke("CombatantKilled", FightLoopUpdateTime);
        }

        else if(diceModifier != 0) InvokeRepeating("FightLoop", FightLoopUpdateTime, FightLoopUpdateTime);
    }

    private int _rollCount = 1;
    private int _currentRolls = 0;
    private int _currentRollTotal = 0;

    public void RollDice(Combatant attacker, GameObject diePrefab, int quantity = 1)
    {
        CancelInvoke("FightLoop");

        Vector3 direction = EnemyLineOffset - AllyLineOffset;
        direction.Normalize();

        _rollCount = quantity;
        _currentRolls = 0;
        _currentRollTotal = 0;

        for (int i = 0; i < quantity; i++)
        {
            Dice die = Instantiate(diePrefab).GetComponent<Dice>();
            die.transform.position = attacker.OverworldObject.transform.position + (transform.rotation * direction * DicePositionMultiplier);
            die.Roll(attacker);
            die.RolledValue.AddListener(WaitForAllRolls);
        }
    }
    
    private void WaitForAllRolls(Combatant combatant, int roll)
    {
        _currentRollTotal += roll;
        _currentRolls++;

        if (_currentRolls >= _rollCount) Attack(combatant, _currentAttack, _currentRollTotal);
    }

    private Combatant _killedCombatant;

    private void CombatantKilled()
    {
        GameManager.instance.CombatUIDescriptionText.text = $"{_killedCombatant.OverworldObject.name} has been slain!";

        _currentTurnIndex--;

        CombatantOrder.Remove(_killedCombatant);

        Rigidbody rb = _killedCombatant.OverworldObject.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.freezeRotation = false;

        AllyCombatants.Remove(_killedCombatant);
        rb.AddExplosionForce(DeathExplosionForce, _killedCombatant.OverworldObject.transform.position + Vector3.down, 2);

        if (AllyCombatants.Contains(_killedCombatant)) AllyCombatants.Remove(_killedCombatant);
        if (EnemyCombatants.Contains(_killedCombatant)) EnemyCombatants.Remove(_killedCombatant);

        _killedCombatant = null;

        if (AllyCombatants.Count == 0)
        {
            Invoke("DelayedLoss", 1);
            return;
        }

        if (EnemyCombatants.Count == 0)
        {
            StopCombat();
            OnVictory.Invoke();
            Destroy(this);
            Enemies.ForEach(enemy =>
            {
                if (enemy.OverworldObject.TryGetComponent(out Dialogue deadNPCDialogue))
                {
                    Destroy(deadNPCDialogue);
                }
            });
            return;
        }

        _currentTurnIndex++;

        InvokeRepeating("FightLoop", FightLoopUpdateTime, FightLoopUpdateTime);
    }

    private void DelayedLoss() 
    {
        StopCombat();
        OnLoss.Invoke();

        // Position enemy
        for (int i = 0; i < Enemies.Count; i++)
        {
            if (Enemies[i].OverworldObject != null)
            {
                Enemies[i].OverworldObject.transform.position = transform.position + EnemyLineOffset + new Vector3((-EnemyCombatants.Count + 1) * (CombatantSpacing * 0.5f) + (i * CombatantSpacing), 0, 0);
            }
        }
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