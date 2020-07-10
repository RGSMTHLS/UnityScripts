using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public int id;
    public float health;
    public float maxHealth = 100f;

    public void Initialize(int id)
    {
        this.id = id;
        health = maxHealth;
    }

    public void SetHealth(float health)
    {
        this.health = health;
        if (health <= 0f)
        {
            GameManager.enemies.Remove(id);
            Destroy(gameObject);
        }
    }
}
