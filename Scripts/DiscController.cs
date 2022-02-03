using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscController : MonoBehaviour
{
    Rigidbody2D body;

    // Start is called before the first frame update
    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Launch(Vector2 direction, float speed)
    {
        body.velocity = (direction * speed);
    }
}
