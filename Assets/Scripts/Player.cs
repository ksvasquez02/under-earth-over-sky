using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Timeline;
using UnityEngine.U2D.IK;

public class Player : MonoBehaviour
{
    private Entity entity;

    private InputAction ia_move;
    private InputAction ia_jump;

    private Vector2 _moveInput;
    private bool _jumpInput;

    private MoveState _state;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        entity = GetComponent<Entity>();
        if (entity == null)
        {
            throw new System.Exception("No entity found on Player!");
        }

        ia_move = InputSystem.actions.FindAction("Move");
        ia_jump = InputSystem.actions.FindAction("Jump");

        entity.HandleMovement += HandleMovement;
        entity.OnHandleGravity += OnHandleGravity;
    }

    private void Update()
    {
        _moveInput = ia_move.ReadValue<Vector2>();
        _jumpInput = ia_jump.IsPressed();

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
        }

        entity.Vel = vel;
    }

    private Vector2 MovementNormal(Vector2 vel)
    {
        // Accelerate when active
        if (Mathf.Abs(_moveInput.x) >= 0.1f)
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
        if (entity.IsGrounded && _jumpInput)
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
            case MoveState.Normal: break;
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
        }

        entity.Vel = vel;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (_state != MoveState.Climbing && other.gameObject.CompareTag("Climb"))
        {
            if (_moveInput.y > 0 && !_jumpInput)
            {
                _state = MoveState.Climbing;
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (_state == MoveState.Climbing && other.gameObject.CompareTag("Climb"))
        {
            _state = MoveState.Normal;
        }
    }
}

enum MoveState
{
    Normal,
    Climbing
}
