using JetBrains.Annotations;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Player : Walkable
{
    public float MoveSpeed = 2;

    private Vector3 _currentMoveDirection;

    public IInteractable CurrentInteractable;

    public static Player instance;

    public UnityEvent MoveSelectionLeft = new();
    public UnityEvent MoveSelectionRight = new();
    public UnityEvent EnterSelection = new();
    public UnityEvent CancelSelection = new();

    public PlayerInput Input { get; private set; }

    new internal void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        Input = GetComponent<PlayerInput>();

        base.Awake(); 
    }

    private void Start()
    {
        CurrentWalkMode = new PlayerMovement(this);
    }


    #region Action Map Input Handling
    // ---- Overworld Action Map Input Handling ---- //
    [UsedImplicitly] public void OnMove(InputValue value)
    {
        _currentMoveDirection = value.Get<Vector3>();
    }
    [UsedImplicitly] public void OnInteract() 
    {
        CurrentInteractable?.OnInteract();
    }



    // ---- Dialogue Action Map Input Handling ---- //
    [UsedImplicitly] public void OnContinue()
    {
        CurrentInteractable?.OnInteract();
    }



    // ---- Combat Action Map Input Handling ---- //
    [UsedImplicitly] public void OnLeft()
    {
        MoveSelectionLeft.Invoke();
    }
    [UsedImplicitly] public void OnRight()
    {
        MoveSelectionRight.Invoke();
    }
    [UsedImplicitly] public void OnSelect()
    {
        EnterSelection.Invoke();
    }
    [UsedImplicitly] public void OnCancel()
    {
        CancelSelection.Invoke();
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

    public class PlayerMovement : WalkMode
    {
        public Player Player;
        public PlayerMovement(Walkable walker) : base(walker)
        {
            if (Walker is Player) Player = Walker as Player;
            else throw new System.Exception("Cannot give a non-player the PlayerMovement WalkMode.");

            Walker.Agent.enabled = false;   
            Walker.BroadcastMessage("ChangeMoving", false);
            Walker._walkModeCoroutine = Walker.StartCoroutine(PlayerMovementLoop());

            if (Player.TryGetComponent(out Rigidbody rb)) rb.isKinematic = false;
        }

        private IEnumerator PlayerMovementLoop()
        {
            while (true)
            {
                Player.Move(Player._currentMoveDirection);
                yield return null;
            }
        }
    }
}
