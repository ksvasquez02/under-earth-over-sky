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
    private bool isGrounded = false;
    private bool facing = true;

    [SerializeField]
    private Level level;
    private Collider2D boundingBox;

    public Vector2 Pos { get { return pos; } set { pos = value; } }
    public Vector2 Vel { get { return vel; } set { vel = value; } }
    public Bounds Bounds { get { return boundingBox.bounds; } }
    public float Gravity { get { return gravity; } set { gravity = value; } }
    public bool IsGrounded { get { return isGrounded; } }
    public bool Facing { get { return facing; } set { facing = value; } }

    private Rigidbody2D body;

    public event Action HandleMovement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pos = transform.position;
        boundingBox = GetComponent<Collider2D>();
        body = GetComponent<Rigidbody2D>();
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

        CheckCollision();
        HandleMovement?.Invoke();
        HandleGravity();

        body.linearVelocity = vel;

        //pos += vel * Time.fixedDeltaTime;
        //transform.position = pos;
    }

    private void CheckCollision()
    {
        Vector2 offset = new Vector2(boundingBox.bounds.size.x / 2, 0);
        float length = boundingBox.bounds.size.y / 2 + 0.05f;
        bool collidesGround = Physics2D.Raycast(pos + boundingBox.offset + offset, Vector2.down, length) || Physics2D.Raycast(pos + boundingBox.offset - offset, Vector2.down, length);

        // Landed on the Ground
        if (!isGrounded && collidesGround)
        {
            isGrounded = true;
            //Debug.Log("You should ground yourself, now!");
        }
        // Left the Ground
        else if (isGrounded && !collidesGround)
        {
            isGrounded = false;
            //Debug.Log("Unground again!");
        }

    }

    private void HandleGravity()
    {
        if (isGrounded)
        {
            if (vel.y < 0) vel.y = -1.0f;
        }
        else
        {
            Vector2 airGrav = gravity * Time.deltaTime * Vector2.down;
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
        Vector2 offset = new Vector2(bb.bounds.size.x / 2, 0);
        float length = bb.bounds.size.y / 2 + 0.05f;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(p + offset, (p + bb.offset + offset) + Vector2.down * length);
        Gizmos.DrawLine(p - offset, (p + bb.offset - offset) + Vector2.down * length);
    }
}
