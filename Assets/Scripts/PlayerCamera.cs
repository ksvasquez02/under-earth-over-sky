using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField]
    private Entity player;
    [SerializeField]
    private Vector2 offset = Vector2.up;

    [SerializeField]
    private float panSpeed = 8f;
    [SerializeField]
    private float idleSpeed = 4f;
    [SerializeField]
    private float delayAccel = 1f;
    [SerializeField]
    private float maxDelay = 1f;

    private Vector2 velOffset = Vector3.zero;
    private Vector2 target;
    private Vector2 basePos;
    [SerializeField]
    private float speed = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (player == null) player = GameObject.FindWithTag("Player").GetComponent<Entity>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;
        float minSpeed = idleSpeed;
        float maxSpeed = panSpeed;
        float accel = delayAccel;

        basePos = new Vector2(player.Body.position.x, player.Body.position.y) + offset;

        Vector2 playerVel = new Vector2(player.NetVel.x, player.NetVel.y * 0.5f);
        Vector2 velDelta = playerVel.magnitude > 0.1f ? Vector2.ClampMagnitude(playerVel, maxDelay) : Vector2.zero;
        target = basePos + velDelta;

        float distance = Vector2.Distance(target, pos);
        if (distance > maxDelay)
        {
            float distanceOffset = distance * 2 - maxDelay;
            speed += distanceOffset;
            maxSpeed += distanceOffset;
        }
        else if (distance < 0.1f)
        {
            speed = minSpeed;
            accel = 0;
        }
        else if (velDelta.magnitude >= 0.1f)
        {
            accel = -delayAccel;
        }

        speed += accel * Time.deltaTime;
        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);

        Vector2 towards = Vector2.MoveTowards(pos, target, speed * Time.deltaTime);
        pos = new Vector3(towards.x, towards.y, pos.z);

        transform.position = pos; 
    }

    private void OnDrawGizmos()
    {
        Vector2 pos = transform.position;
        Vector2 tar = target;
        Vector2 bas = basePos;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(tar, 0.1f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pos, tar);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(bas, 0.1f);
    }
}
