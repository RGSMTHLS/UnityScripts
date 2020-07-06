using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Remember to set a gameobjects tag to "Player" so this script knows what to chase
//Give the Enemy an additional collider that is set to "Is Trigger" to be able to attack
public class EnemyAttack : MonoBehaviour
{
    public float timeBetweenAttacks = 0.5f;
    public int attackDamage = 10;

    Animator anim;
    GameObject player;
    PlayerHealth playerHealth;
    EnemyHealth enemyHealth;
    bool playerInRange;
    float timer;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerHealth = player.GetComponent<PlayerHealth>();
        //anim = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == player)
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject == player)
        {
            playerInRange = false;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if(timer >= timeBetweenAttacks && playerInRange && enemyHealth.currentHealth > 0)
        {
            Attack();
        }

        if(playerHealth.currentHealth <= 0)
        {
            //anim.SetTrigger("PlayerDead");
        }
    }

    void Attack()
    {
        timer = 0f;

        if(playerHealth.currentHealth > 0)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }
}
