using UnityEngine;

[CreateAssetMenu(fileName = "CombatEncounterSettings", menuName = "CombatEncounterSettings", order = 0)]
public class CombatEncounterSettings : ScriptableObject {
    /// <summary>
    /// How many seconds enemies will be given to spawn.
    /// </summary>
    public float EnemySpawnTime = 1;

    /// <summary>
    /// The spacing between turn order icons.
    /// </summary>
    public float TurnIconSpacing = 80;

    public Color SkipTurnFadeColour = new(0.75f, 0.75f, 0.75f, 0.75f);


    [Header("Combat Start Timings")]
    public float TimeBetweenMoveToPositionAndDiceRoll = 1;
    public float TimeBetweenRollingEachDiceForTurnOrder = 0.2f;
    public float TimeBetweenDiceRollAndTurnOrderDisplay = 3;
    public float TimeBetweenDisplayingEachTurnOrderIcon = 0.4f;
    public float TimeBetweenTurnOrderDisplayAndCombatLoopStart = 1;
    
    [Header("General Turn Timings")]
    public float TimeBeforeTurnStart = 0.5f;
    public float TimeBetweenAttackEndAndTurnOrderCycling = 0.5f;

    public float TimeBetweenDeclaringTurnAndAttack = 1;

    [Header("Attack Timings")]
    public float TimeBetweenAttackUsedMessageAndAttackAnimation = 0.5f;
    public float TimeBetweenAttackAnimationAndAttackResultsText = 1;
    public float TimeAfterAttack = 3;

    [Header("Misc Timings")]
    public float TimeAfterPlayerTurn = 1;

    public float TimeBetweenRemovingTurnOrderIconAndResortingTurnOrderIcons = 1;

}