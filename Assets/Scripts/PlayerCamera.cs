using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private Entity player;

    [SerializeField]
    private Vector2 focusAreaSize;
    private FocusArea focusArea;

    [SerializeField]
    private Vector2 offset = Vector2.up;

    [SerializeField]
    private float lookAheadDist = 1f;
    [SerializeField]
    private Vector2 smoothTime;

    public Vector2 vel;
    private float currentLookAhead;
    private float targetLookAhead;
    private float lookAheadDir;

    struct FocusArea
    {
        float left;
        float right;
        float bottom;
        float top;
        public Vector2 center;
        public Vector2 vel;

        public FocusArea(Bounds targetBounds, Vector2 size)
        {
            left = targetBounds.center.x - size.x / 2;
            right = targetBounds.center.x + size.x / 2;
            bottom = targetBounds.min.y;
            top = targetBounds.min.y + size.y;

            center = new Vector2((left + right) / 2, (top + bottom) / 2);
            vel = Vector2.zero;
        }
        public void Update(Bounds targetBounds)
        {
            vel = Vector2.zero;

            if (targetBounds.min.x < left)
                vel.x = targetBounds.min.x - left;
            else if (targetBounds.max.x > right)
                vel.x = targetBounds.max.x - right;
            if (targetBounds.min.y < bottom)
                vel.y = targetBounds.min.y - bottom;
            else if (targetBounds.max.y > top)
                vel.y = targetBounds.max.y - top;

            left += vel.x;
            right += vel.x;
            bottom += vel.y;
            top += vel.y;
            center = new Vector2((left + right) / 2, (top + bottom) / 2);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (player == null) player = GameObject.FindWithTag("Player").GetComponent<Entity>();
        focusArea = new FocusArea(player.Bounds, focusAreaSize);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Bounds playerBounds = new Bounds((Vector2)player.transform.position + player.Box.offset, player.Box.size);
        focusArea.Update(playerBounds);

        Vector2 focusPos = focusArea.center + offset;
        Vector2 pos = focusPos;

        if (focusArea.vel.x != 0)
        {
            lookAheadDir = Mathf.Sign(focusArea.vel.x);
            targetLookAhead = lookAheadDir * lookAheadDist;
        }
        else if (lookAheadDir != 0)
        {
            targetLookAhead = currentLookAhead + (lookAheadDir * lookAheadDist - currentLookAhead) / 4f;
            lookAheadDir = 0;
        }

        currentLookAhead = Mathf.SmoothDamp(currentLookAhead, targetLookAhead, ref vel.x, smoothTime.x);
        pos.x += currentLookAhead;

        pos.y = Mathf.SmoothDamp(transform.position.y, pos.y, ref vel.y, smoothTime.y);

        if (vel.magnitude < 0.001f) vel = Vector2.zero;
        
        transform.position = new Vector3(pos.x, pos.y, transform.position.z);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f,0,1,0.5f);
        Gizmos.DrawCube(focusArea.center, focusAreaSize);
    }
}
