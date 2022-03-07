using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class DiscController : MonoBehaviour
{
    [SerializeField] Sprite discSprite;
    [SerializeField] Sprite chargedDiscSprite;

    public bool hyperDisc;
    public bool pickupReady = false;
    private Rigidbody2D body;
    private float decelSpeed = -0.8f;
    private PlayerController player;
    private SpriteRenderer rend;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        rend = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (player.GetCharge() >= 100)
            rend.sprite = chargedDiscSprite;
        else
            rend.sprite = discSprite;
    }

    public void Launch(Vector2 direction, float speed, int collisionLayer)
    {
        body.velocity = (direction * speed);
        StartCoroutine(TestLaunch(direction, collisionLayer));
    }

    //It also allows us to more easily set when a disc is ready to grab
    //Commented it out for easy understanding.
    public IEnumerator TestLaunch(Vector2 direction, int collisionLayer)
    {
        Vector2 launchDirection = direction;
        float radius = Mathf.Max(transform.localScale.x, transform.localScale.y);
        int iterations = 0;
        int maxIterations = 180;
        while (body.velocity.magnitude > Mathf.Epsilon && iterations < maxIterations)
        {
            yield return new WaitForFixedUpdate();
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, radius * 1.05f, Vector2.zero, 0.0f, collisionLayer);//Checks a circle direction on the player for collisions
            if (hit) //If the disc hits something, reflect the direction
            {
                Charge(34);
                body.velocity = Vector3.Reflect(body.velocity, hit.normal);
                pickupReady = true; //Player can only pick it up after it has hit a wall
            }
            body.AddForce(body.velocity * decelSpeed);
            decelSpeed -= 0.02f;
            iterations++;
        }
        pickupReady = true; //Player can also pick it up after it stops moving
    }

    public void Charge(int amount)
    {
        player.ChangeCharge(amount);
    }
}
