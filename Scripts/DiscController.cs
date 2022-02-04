using UnityEngine;

public class DiscController : MonoBehaviour
{
    Rigidbody2D body;
    float decelSpeed = -0.8f;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        body.AddForce(body.velocity * decelSpeed);
        decelSpeed -= 0.01f;
    }

    public void Launch(Vector2 direction, float speed)
    {
        body.velocity = (direction * speed);
    }
}
