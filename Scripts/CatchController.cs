using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchController : MonoBehaviour
{
    PlayerController player;
    float catchDelay = 0.7f;
    float catchDelayTimer;
    bool catchReady = false;

    // Start is called before the first frame update
    void Start()
    {
        player = gameObject.GetComponentInParent<PlayerController>();
        catchDelayTimer = catchDelay;
    }

    // Update is called once per frame
    void Update()
    {
        catchDelayTimer -= Time.deltaTime;
        if (catchDelayTimer < 0)
            catchReady = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        DiscController disc = other.GetComponent<DiscController>();

        if (disc != null && catchReady)
        {
            player.PickupDisc();
            catchDelayTimer = catchDelay;
            catchReady = false;
            Destroy(other.gameObject);
        }
    }
}
