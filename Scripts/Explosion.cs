using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private AudioClip[] explosionAudio;
    private AudioSource audioSource;

    float explosionTimer = 1.0f;

    // Start is called before the first frame update
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        int index = Random.Range(0, 4);
        audioSource.PlayOneShot(explosionAudio[index]);
    }

    // Update is called once per frame
    void Update()
    {
        if (explosionTimer <= 0)
            Destroy(gameObject);
        explosionTimer -= Time.deltaTime;
    }
}
