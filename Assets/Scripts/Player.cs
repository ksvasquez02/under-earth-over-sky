using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public enum MoveState
    {
        Normal,
        Climbing,
        Locked
    }

    #region Variables
    private Entity entity;

    private PlayerInput playerInput;
    private InputAction ia_move;
    private InputAction ia_jump;
    private InputAction ia_interact;
    private InputAction ia_inventory;

    private Vector2 _moveInput;
    private bool _jumpInput;
    private bool _interactInput;
    private bool _inventoryInput;

    [SerializeField]
    private MoveState _state;

    [Header("Movement")]
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float accel = 10f;
    [SerializeField]
    private float deaccel = 20f;
    private float curSpeed = 0f;

    [Header("Air Movement")]
    [SerializeField]
    private float jumpPower = 10f;
    [SerializeField]
    private float jumpHoldTime = 0.5f;
    [SerializeField]
    private float jumpHoldBuffer = 0.02f;
    [SerializeField]
    private float jumpHoldPower = 1f;
    private float jumpHold = 0f;
    [SerializeField]
    private float airSpeed = 5f;
    [SerializeField]
    private float airAccel = 10f;

    [Header("Climbing")]
    [SerializeField]
    private float climbSpeed = 5f;
    [SerializeField]
    private float climbGravity = 1f;
    [SerializeField]
    private float climbFriction = 1f;
    private float curFriction;
    [SerializeField]
    private float climbJump = 10f;
    [SerializeField]
    private float climbJumpFrictionReq = 0.5f;

    [Header("Managers")]
    [SerializeField]
    private HUDManager hudManager;

    private readonly Inventory inventory = new();
    private readonly List<Interactable> nearbyInteractables = new(); // i hate unity
    private readonly Dictionary<string, Timer> timers = new();

    public MoveState State { get { return _state; } }
    #endregion

    #region Init
    private void Awake()
    {
        entity = GetComponent<Entity>();
        if (entity == null) throw new System.Exception("No entity found on Player!");

        playerInput = GetComponent<PlayerInput>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ia_move = InputSystem.actions.FindAction("Move");
        ia_jump = InputSystem.actions.FindAction("Jump");
        ia_interact = InputSystem.actions.FindAction("Interact");
        ia_inventory = InputSystem.actions.FindAction("Inventory");

        entity.HandleMovement += HandleMovement;
        entity.OnHandleGravity += OnHandleGravity;
    }
    #endregion

    #region InputUpdate

    private void Update()
    {
        _moveInput = ia_move.ReadValue<Vector2>();
        _jumpInput = ia_jump.IsPressed();
        _interactInput = ia_interact.WasPressedThisFrame();
        _inventoryInput = ia_inventory.WasPressedThisFrame();

        switch (_state)
        {
            case MoveState.Locked:
                UpdateLocked();
                break;
            default:
                UpdateDefault();
                break;
        }

        foreach ((string id, Timer timer) in timers)
        {
            timer.Update();
        }
    }

    private void UpdateLocked()
    {
        // Handle player input when menus are open
        switch (hudManager.State)
        {
            case MenuState.Lore:
                if (_interactInput && !hudManager.AdvanceLore())
                {
                    hudManager.HideMenu(MenuState.Lore);
                }
                break;
            case MenuState.Inventory:
                if (_inventoryInput)
                {
                    hudManager.HideMenu(MenuState.Inventory);
                }
                break;
            // If no menu is open
            case MenuState.None:
                _state = MoveState.Normal;
                break;
            default: break;
        }
    }

    private void UpdateDefault()
    {
        // Handle activate interactable
        if (_interactInput && nearbyInteractables.Count > 0)
        {
            Interactable inter = nearbyInteractables[0];
            ItemData itemData = inter.CurrentItem;
            hudManager.PopulateLore(itemData);
            if (inventory.AddItem(itemData))
            {
                hudManager.PopulateInventory(inventory);
                inter.Stage++;
                hudManager.ShowTooltip(inter);
            }
        }
        // Handle open inventory
        else if (_inventoryInput)
        {
            hudManager.ShowMenu(MenuState.Inventory);
        }
    }
    #endregion

    #region HandleMovement

    private void HandleMovement()
    {
        Vector2 vel = entity.Vel;
        curSpeed = vel.x;

        switch (_state)
        {
            case MoveState.Normal:
                vel = MovementNormal(vel);
                break;
            case MoveState.Climbing:
                vel = MovementClimbing(vel);
                break;
            case MoveState.Locked:
                vel = MovementNormal(vel, false);
                break;
        }

        entity.Vel = vel;
    }

    private Vector2 MovementNormal(Vector2 vel, bool useInput = true)
    {
        // Use appropriate ground/air speed
        float useAccel = entity.IsGrounded ? accel : airAccel;
        float useSpeed = entity.IsGrounded ? speed : airSpeed;

        // Accelerate when active
        if (useInput && Mathf.Abs(_moveInput.x) >= 0.1f)
        {
            float delta = useAccel * Time.fixedDeltaTime;
            curSpeed += delta * _moveInput.x;
        }
        // Deaccelerate when inactive
        else
        {
            float delta = deaccel * Time.fixedDeltaTime;
            curSpeed = Mathf.MoveTowards(curSpeed, 0f, delta);
        }

        // Limit and apply horizontal velocity
        curSpeed = Mathf.Clamp(curSpeed, -useSpeed, useSpeed);
        vel.x = curSpeed;

        // Handle jumping
        HandleJumpNormal(ref vel, useInput);

        // Handle passthrough platforms
        if (useInput)
        {
            if (entity.IsGrounded)
            {
                entity.IsPassThrough = _moveInput.y < -0.5f;
            }
            // Passthrough resets in OnTriggerExit2D 
        }
        else
        {
            entity.IsPassThrough = false;
        }

        // Enable gravity
        entity.IgnoreGravity = false;
        curFriction = 0f;

        return vel;
    }

    private void HandleJumpNormal(ref Vector2 vel, bool useInput = true)
    {
        // If jump is input...
        if (useInput && _jumpInput)
        {
            // Handle intial grounded jump
            if (entity.IsGrounded && vel.y <= jumpHoldPower)
            {
                vel.y = jumpPower;
                jumpHold = jumpHoldTime;
            }
            // Handle in-air held jump
            else if (jumpHold > 0f)
            {
                jumpHold -= Time.fixedDeltaTime;
                // Buffer prevents holding for the first few frames
                if (jumpHold < jumpHoldTime - jumpHoldBuffer)
                {
                    float ratio = jumpHold / jumpHoldTime;
                    vel.y += jumpHoldPower + jumpHoldPower * ratio;
                }
                // Held jump has reached its maximum length
                if (jumpHold <= 0)
                {
                    jumpHold = 0f;
                    _jumpInput = false;
                }
            }
        }
        // Jump released, stop held jump
        else
        {
            jumpHold = 0f;
        }
    }

    private Vector2 MovementClimbing(Vector2 vel)
    {
        // Handle Climb Movement
        Vector2 raw = _moveInput.normalized * climbSpeed;

        float frictionMult;
        if (_moveInput.y >= 0f)
        {
            float ratio = 0.5f;
            frictionMult = (_moveInput.y * ratio) + (1 - ratio);
        }
        else
        {
            float ratio = 0.75f;
            frictionMult = (_moveInput.y * ratio) - (1 - ratio);
        }
        curFriction += climbFriction * frictionMult * Time.fixedDeltaTime;
        curFriction = Mathf.Clamp01(curFriction);

        vel = raw;
        vel.x = (curFriction * vel.x * 0.5f) + (vel.x * 0.5f);
        vel.y = (curFriction * climbGravity) + vel.y;

        //Vector2 raw = _moveInput.normalized * climbSpeed;
        //float climbY = vel.y > raw.y && raw.y > 0 ? vel.y : raw.y; // High y-vel overrides up-input
        //vel = new Vector2(raw.x, climbY);

        // Handle Climb Jump
        if (_jumpInput && vel.y <= climbJump)
        {
            bool hasJumped = false;

            // Trying to climb up
            if (_moveInput.y >= 0f && curFriction >= climbJumpFrictionReq)
            {
                vel.y = Mathf.Max(vel.y * 0.5f + climbJump, climbJump + climbSpeed);
                hasJumped = true;
            }
            // Dropping off
            else if (_moveInput.normalized.y < 0.5f && curFriction < 0.9f)
            {
                hasJumped = true;
            } 

            if (hasJumped)
            {
                _jumpInput = false;
                jumpHold = 0f;
                _state = MoveState.Normal;
            }
        }

        // Disable Gravity
        entity.IgnoreGravity = true;

        return vel;
    }
    #endregion

    #region OnGravity

    private void OnHandleGravity()
    {
        Vector2 vel = entity.Vel;

        switch (_state)
        {
            case MoveState.Climbing:
                vel.y -= climbGravity;
                break;
            default: break;
        }

        entity.Vel = vel;
    }
    #endregion

    #region Collisions

    private void OnTriggerStay2D(Collider2D other)
    {
        if (_state == MoveState.Normal && other.gameObject.CompareTag("Climb"))
        {
            if (_moveInput.y > 0 && entity.Vel.y <= climbSpeed)
            {
                entity.Vel = new Vector2(entity.Vel.x, Mathf.Min(-climbSpeed, entity.Vel.y));
                curFriction = 0f;
                _state = MoveState.Climbing;
            }
        }

    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Dialogue"))
        {
            if (other.gameObject.TryGetComponent<Dialoguer>(out var dia))
            {
                hudManager.ShowDialogue(dia);
            }
        }

        if (other.gameObject.CompareTag("Interactable"))
        {
            if (!other.gameObject.TryGetComponent(out Interactable inter)) return;

            if (inter != null && !nearbyInteractables.Contains(inter))
            {
                nearbyInteractables.Add(inter);
                hudManager.ShowTooltip(inter);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        switch (_state)
        {
            case MoveState.Climbing:

                if (other.gameObject.CompareTag("Climb"))
                {
                    _state = MoveState.Normal;
                    curFriction = 0f;
                }
                break;

            case MoveState.Normal:

                if (other.gameObject.CompareTag("Pass-Through"))
                {
                    if (entity.IsPassThrough && !entity.IsGrounded)
                    {
                        entity.IsPassThrough = false;
                    }
                }
                break;

            default: break;
        }

        if (other.gameObject.CompareTag("Interactable"))
        {
            if (!other.gameObject.TryGetComponent(out Interactable inter)) return;

            if (inter != null && nearbyInteractables.Contains(inter))
            {
                nearbyInteractables.Remove(inter);
                hudManager.HideTooltip(inter);
            }
        }
    }
    #endregion

    #region API
    public bool LockPlayer()
    {
        if (_state == MoveState.Locked) return false;
        _state = MoveState.Locked;
        playerInput.SwitchCurrentActionMap("UI");
        return true;
    }
    #endregion
}