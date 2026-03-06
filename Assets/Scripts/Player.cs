using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Entity entity;

    private InputAction ia_move;
    private InputAction ia_jump;

    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float jumpPower = 10f;

    [SerializeField]
    private Vector2 _moveInput;
    [SerializeField]
    private bool _jumpInput;


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
        entity.Vel = new Vector2(speed * _moveInput.x, entity.Vel.y);

        if (entity.IsGrounded && _jumpInput)
        {
            entity.Vel += new Vector2(0f, _jumpInput ? jumpPower : 0f);
            //Debug.Log($"Jumping: {entity.Vel}");
            _jumpInput = false;
        }
    }
}
