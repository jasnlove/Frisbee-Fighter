using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class DiscController : MonoBehaviour
{
    public bool pickupReady = false;
    private Rigidbody2D body;
    //private float decelSpeed = -0.8f;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        //body.AddForce(body.velocity * decelSpeed);
        //decelSpeed -= 0.01f;
    }

    public void Launch(Vector2 direction, float speed, float distance, int collisionLayer)
    {
        //body.velocity = (direction * speed);
        StartCoroutine(TestLaunch(direction, distance, speed, collisionLayer));
    }

    //Ran into issues using OnCollisionEnter with clipping and whatnot. Ended up writing this
    //It also allows us to more easily set when a disc is ready to grab, rather than just using an abritrary timer.
    //Commented it out for easy understanding.
    public IEnumerator TestLaunch(Vector2 direction, float distance, float speed, int collisionLayer)
    {
        float distanceTossed = 0;
        Vector2 launchDirection = direction;
        int iterations = 0;
        int maxIterations = 50;
        float radius = Mathf.Max(transform.localScale.x, transform.localScale.y);
        while (distanceTossed < distance && iterations < maxIterations)
        {
            yield return new WaitForFixedUpdate();
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, radius * 1.05f, launchDirection, 0.0f, collisionLayer);//Checks a circle direction on the player for collisions
            if (hit) //If the disc hits something, reflect the direction
            {
                distanceTossed += speed * Time.fixedDeltaTime;
                launchDirection = Vector3.Reflect(launchDirection, hit.normal);
                body.MovePosition(body.position + launchDirection * speed * Time.fixedDeltaTime); //Creates slight separation from where it hits, to ensure no clipping happens
                pickupReady = true; //Player can only pick it up after it has hit a wall
            }
            else //Otherwise just move it forward
            {
                body.MovePosition(body.position + launchDirection * speed * Time.fixedDeltaTime);
                distanceTossed += speed * Time.fixedDeltaTime;
            }
        }
        pickupReady = true; //Player can also pick it up after it stops moving
    }
}
