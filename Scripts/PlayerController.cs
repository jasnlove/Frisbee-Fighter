using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 3.0f;
    public float launchSpeed = 6.0f;
    public GameObject discPrefab;

    Rigidbody2D body;
    float horizontal;
    float vertical;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Throw disc");
            Vector2 launchDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            launchDirection.Normalize();
            GameObject discObject = Instantiate(discPrefab, body.position, Quaternion.identity);
            DiscController disc = discObject.GetComponent<DiscController>();
            disc.Launch(launchDirection, launchSpeed);
        }
    }

    void FixedUpdate()
    {
        //Vector2 position = transform.position;
        Vector2 move = new Vector2(horizontal, vertical);
        move = Vector2.ClampMagnitude(move, 1f);
        body.MovePosition(body.position + speed * move * Time.deltaTime);
    }
}
