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
    private float delayAccel = 1f;
    [SerializeField]
    private float maxDelay = 1f;

    private Vector2 velOffset = Vector3.zero;
    private Vector3 target;
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

        Vector3 basePos = new Vector3(player.Pos.x, player.Pos.y, pos.z) + (Vector3) offset;
        Vector2 delta = maxDelay * Vector2.ClampMagnitude(new Vector2(player.NetVel.x, player.NetVel.y * 0.5f), 1);
        target = basePos + (Vector3) delta;

        speed += delayAccel * Time.deltaTime;
        speed = Mathf.Min(speed, 20f);
        if (delta.magnitude < 0.1f) speed = 0;

        //pos = Vector3.MoveTowards(pos, basePos, panSpeed * Time.deltaTime);
        //transform.position = Vector3.MoveTowards(pos, target, speed * Time.deltaTime);
        transform.position = basePos;
    }

    private void OnDrawGizmos()
    {
        Vector2 pos = transform.position;
        Vector2 tar = target;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pos, tar);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(tar, 0.1f);
    }
}
