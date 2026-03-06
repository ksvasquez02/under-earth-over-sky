using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Entity entity;

    private InputAction ia_move;
    private InputAction ia_jump;

    [SerializeField]
    private float accel = 10f;
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float deaccel = 20f;
    [SerializeField]
    private float jumpPower = 10f;

    private Vector2 _moveInput;
    private bool _jumpInput;

    [SerializeField]
    private float curSpeed = 0f;


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

        entity.HandleMovement += MovementUpdate;
    }

    private void Update()
    {
        _moveInput = ia_move.ReadValue<Vector2>();
        _jumpInput = ia_jump.IsPressed();

    }

    // Update is called once per frame
    private void MovementUpdate()
    {
        Vector2 vel = entity.Vel;
        float momentum = System.Math.Sign(vel.x); // Mathf.Sign is stupid.
        if (_moveInput.magnitude > 0.1)
        {
            curSpeed += accel * Time.fixedDeltaTime;
            curSpeed = Mathf.Min(curSpeed, speed);
            vel.x = _moveInput.x * curSpeed;
        }
        else
        {
            curSpeed -= deaccel * Time.fixedDeltaTime;
            curSpeed = Mathf.Max(curSpeed, 0f);
            vel.x = momentum * curSpeed;
        }

        if (entity.IsGrounded && _jumpInput)
        {
            vel.y += _jumpInput ? jumpPower : 0f;
            _jumpInput = false;
        }
        entity.Vel = vel;
    }
}
