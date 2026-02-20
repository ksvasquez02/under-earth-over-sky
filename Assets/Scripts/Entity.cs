using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

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
            currentGravity = Mathf.Min(currentGravity + gravity, maxGravity);
            Vector2 grav = currentGravity * Vector2.down;
            vel += grav;
        } else
        {
            currentGravity = 0;
        }

        CollisionData cd = level.CheckCollision(Bounds, vel);
        Vector2 boundOffset = (Vector2)Bounds.extents - boundingBox.offset;

        if (cd.collisions.x > 0)
        {
            if (vel.x > 0)
            {
                // Going too far right
                pos.x = cd.right - boundOffset.x;
                //Debug.Log($"Pos Right:  {transform.position.x} => {pos.x} | Edge: {cd.right}");
            }
            else
            {
                // Going too far left
                pos.x = cd.left + boundOffset.x;
                //Debug.Log($"Pos Left:   {transform.position.x} => {pos.x} | Edge: {cd.left}");
            }
            vel.x = 0;
        }
        if (cd.collisions.y > 0)
        {
            if (vel.y > 0)
            {
                // Going too far up
                pos.y = cd.top - boundOffset.y;
                //Debug.Log($"Pos Top:    {transform.position.y} => {pos.y} (Edge: {cd.top} Offset: {boundOffset.y})");
            }
            else
            {
                // Going too far down
                pos.y = cd.bottom + boundOffset.y;
                //Debug.Log($"Pos Bottom: {transform.position.y} => {pos.y} (Edge: {cd.bottom} Offset: {boundOffset.y})");
                isGrounded = true;
            }
            vel.y = 0;
        }
        else if (vel.y != 0)
        {
            isGrounded = false; // Lot of issues but fix later
        }

        pos += vel * Time.deltaTime;

        transform.position = pos;
    }
}
