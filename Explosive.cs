using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Explosive : MonoBehaviour
{
    public Collider[] hitColliders;
    public LayerMask explosionLayers;
    public int currentHealth = 10;
    public float radius = 5.0f;
    public float power = 10.0f;
    public AudioSource explosionSound;
    
    bool isExploded;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
    public void TakeDamage(int amount)
    {
        if (isExploded)
            return;
        currentHealth -= amount;

        //hitParticles.transform.position = hitPoint;
        //hitParticles.Stop();
        //hitParticles.Play();
        if (currentHealth <= 0)
        {
            Explode();
        }

    }
    void Explode()
    {

        explosionSound.Stop();
        explosionSound.Play();
        Vector3 explosionPos = transform.position;

        hitColliders = Physics.OverlapSphere(explosionPos, radius, explosionLayers);
        foreach(var collider in hitColliders)
        {
            if(collider.GetComponent<Rigidbody>() != null)
            {
                collider.GetComponent<Rigidbody>().isKinematic = false;
                collider.GetComponent<Rigidbody>().AddExplosionForce(power, explosionPos, radius, 3.0f, ForceMode.Impulse);
            }
        }

        Destroy(gameObject, 0.5f);
    }
}
