using UnityEngine;

enum Edge
{
    left,
    right,
    bottom,
    top,
}

public struct CollisionData
{
    public Vector2 collisions;
    public float left;
    public float right;
    public float bottom;
    public float top;
}

public class Level : MonoBehaviour
{
    private Platform[] platforms;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        platforms = GetComponentsInChildren<Platform>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public CollisionData CheckCollision(Bounds entity, Vector2 vel)
    {
        CollisionData data = new CollisionData();
        data.collisions = Vector2.zero;
        data.left = Mathf.NegativeInfinity;
        data.bottom = Mathf.NegativeInfinity;
        data.right = Mathf.Infinity;
        data.top = Mathf.Infinity;

        Bounds predicted = new(entity.center + (Vector3)(vel * Time.deltaTime), entity.size);
        Bounds predictedX = new(entity.center + new Vector3(vel.x * Time.deltaTime, 0, 0), entity.size);
        Bounds predictedY = new(entity.center + new Vector3(0, vel.y * Time.deltaTime, 0), entity.size);
        Vector2 dir = new(System.Math.Sign(vel.x), System.Math.Sign(vel.y));
        foreach (Platform plat in platforms)
        {
            //Bounds platBounds = new(plat.Bounds.center, new Vector2(plat.Bounds.size.x - Mathf.Epsilon * 2f, plat.Bounds.size.y - Mathf.Epsilon * 2f));
            Bounds platBounds = plat.Bounds;

            if (IntersectsStrict(predicted, platBounds))
            {
                if (dir.x != 0 && IntersectsEdge(predictedX, platBounds, dir.x > 0 ? Edge.right : Edge.left))
                {
                    data.collisions.x++;
                    if (dir.x > 0)
                    {
                        data.right = Mathf.Min(platBounds.min.x, data.right);
                    }
                    else
                    {
                        data.left = Mathf.Max(platBounds.max.x, data.left);
                    }
                }
                if (dir.y != 0 && IntersectsEdge(predictedY, platBounds, dir.y > 0 ? Edge.top : Edge.bottom))
                {
                    data.collisions.y++;
                    if (dir.y > 0)
                    {
                        data.top = Mathf.Min(platBounds.min.y, data.top);
                    }
                    else
                    {
                        data.bottom = Mathf.Max(platBounds.max.y, data.bottom);
                    }
                }
            }
        }
        return data;
    }

    private bool IntersectsStrict(Bounds a, Bounds b)
    {
        return  a.min.x < b.max.x &&
                a.max.x > b.min.x &&
                a.min.y < b.max.y &&
                a.max.y > b.min.y;
    }
    private bool IntersectsEdge(Bounds a, Bounds b, Edge edge)
    {
        if (!IntersectsStrict(a, b)) return false;

        bool left = b.min.x <= a.min.x && a.min.x <= b.max.x;
        bool right = b.min.x <= a.max.x && a.max.x <= b.max.x;
        bool bottom = b.min.y <= a.min.y && a.min.y <= b.max.y;
        bool top = b.min.y <= a.max.y && a.max.y <= b.max.y;

        switch (edge)
        {
            case Edge.left:
                return left && (bottom || top) && a.max.x >= b.max.x;
            case Edge.right:
                return right && (bottom || top) && a.min.x <= b.min.x;
            case Edge.bottom:
                return bottom && (left || right) && a.max.y >= b.max.y;
            case Edge.top:
                return top && (left || right) && a.min.y <= b.min.y;
            default:
                return false;
        }
    }
}
