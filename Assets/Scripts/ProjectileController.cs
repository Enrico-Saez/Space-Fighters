using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 3f; // Tempo em segundos antes de retornar à pool

    private float lifetimeTimer;
    private PlayerController ownerPool; // Referência para a pool do jogador

    // Método para o PlayerController definir a qual pool este projétil pertence
    public void SetPool(PlayerController pool)
    {
        ownerPool = pool;
    }

    // Chamado quando o objeto é ativado (SetActive(true))
    void OnEnable()
    {
        lifetimeTimer = lifetime; // Reseta o tempo de vida
    }

    void Update()
    {
        // Move o projétil para cima
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        // Decrementa o tempo de vida
        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0f)
        {
            ReturnToPool();
        }
    }

    // Colisão (Opcional, exemplo)
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyProjectile"))
        {
            Destroy(other.gameObject);
        }
        ReturnToPool();
    }


    void ReturnToPool()
    {
        // Verifica se tem uma referência válida da pool antes de tentar retornar
        if (ownerPool != null)
        {
            ownerPool.ReturnProjectileToPool(this.gameObject);
        }
        else
        {
            Debug.LogError("Projétil não sabe a qual pool retornar! Destruindo para evitar problemas.");
            Destroy(gameObject); // Destruir como fallback se a referência da pool foi perdida
        }
    }
}