using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private Entity player;
    [SerializeField]
    private Vector2 offset = Vector2.up;

    [SerializeField]
    private float panSpeed = 8f;
    [SerializeField]
    private float minPanSpeed = 2f;
    [SerializeField]
    private float followAccel = 1f;
    [SerializeField]
    private float followDist = 1f;
    [SerializeField]
    private float followVertDamping = 0.5f;
    [SerializeField]
    private float followDelaySpeed = 8f;

    private Vector2 velOffset = Vector3.zero;
    private Vector2 targetPos;
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
        float minSpeed = minPanSpeed;
        float maxSpeed = panSpeed;
        float accel = followAccel;

        basePos = new Vector2(player.Body.position.x, player.Body.position.y) + offset;

        Vector2 playerVel = new Vector2(player.NetVel.x, player.NetVel.y * followVertDamping);
        Vector2 velDeltaTarget = playerVel.magnitude > 0.1f ? Vector2.ClampMagnitude(playerVel, followDist) : Vector2.zero;
        velOffset = Vector2.MoveTowards(velOffset, velDeltaTarget, followDelaySpeed * Time.deltaTime);

        targetPos = basePos + velOffset;

        float distance = Vector2.Distance(targetPos, pos);
        if (distance > followDist)
        {
            float distanceOffset = distance * 2 - followDist;
            speed += distanceOffset;
            maxSpeed += distanceOffset;
        }
        else if (distance < 0.1f)
        {
            speed = minSpeed;
            accel = 0;
        }
        // If player is not moving, and camera has finished compensating for velocity offset
        else if (velDeltaTarget.magnitude <= 0.1f && velOffset.magnitude <= 0.1f)
        {
            accel = -followAccel;
        }

        speed += accel * Time.deltaTime;
        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);

        Vector2 towards = Vector2.MoveTowards(pos, targetPos, speed * Time.deltaTime);
        pos = new Vector3(towards.x, towards.y, pos.z);

        transform.position = pos; 
    }

    private void OnDrawGizmos()
    {
        Vector2 pos = transform.position;
        Vector2 tar = targetPos;
        Vector2 bas = basePos;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(tar, 0.1f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pos, tar);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(bas, 0.1f);
    }
}
