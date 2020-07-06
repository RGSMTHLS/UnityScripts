using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//Remember to set all obstacles to "Static" and then Bake navigation to make Pathfinding work.
//Set NavMeshAgent values depending on which kind of enemy. 
//Radius 0.3, Speed 3, Stopping Distance 1.3, Height 1.1
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    Transform player;
    PlayerHealth playerHealth;
    EnemyHealth enemyHealth;
    NavMeshAgent nav;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = player.GetComponent<PlayerHealth>();
        enemyHealth = GetComponent<EnemyHealth>();
        nav = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if(enemyHealth.currentHealth > 0 && playerHealth.currentHealth > 0)
        {
            nav.SetDestination(player.position);
        }
        else
        {
            nav.enabled = false;
        }
    }
}
