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
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 move = ia_move.ReadValue<Vector2>();
        float jump = ia_jump.IsPressed() ? 1f : 0f;

        Vector2 dir = new Vector2(move.x, 0f);
        entity.Vel = speed * Time.deltaTime * Vector2.ClampMagnitude(dir, 1f);

        entity.Vel += new Vector2(0f, jump * jumpPower);
    }
}
