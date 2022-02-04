using UnityEngine;

public class DiscController : MonoBehaviour
{
    Rigidbody2D body;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {

    }

    public void Launch(Vector2 direction, float speed)
    {
        body.velocity = (direction * speed);
    }
}
