using System;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class Entity : MonoBehaviour
{
    private Vector2 pos;
    [SerializeField]
    private Vector2 vel = Vector2.zero;

    [SerializeField]
    private float gravity = 0f;
    [SerializeField]
    private float maxGravity = 10f;
    [SerializeField]
    private float groundOffset = 0.05f;
    [SerializeField]
    private float maxWallAngle = 60f;
    [SerializeField]
    private float wallOffset = 0.01f;
    [SerializeField]
    private float groundGravity = 1f;

    [SerializeField]
    private bool isGrounded = false;
    private bool facing = true;

    // Collision
    private Rigidbody2D body;
    private Collider2D bounds;

    public event Action HandleMovement;

    [SerializeField]
    private Level level;

    public Vector2 Pos { get { return pos; } set { pos = value; } }
    public Vector2 Vel { get { return vel; } set { vel = value; } }
    public Vector2 NetVel { get { return new Vector2(body.linearVelocityX, body.linearVelocityY); } }
    public Bounds Bounds { get { return bounds.bounds; } }
    public float Gravity { get { return gravity; } set { gravity = value; } }
    public bool IsGrounded { get { return isGrounded; } }
    public bool Facing { get { return facing; } set { facing = value; } }
    private float MaxWallAngle {
        set
        {
            float angle = Mathf.Clamp(value, 0, 90);
            float a = groundOffset * 0.9f;
            float c = a / Mathf.Sin(Mathf.Deg2Rad * angle);
            float b = Mathf.Sqrt(c * c - a * a);
            wallOffset = b;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pos = transform.position;
        body = GetComponent<Rigidbody2D>();
        bounds = GetComponent<Collider2D>();
        MaxWallAngle = maxWallAngle;
        level = level ? level : GameObject.FindGameObjectWithTag("Level").GetComponent<Level>();
        Physics2D.queriesStartInColliders = false;
    }

    private void Update()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        pos = transform.position;
        //vel = vel / 2 + body.linearVelocity / 2;
        

        CheckCollision();
        HandleMovement?.Invoke();
        HandleGravity();

        body.linearVelocity = vel;
        //pos += vel * Time.fixedDeltaTime;
        //transform.position = pos;
    }

    private void CheckCollision()
    {
        Vector2 bCenter = pos + bounds.offset;
        Vector2 bOffset = new Vector2(bounds.bounds.size.x / 2 - wallOffset, 0);
        float length = bounds.bounds.size.y / 2 + groundOffset;
        bool collidesGround = Physics2D.Raycast(bCenter, Vector2.down, length)
            || Physics2D.Raycast(bCenter + bOffset, Vector2.down, length)
            || Physics2D.Raycast(bCenter - bOffset, Vector2.down, length);

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
        if (isGrounded)
        {
            //if (vel.y < 0) vel.y = groundGravity * -1;
            vel.y = Mathf.Max(vel.y - groundGravity, groundGravity * -1);
        }
        else
        {
            Vector2 airGrav = gravity * Time.fixedDeltaTime * Vector2.down;
            vel += airGrav;
            vel.y = Mathf.Max(vel.y, maxGravity * -1);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("HELP!");
    }

    private void OnDrawGizmos()
    {
        Vector2 p = transform.position;
        Collider2D bb = GetComponent<Collider2D>();
        Vector2 offset = new Vector2(bb.bounds.size.x / 2 - wallOffset, 0);
        float length = bb.bounds.size.y / 2 + groundOffset;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(p, (p + bb.offset) + Vector2.down * length);
        Gizmos.DrawLine(p + offset, (p + bb.offset + offset) + Vector2.down * length);
        Gizmos.DrawLine(p - offset, (p + bb.offset - offset) + Vector2.down * length);
    }
}
