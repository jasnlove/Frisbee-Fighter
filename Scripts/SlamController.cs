using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlamController : MonoBehaviour
{
    [SerializeField] private float damageTimer = 0.1f;

    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        timer = damageTimer;
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;

        if (timer < 0)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        Enemy enemy = collider.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.Explode();
            Destroy(collider.gameObject);
        }
    }
}
