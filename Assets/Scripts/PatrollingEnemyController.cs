using UnityEngine;

public class PatrollingEnemy : MonoBehaviour
{
    private Transform player;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float projectileSpeed = 10f;
    
    private float fireCooldown;
    
    public float descendSpeed = 5f;
    public float patrolSpeed = 3f;
    public float targetY = 4f;
    public float leftBound = -8f;
    public float rightBound = 8f;

    private bool descending = true;
    private bool movingRight = true;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        fireCooldown = fireRate;

    }
    
    void Update()
    {
        if (descending)
        {
            transform.position += Vector3.down * descendSpeed * Time.deltaTime;

            if (transform.position.y <= targetY)
            {
                transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
                descending = false;
            }
        }
        else
        {
            // Patrol left/right
            if (movingRight)
            {
                transform.Translate(Vector3.right * patrolSpeed * Time.deltaTime);
                if (transform.position.x >= rightBound)
                    movingRight = false;
            }
            else
            {
                transform.Translate(Vector3.left * patrolSpeed * Time.deltaTime);
                if (transform.position.x <= leftBound)
                    movingRight = true;
            }
            
            fireCooldown -= Time.deltaTime;

            if (fireCooldown <= 0f)
            {
                Shoot();
                fireCooldown = fireRate;
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile"))
        {
            Destroy(gameObject);
        }
    }
    
    void Shoot()
    {
        if (player == null) return;

        Vector3 direction = (player.position - firePoint.position).normalized;
        if (direction == Vector3.zero) direction = Vector3.down;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
    
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.velocity = direction * projectileSpeed;
    }

}