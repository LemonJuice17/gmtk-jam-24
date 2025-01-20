using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Walkable
{
    public Combatant Stats;

    public float MoveSpeed = 2;

    private Vector3 _currentMoveDirection;

    public IInteractable CurrentInteractable;

    public static Player instance;

    public PlayerInput Input { get; private set; }

    new internal void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        Input = GetComponent<PlayerInput>();
        Stats.OverworldObject = transform;
        Stats.IsEnemy = false;

        base.Awake();
        Agent.enabled = false;
    }

    public void Update()
    {
        Move(_currentMoveDirection);
    }

    #region Action Map Input Handling
    // ---- Overworld Action Map Input Handling ---- //
    public void OnMove(InputValue value)
    {
        _currentMoveDirection = value.Get<Vector3>();
    }
    public void OnInteract() 
    {
        CurrentInteractable?.OnInteract();
    }
    // ---- Dialogue Action Map Input Handling ---- //
    public void OnContinue()
    {
        CurrentInteractable?.OnInteract();
    }
    // ---- Combat Action Map Input Handling ---- //
    public void OnLeft()
    {
        CombatEncounter.InputLeft.Invoke();
    }
    public void OnRight()
    {
        CombatEncounter.InputRight.Invoke();
    }
    public void OnSelect()
    {
        CombatEncounter.InputSelect.Invoke();
    }
    #endregion Action Map Input Handling

    /// <summary>
    /// Moves the player for one frame.
    /// </summary>
    /// <param name="direction"> The direction to move the player in. </param>
    private void Move(Vector3 direction)
    {
        if (direction.magnitude == 0)
        {
            BroadcastMessage("ChangeMoving", false);
            return;
        }

        BroadcastMessage("ChangeMoving", true);

        Vector3 adjustedMoveVector = Camera.main.transform.rotation * direction.normalized * MoveSpeed * Time.deltaTime;
        adjustedMoveVector.y = 0;
        transform.position += adjustedMoveVector;
        transform.rotation = Quaternion.LookRotation(adjustedMoveVector, Vector3.up);
    }
}
