using UnityEngine;

public class Platform : MonoBehaviour
{
    private Collider2D boundingBox;

    public Bounds Bounds { get { return boundingBox.bounds; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boundingBox = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
