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
    [SerializeField]
    private float jumpPower = 10f;
    [SerializeField]
    private float curSpeed = 0f;

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

    private void Update()
    {
        _moveInput = ia_move.ReadValue<Vector2>();
        _jumpInput = ia_jump.IsPressed();
        _interactInput = ia_interact.WasPressedThisFrame();
        _inventoryInput = ia_inventory.WasPressedThisFrame();

        switch (_state)
        {
            case MoveState.Locked:
                if (_interactInput && hudManager.State == (int)MenuState.Lore)
                {
                    hudManager.HideMenu(MenuState.Lore);
                    _state = MoveState.Normal;
                }
                if (_inventoryInput && hudManager.State == (int)MenuState.Inventory)
                {
                    hudManager.HideMenu(MenuState.Inventory);
                    _state = MoveState.Normal;
                }
                break;
            default:
                if (_interactInput && nearbyInteractables.Count > 0)
                {
                    Interactable interactable = nearbyInteractables[0];
                    ItemData itemData = interactable.itemData;
                    hudManager.PopulateLore(itemData);
                    if (inventory.AddItem(itemData))
                    {
                        hudManager.PopulateInventory(inventory);
                    }
                } else if (_inventoryInput)
                {
                    hudManager.ShowMenu(MenuState.Inventory);
                }
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

    private Vector2 MovementNormal(Vector2 vel, bool useInput = true)
    {
        // Accelerate when active
        if (useInput && Mathf.Abs(_moveInput.x) >= 0.1f)
        {
            float delta = accel * Time.fixedDeltaTime;
            curSpeed += delta * _moveInput.x;
        }
        // Deaccelerate when inactive
        else
        {
            float delta = deaccel * Time.fixedDeltaTime;
            curSpeed = Mathf.MoveTowards(curSpeed, 0f, delta);
        }

        // Limit and apply horizontal velocity
        curSpeed = Mathf.Clamp(curSpeed, -speed, speed);
        vel.x = curSpeed;

        // Handle Jump
        if (useInput && entity.IsGrounded && _jumpInput)
        {
            vel.y = jumpPower;
            _jumpInput = false;
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
        if (_state == MoveState.Climbing && other.gameObject.CompareTag("Climb"))
        {
            _state = MoveState.Normal;
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
