using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    private int currentHealth;
    public int maxHealth = 300;
    public float moveSpeed = 5f;
    private Transform player;
    
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        currentHealth = maxHealth;
    }

    void Update()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
            
        Vector3 lookDir = player.position - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 90);
        
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile"))
        {
            currentHealth -= 10;
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}
