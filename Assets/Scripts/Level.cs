using UnityEngine;

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

    public Vector2 CheckCollision(Bounds entity, Vector2 vel)
    {
        Vector2 collisions = Vector2.zero;
        Bounds predictedX = new(entity.center + new Vector3(vel.x, 0, 0), entity.size);
        Bounds predictedY = new(entity.center + new Vector3(0, vel.y, 0), entity.size);
        foreach (Platform plat in platforms)
        {
            Bounds platBounds = new(plat.Bounds.center, new Vector2(plat.Bounds.size.x - Mathf.Epsilon * 4f, plat.Bounds.size.y - Mathf.Epsilon * 4f));

            if (predictedX.Intersects(platBounds))
            {
                collisions.x++;
            }
            if (predictedY.Intersects(platBounds))
            {
                collisions.y++;
            }
        }
        return collisions;
    }
}
