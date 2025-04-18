using UnityEngine;
using System.Collections.Generic; // Necessário para Queue

public class PlayerController : MonoBehaviour
{
    public enum PlayerState { Vulnerable, Invulnerable }
	private Camera cam;
	[SerializeField] private Material playerMaterial;
	[SerializeField] private Transform playerVisual; // reference to the visual child (like the mesh or sprite)

	private Vector3 normalScale = new Vector3(0.7f, 0.7f, 0.7f);
	private Vector3 squishScale = new Vector3(0.8f, 0.5f, 0.7f);
	private Color normalColor;
	private Color transparentColor;

    [Header("Movement")]
    public float moveSpeed = 15f;
    public float acceleration = 30f;
    public float deceleration = 15f;

    private Vector2 moveInput;
    private Vector2 currentVelocity;
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
    public int initialPoolSize = 20;  // Quantos projéteis criar inicialmente

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

    void Update()
    {
        HandleInput();
        HandleInvulnerability();
        HandleShooting(); // Adiciona a lógica de tiro
    }

    void FixedUpdate()
    {
        HandleMovement();
		// Clamp player inside camera view
		Vector3 pos = transform.position;
		Vector3 viewportPos = cam.WorldToViewportPoint(pos);
		viewportPos.x = Mathf.Clamp(viewportPos.x, 0f, 1f);
		viewportPos.y = Mathf.Clamp(viewportPos.y, 0f, 1f);
		transform.position = cam.ViewportToWorldPoint(viewportPos);
    }

    void HandleInput()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        isInvulnPressed = Input.GetKey(KeyCode.Space);
        // Input de tiro será verificado em HandleShooting
    }

    void HandleMovement()
    {
        // Lógica de movimento permanece a mesma
        if (moveInput != Vector2.zero)
        {
            currentVelocity = Vector2.MoveTowards(currentVelocity, moveInput * moveSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentVelocity = Vector2.MoveTowards(currentVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
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
        // Pega um projétil da pool
        GameObject projectile = GetProjectileFromPool();
        if (projectile != null)
        {
            // Define a posição e rotação
            projectile.transform.position = firePoint.position;
            // Rotação para cima (pode ajustar se firePoint tiver rotação)
            projectile.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);

            // Ativa o projétil
            projectile.SetActive(true);

            // (Opcional) Configura o projétil se ele precisar de algo (velocidade, dano, etc.)
            // Exemplo: projectile.GetComponent<ProjectileController>().Setup(...);
        }
        else
        {
             Debug.LogWarning("Pool de projéteis esgotada!"); // Avisa se a pool acabou
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
             } else {
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