using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 3f;
    public bool forward = false;

    private float lifetimeTimer;
    private PlayerController ownerPool;

    // Referência ao Collider do player para ignorar colisão
    private Collider playerCollider;

    public void SetPool(PlayerController pool)
    {
        ownerPool = pool;
    }

    void OnEnable()
    {
        lifetimeTimer = lifetime;

        // Ignora colisão com o jogador
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerCollider = player.GetComponent<Collider>();
            Collider projectileCollider = GetComponent<Collider>();

            if (playerCollider != null && projectileCollider != null)
            {
                Physics.IgnoreCollision(projectileCollider, playerCollider, true);
            }
        }
    }

    void Update()
    {
        // Movimento baseado na rotação do projétil
        Vector3 direction = forward ? transform.forward : Vector3.up;
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0f)
        {
            ReturnToPool();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Ignora colisão com o jogador ou outros projéteis
        if (other.CompareTag("Player") || other.CompareTag("Projectile"))
            return;

        // Dano ao inimigo ou boss é tratado no outro objeto
        ReturnToPool();
    }

    void ReturnToPool()
    {
        if (ownerPool != null)
        {
            gameObject.SetActive(false);
            transform.SetParent(ownerPool.transform); // opcional, para organização
            ownerPool.ReturnProjectileToPool(gameObject);
        }
        else
        {
            Debug.LogError("Projétil não sabe a qual pool retornar! Destruindo para evitar problemas.");
            Destroy(gameObject);
        }
    }
}
