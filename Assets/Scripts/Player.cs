using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Entity entity;

    private InputAction ia_move;
    private InputAction ia_jump;
    private InputAction ia_interact;
    private InputAction ia_inventory;

    private Vector2 _moveInput;
    private bool _jumpInput;
    private bool _interactInput;
    private bool _inventoryInput;

    private MoveState _state;
    private Dictionary<string, Timer> timers = new();

    [Header("Movement")]
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float accel = 10f;
    [SerializeField]
    private float deaccel = 20f;
    [Header("Air Movement")]
    [SerializeField]
    private float jumpPower = 10f;
    [SerializeField]
    private float jumpHoldTime = 0.5f;
    [SerializeField]
    private float jumpHoldBuffer = 0.02f;
    [SerializeField]
    private float jumpHoldPower = 1f;
    [SerializeField]
    private float airSpeed = 5f;
    [SerializeField]
    private float airAccel = 10f;

    private float curSpeed = 0f;
    private float jumpHold = 0f;

    [Header("Climbing")]
    [SerializeField]
    private float climbSpeed = 5f;
    [SerializeField]
    private float climbGravity = 1f;
    [SerializeField]
    private float climbJump = 10f;

    private List<Interactable> nearbyInteractables = new(); // i hate unity

    [Header("Managers")]
    [SerializeField]
    private HUDManager hudManager;

    private Inventory inventory;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventory = new Inventory();

        entity = GetComponent<Entity>();
        if (entity == null)
        {
            throw new System.Exception("No entity found on Player!");
        }

        ia_move = InputSystem.actions.FindAction("Move");
        ia_jump = InputSystem.actions.FindAction("Jump");
        ia_interact = InputSystem.actions.FindAction("Interact");
        ia_inventory = InputSystem.actions.FindAction("Inventory");

        entity.HandleMovement += HandleMovement;
        entity.OnHandleGravity += OnHandleGravity;
    }

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

        foreach (Interactable i in nearbyInteractables)
        {

        }

        foreach ((string id, Timer timer) in timers)
        {
            timer.Update();
        }
    }

    private void UpdateLocked()
    {
        switch (hudManager.State)
        {
            case (int)MenuState.Lore:
                if (_interactInput && !hudManager.AdvanceLore())
                {
                    hudManager.HideMenu(MenuState.Lore);
                }
                break;
            case (int)MenuState.Inventory:
                if (_inventoryInput)
                {
                    hudManager.HideMenu(MenuState.Inventory);
                }
                break;
            // If no menu is open
            case -1:
                if (hudManager.State < 0) _state = MoveState.Normal;
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
                inter.stage++;
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

    // Update is called once per frame
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

    bool debugJump = false;
    private Vector2 MovementNormal(Vector2 vel, bool useInput = true)
    {
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

        // Handle Jump
        if (useInput && _jumpInput)
        {
            if (entity.IsGrounded && vel.y <= jumpHoldPower)
            {
                vel.y = jumpPower;
                jumpHold = jumpHoldTime;
                debugJump = true;
            }
            else if (jumpHold > 0f)
            {
                jumpHold -= Time.fixedDeltaTime;
                if (jumpHold < jumpHoldTime - jumpHoldBuffer)
                {
                    float ratio = jumpHold / jumpHoldTime;
                    vel.y += jumpHoldPower + jumpHoldPower * ratio;
                    if (debugJump)
                    {
                        //Debug.Log($"Held Jump!");
                        debugJump = false;
                    }
                }

                if (jumpHold <= 0)
                {
                    jumpHold = 0f;
                    _jumpInput = false;
                }
            }
        }
        else
        {
            jumpHold = 0f;
            debugJump = false;
        }

        // Handle Passthrough Platforms
        if (useInput)
        {
            if (entity.IsGrounded)
            {
                entity.IsPassThrough = _moveInput.y < 0;
            }
            // Passthrough resets in OnTriggerExit2D 
        }
        else
        {
            entity.IsPassThrough = false;
        }

        // Enable Gravity
        entity.IgnoreGravity = false;

        return vel;
    }

    private Vector2 MovementClimbing(Vector2 vel)
    {
        // Handle Climb Movement
        Vector2 raw = _moveInput.normalized * climbSpeed;
        float climbY = vel.y > raw.y && raw.y > 0 ? vel.y : raw.y; // High y-vel overrides up-input
        vel = new Vector2(raw.x, climbY);

        // Handle Climb Jump
        if (_jumpInput && vel.y < climbJump)
        {
            vel.y = climbJump;
            _jumpInput = false;
            _state = MoveState.Normal;
        }

        // Disable Gravity
        entity.IgnoreGravity = true;

        return vel;
    }
    #endregion

    #region OnHandleGravity

    private void OnHandleGravity()
    {
        Vector2 vel = entity.Vel;

        switch (_state)
        {
            case MoveState.Climbing:
                {
                    if (_moveInput.y == 0)
                    {
                        vel.y = -climbGravity;
                    } else
                    {
                        vel.y -= climbGravity;
                    }
                    break;
                }
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
            if (_moveInput.y > 0 && !_jumpInput)
            {
                _state = MoveState.Climbing;
            }
        }

    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Dialogue"))
        {
            Dialoguer dia = other.gameObject.GetComponent<Dialoguer>();

            if (dia != null)
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
                hudManager.HideTooltip();
            }
        }
    }
    #endregion

    public bool LockPlayer()
    {
        if (_state == MoveState.Locked) return false;
        _state = MoveState.Locked;
        return true;
    }

    //public void OnInteract(InputValue value)
    //{
    //    _interactInput = value.isPressed;
    //}

    //public void OnInventory(InputValue value)
    //{
    //    _inventoryInput = value.isPressed;
    //}
}

enum MoveState
{
    Normal,
    Climbing,
    Locked
}
