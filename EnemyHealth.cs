using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(ParticleSystem))]
public class EnemyHealth : MonoBehaviour
{
    public int startingHealth = 100;
    public int currentHealth;
    public float sinkSpeed = 2.5f;
    public int scoreValue = 10;
    public AudioClip deathClip;

    Animator anim;
    AudioSource enemyAudio;
    ParticleSystem hitParticles;
    CapsuleCollider capsuleCollider;
    bool isDead;
    bool isSinking;

    void Awake()
    {
        //anim = GetComponent<Animator>();
        //enemyAudio = GetComponent<AudioSource>();
        hitParticles = GetComponent<ParticleSystem>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        currentHealth = startingHealth;
    }
    void Update()
    {
        if (isSinking)
        {
            transform.Translate(-Vector3.up * sinkSpeed * Time.deltaTime);
        }
    }

    public void TakeDamage(int amount, Vector3 hitPoint)
    {
        if (isDead)
            return;
        currentHealth -= amount;

        //hitParticles.transform.position = hitPoint;
        hitParticles.Stop();
        hitParticles.Play();
        if(currentHealth <= 0)
        {
            Death();
        }
        
    }

    void Death()
    {
        isDead = true;

        capsuleCollider.isTrigger = true;
        StartSinking();
        //anim.SetTrigger("Dead");
        //enemyAudio.clip = deathClip;
        //enemyAudio.Play();
    }

    public void StartSinking()
    {
        GetComponent<NavMeshAgent>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        isSinking = true;
        //Destroy(gameObject, 2f);
        Destroy(gameObject);
    }
}
