using UnityEngine;

public class EnemyController : MonoBehaviour
{
    
    public float moveSpeed = 10f;
    private Transform player;
    
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
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
            Destroy(gameObject);
        }
    }
}