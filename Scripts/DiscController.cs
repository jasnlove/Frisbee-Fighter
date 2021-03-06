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
    private float airTimeReq = 0.05f;
    private float airTimer;
    private bool airborne = false;
    private PlayerController player;
    private SpriteRenderer rend;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        rend = GetComponent<SpriteRenderer>();

        // Change player sprite to default
        ChangeSprite(0);
        airTimer = airTimeReq;
    }

    private void Update()
    {
        if (player.GetCharge() >= 100)
            rend.sprite = chargedDiscSprite;
        else
            rend.sprite = discSprite;

        if (velocity.magnitude < 0.05f && player.GetCharge() > 0)
        {
            rend.sprite = discSprite;
            hyperDisc = false;
            Charge(0);
        }

        if (!airborne)
        {
            airTimer -= Time.deltaTime;
            if (airTimer <= 0)
                airborne = true;
        }
    }

    public void Launch(Vector2 direction, float speed, int collisionLayer)
    {
        body.velocity = (direction * speed);
        StartCoroutine(TestLaunch(direction, collisionLayer));

        // Change sprite to thrown version.
        ChangeSprite(1);
    }

    //It also allows us to more easily set when a disc is ready to grab
    //Commented it out for easy understanding.
    public IEnumerator TestLaunch(Vector2 direction, int collisionLayer)
    {
        Vector2 launchDirection = direction;
        float radius = Mathf.Max(transform.localScale.x, transform.localScale.y);
        Vector2 lastBouncePos = Vector2.zero;
        while (body.velocity.magnitude > Mathf.Epsilon)
        {
            yield return new WaitForFixedUpdate();
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, radius * 0.95f, Vector2.zero, 0.0f, collisionLayer);
            if (hit && Vector3.SqrMagnitude(hit.point - lastBouncePos) >= 1) //If the disc hits something, reflect the direction
            {
                lastBouncePos = hit.point;
                Debug.DrawRay(transform.position, hit.normal, Color.red, 3);
                if (airborne) //To prevent spamming disc into wall at close range
                    Charge(34);
                body.velocity = Vector2.Reflect(body.velocity, hit.normal);
                pickupReady = true; //Player can only pick it up after it has hit a wall
            }
            body.AddForce(body.velocity * decelSpeed);
            decelSpeed -= 0.02f;
        }
        pickupReady = true; //Player can also pick it up after it stops moving
    }

    public void Charge(int amount)
    {
        player.ChangeCharge(amount);
    }

    public void ChangeSprite(int status)
    {
        player.spriteRenderer.sprite = player.spriteArray[status];
    }
    public Vector2 velocity => body.velocity;
}
