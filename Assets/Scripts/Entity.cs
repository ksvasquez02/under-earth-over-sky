using UnityEngine;

public class Entity : MonoBehaviour
{
    private Vector2 pos;
    private Vector2 vel = Vector2.zero;

    [SerializeField]
    private float currentGravity = 0f;
    [SerializeField]
    private float gravity = 0f;
    [SerializeField]
    private const float maxGravity = 10f;

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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pos = transform.position;
        boundingBox = GetComponent<Collider2D>();
        level = level ? level : GameObject.FindGameObjectWithTag("Level").GetComponent<Level>();
    }

    // Update is called once per frame
    void Update()
    {
        pos = transform.position;

        if (!isGrounded)
        {
            currentGravity = Mathf.Min(currentGravity + gravity * Time.deltaTime, maxGravity * Time.deltaTime);
            Vector2 grav = currentGravity * Vector2.down;
            vel += grav;
        } else
        {
            currentGravity = 0;
        }

        Vector2 Collision = level.CheckCollision(Bounds, vel);

        if (Collision.x > 0)
        {
            vel.x = 0;
        }
        if (Collision.y > 0)
        {
            if (vel.y < 0)
            {
                isGrounded = true;
            }
            vel.y = 0;
        }
        else if (vel.y != 0)
        {
            isGrounded = false; // Lot of issues but fix later
        }

        pos += vel;

        transform.position = pos;
    }
}
