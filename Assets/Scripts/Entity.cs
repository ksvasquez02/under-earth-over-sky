using System;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class Entity : MonoBehaviour
{
    private Vector2 pos;
    [SerializeField]
    private Vector2 vel = Vector2.zero;

    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private float gravity = 0f;
    [SerializeField]
    private float maxGravity = 10f;
    [SerializeField]
    private float groundOffset = 0.05f;
    [SerializeField]
    private float groundGravity = 1f;

    [SerializeField]
    private bool isGrounded = false;
    private bool facing = true;

    // Collision
    private Rigidbody2D _body;
    private BoxCollider2D _col;

    public event Action HandleMovement;

    public Vector2 Pos { get { return pos; } set { pos = value; } }
    public Vector2 Vel { get { return vel; } set { vel = value; } }
    public Vector2 NetVel { get { return new Vector2(_body.linearVelocityX, _body.linearVelocityY); } }
    public Rigidbody2D Body { get { return _body; } }
    public Bounds Bounds { get { return _col.bounds; } }
    public float Gravity { get { return gravity; } set { gravity = value; } }
    public bool IsGrounded { get { return isGrounded; } }
    public bool Facing { get { return facing; } set { facing = value; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pos = transform.position;
        _body = GetComponent<Rigidbody2D>();
        _col = GetComponent<BoxCollider2D>();
        Physics2D.queriesStartInColliders = false;
    }

    private void Update()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        pos = transform.position;
        
        CheckCollisions();
        HandleMovement?.Invoke();
        HandleGravity();

        _body.linearVelocity = vel;
    }

    private void CheckCollisions()
    {
        // Raycasts to detect collisions
        bool collidesGround = Physics2D.BoxCast(_col.bounds.center, _col.size, 0, Vector2.down, groundOffset, groundLayer);
        bool collidesCeiling = Physics2D.BoxCast(_col.bounds.center, _col.size, 0, Vector2.up, groundOffset, groundLayer);

        // Hit a Ceiling
        if (collidesCeiling)
        {
            vel.y = Mathf.Min(0, vel.y);
        }

        // Landed on the Ground
        if (!isGrounded && collidesGround)
        {
            isGrounded = true;
        }
        // Left the Ground
        else if (isGrounded && !collidesGround)
        {
            isGrounded = false;
        }

    }

    private void HandleGravity()
    {
        // On Ground
        if (isGrounded && vel.y <= 0f)
        {
            vel.y = -groundGravity;
        }
        // In Air
        else
        {
            Vector2 airGrav = gravity * Time.fixedDeltaTime * Vector2.down;
            vel += airGrav;
            vel.y = Mathf.Max(vel.y, maxGravity * -1);
        }
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.green;
        //Collider2D col = GetComponent<Collider2D>();
        //Vector2 p = col.bounds.center;
        //Vector2 off = Vector2.down * groundOffset;
        //Gizmos.DrawWireCube(p + off, col.bounds.size);
    }
}
