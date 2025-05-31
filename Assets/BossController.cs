using UnityEngine;
using UnityEngine.SceneManagement;

public class BossController : MonoBehaviour
{
    private int currentHealth;
    public int maxHealth = 300;
    public float moveSpeed = 5f;
    private Transform player;

    private bool isDead = false; // evita múltiplas chamadas

    void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead || player == null) return;

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
            currentHealth -= 5;
            Debug.Log("Boss atingido! Vida atual: " + currentHealth);

            if (currentHealth <= 0 && !isDead)
            {
                isDead = true;
                HandleBossDefeat();
            }
        }
    }

    void HandleBossDefeat()
    {
        Debug.Log("Boss derrotado!");

        // Destrava o cursor antes de sair da cena
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("Scenes/MainMenu");
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}
