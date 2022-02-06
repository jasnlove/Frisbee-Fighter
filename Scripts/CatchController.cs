using UnityEngine;

public class CatchController : MonoBehaviour
{
    [SerializeField] private LayerMask discLayer;
    private PlayerController player;
    
    //float catchDelay = 0.7f;
    //float catchDelayTimer;
    //bool catchReady = false;

    // Start is called before the first frame update
    void Start()
    {
        player = gameObject.GetComponentInParent<PlayerController>();
        //catchDelayTimer = catchDelay;
    }

    // Update is called once per frame
    void Update()
    {
        //catchDelayTimer -= Time.deltaTime;
        //if (catchDelayTimer < 0)
        //catchReady = true;
    }

    //This could also likely just go into PlayerController if you don't care about separating behaviors
    //Wrote this because disabling collision to the player disallowed the player from grabbing it.
    //This just goes around collisions and checks another way
    


    /*void OnTriggerEnter2D(Collider2D other)
    {
        DiscController disc = other.GetComponent<DiscController>();

        if (disc != null && disc.pickupReady)
        {
            player.PickupDisc();
            //catchDelayTimer = catchDelay;
            //catchReady = false;
            Destroy(other.gameObject);
        }
    }*/
}
