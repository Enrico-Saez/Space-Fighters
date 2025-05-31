using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // Necessário para Queue

public class PlayerController : MonoBehaviour
{
    public enum PlayerState { Vulnerable, Invulnerable }
    private Camera cam;
    [SerializeField] private Material playerMaterial;
    [SerializeField] private Transform playerVisual;
    [SerializeField] private AudioSource shootingSFX;

    private Vector3 normalScale = new Vector3(0.7f, 0.7f, 0.7f);
    private Vector3 squishScale = new Vector3(0.8f, 0.5f, 0.7f);
    private Color normalColor;
    private Color transparentColor;

    [Header("Movement")]
    public float moveSpeed = 15f;
    public float acceleration = 30f;
    public float deceleration = 15f;

    private Vector3 moveInput;
    private Vector3 currentVelocity;
    private Rigidbody rb;

    [Header("Invulnerability")]
    public float maxInvulnerabilityTime = 2f;
    public float cooldownTime = 5f;

    private float currentInvulnTime = 0f;
    private float cooldownTimer = 0f;
    private bool isInvulnPressed = false;

    private PlayerState currentState = PlayerState.Vulnerable;

    [Header("Shooting")]
    public GameObject projectilePrefab; // O prefab do projétil
    public Transform firePoint;       // Ponto de onde os projéteis saem
    public float fireRate = 5f;       // Projéteis por segundo
    public int initialPoolSize = 40;  // Quantos projéteis criar inicialmente

    private float fireTimer; // Controla a cadência de tiro
    private Queue<GameObject> projectilePool; // A pool de projéteis
    private Transform poolContainer; // Objeto pai para organizar os projéteis na hierarquia

    void Awake() // Usamos Awake para garantir que a pool seja criada antes de Start
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.drag = 0f; // drag padrão já era 0, mas bom garantir

        InitializeProjectilePool();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleInput();
        HandleInvulnerability();
        HandleShooting(); // Adiciona a lógica de tiro
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Enemy") || other.CompareTag("EnemyProjectile")) && currentState == PlayerState.Vulnerable)
        {
            // Destrava o cursor antes de mudar de cena
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Destroy(gameObject);
            SceneManager.LoadScene("Scenes/MainMenu");
        }
    }

    void HandleInput()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = 0f;
        if (Input.GetKey(KeyCode.Space)) inputY = 1f;
        if (Input.GetKey(KeyCode.LeftShift)) inputY = -1f;
        float inputZ = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(inputX, inputY, inputZ).normalized;

        // Movimento relativo à câmera
        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;
        Vector3 camUp = cam.transform.up;

        moveInput = (camRight * input.x) + (camUp * input.y) + (camForward * input.z);
        moveInput = moveInput.normalized;

        isInvulnPressed = Input.GetKey(KeyCode.Mouse1);
    }

    void HandleMovement()
    {
        if (moveInput != Vector3.zero)
        {
            currentVelocity = Vector3.MoveTowards(
                currentVelocity,
                moveInput * moveSpeed,
                acceleration * Time.fixedDeltaTime
            );

            // Usa a currentVelocity para suavizar a rotação também
            Vector3 lookDirection = new Vector3(currentVelocity.x, 0, currentVelocity.z);

            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 3f * Time.fixedDeltaTime);
            }
        }
        else
        {
            currentVelocity = Vector3.MoveTowards(
                currentVelocity,
                Vector3.zero,
                deceleration * Time.fixedDeltaTime
            );
        }

        rb.velocity = currentVelocity;
    }

    void HandleInvulnerability()
    {
        // Reset do tempo de invulnerabilidade se não estiver pressionando espaço
        if (!isInvulnPressed && currentState == PlayerState.Invulnerable)
        {
            // Se soltou o espaço antes do tempo máximo, entra em cooldown
            currentState = PlayerState.Vulnerable;
            cooldownTimer = cooldownTime;
            currentInvulnTime = 0f;
            ResetVisuals(); // Reseta o tempo acumulado
        }

        if (isInvulnPressed && cooldownTimer <= 0f)
        {
            if (currentInvulnTime < maxInvulnerabilityTime)
            {
                currentState = PlayerState.Invulnerable;
                currentInvulnTime += Time.deltaTime;
                ApplyInvulnerableVisuals();
            }
            else
            {
                // Tempo de invulnerabilidade esgotado, força Vulnerable e cooldown
                currentState = PlayerState.Vulnerable;
                cooldownTimer = cooldownTime;
                currentInvulnTime = 0f; // Reseta o tempo acumulado
            }
        }
        else if (!isInvulnPressed) // Se não está pressionando e não estava invulnerável antes
        {
            // Garante que está vulnerável e reseta o tempo de invulnerabilidade
            currentState = PlayerState.Vulnerable;
            currentInvulnTime = 0f;
        }

        // Tick cooldown
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer < 0f) cooldownTimer = 0f; // Garante que não fique negativo
        }
    }

    void HandleShooting()
    {
        // Decrementa o timer de tiro
        if (fireTimer > 0)
        {
            fireTimer -= Time.deltaTime;
        }

        // Verifica se o botão esquerdo está pressionado E o jogador está vulnerável
        if (Input.GetMouseButton(0) && currentState == PlayerState.Vulnerable)
        {
            // Verifica se pode atirar (baseado no fireRate)
            if (fireTimer <= 0f)
            {
                Shoot();
                // Reseta o timer baseado na cadência (1 / projéteis por segundo)
                fireTimer = 1f / fireRate;
            }
        }
    }

    void Shoot()
    {
        GameObject projectile = GetProjectileFromPool();
        if (projectile != null)
        {
            // Ray do centro da tela
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Vector3 shootDirection;

            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                // Se acertar algo, atira para esse ponto
                shootDirection = (hit.point - firePoint.position).normalized;
            }
            else
            {
                // Se não acertar nada, atira para frente da câmera
                shootDirection = cam.transform.forward;
            }

            // Define posição do projétil na boca da arma
            projectile.transform.position = firePoint.position;

            // Rota o projétil para a direção calculada
            projectile.transform.rotation = Quaternion.LookRotation(shootDirection);

            // Ativa o projétil
            projectile.SetActive(true);

            // Som
            if (shootingSFX != null)
            {
                shootingSFX.Play();
            }
        }
        else
        {
            Debug.LogWarning("Não foi possível obter um projétil da pool.");
        }
    }

    // --- Object Pooling ---

    void InitializeProjectilePool()
    {
        projectilePool = new Queue<GameObject>();

        // Cria um objeto vazio para guardar os projéteis inativos
        poolContainer = new GameObject("ProjectilePoolContainer").transform;
        poolContainer.SetParent(this.transform.parent); // Opcional: organizar na cena

        if (projectilePrefab == null)
        {
            Debug.LogError("Prefab do projétil não atribuído no PlayerController!");
            return; // Evita erro NullReferenceException
        }


        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject projectile = Instantiate(projectilePrefab, poolContainer);
            projectile.SetActive(false); // Começa desativado
                                         // Guarda referência para a pool no projétil para ele poder retornar sozinho
            var projController = projectile.GetComponent<ProjectileController>(); // Assumindo que você terá um script no projétil
            if (projController != null)
            {
                projController.SetPool(this); // Passa a referência desta instância do PlayerController
            }
            else
            {
                Debug.LogWarning($"Projétil {projectilePrefab.name} não tem o script ProjectileController. A devolução manual será necessária ou ele nunca voltará para a pool.");
            }
            projectilePool.Enqueue(projectile);
        }
    }

    GameObject GetProjectileFromPool()
    {
        if (projectilePool.Count > 0)
        {
            GameObject projectile = projectilePool.Dequeue();
            return projectile;
        }
        else
        {
            // Opcional: Expandir a pool se ela acabar
            Debug.LogWarning("Pool de projéteis vazia. Expandindo...");
            GameObject projectile = Instantiate(projectilePrefab, poolContainer);
            var projController = projectile.GetComponent<ProjectileController>();
            if (projController != null) projController.SetPool(this);
            // Não enfileirar imediatamente, pois ele será usado agora
            return projectile; // Retorna o projétil recém-criado

            // Ou retornar null se não quiser expansão dinâmica
            // return null;
        }
    }

    // Método para ser chamado pelo projétil quando ele deve retornar à pool
    public void ReturnProjectileToPool(GameObject projectile)
    {
        projectile.SetActive(false);
        projectile.transform.SetParent(poolContainer); // Garante que volte para o container
        projectilePool.Enqueue(projectile);
    }

    // Call this when entering invulnerable state
    void ApplyInvulnerableVisuals()
    {
        playerVisual.localScale = squishScale;
        playerMaterial.color = transparentColor;
    }

    // Call this when returning to normal
    void ResetVisuals()
    {
        playerVisual.localScale = normalScale;
        playerMaterial.color = normalColor;
    }


    // --- Métodos Públicos ---

    public bool IsInvulnerable()
    {
        return currentState == PlayerState.Invulnerable;
    }

    public float GetCooldownPercent()
    {
        if (cooldownTime <= 0) return 0f; // Evita divisão por zero
        return Mathf.Clamp01(cooldownTimer / cooldownTime);
    }

    public PlayerState GetCurrentState() // Método útil para debug ou UI
    {
        return currentState;
    }
}