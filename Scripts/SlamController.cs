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
        Overlap();
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

    private void Overlap(){
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, transform.localScale.x / 2, LayerMask.GetMask("Enemy"));
        foreach(Collider2D col in cols){
            Enemy enemy = col.GetComponent<Enemy>();
            enemy.Explode();
            Destroy(col.gameObject);
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
