using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
    [Header("Arrow Settings")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float arrowSpeed = 10f;
    [SerializeField] private float arrowLifetime = 5f;
    
    [Header("Shooting Pattern")]
    [SerializeField] private float shootInterval = 2f; // Tiempo entre disparos
    [SerializeField] private bool startShooting = true; // Empezar disparando automáticamente
    [SerializeField] private Vector2 shootDirection = Vector2.right; // Dirección de disparo
    
    [Header("Trigger Settings")]
    [SerializeField] private bool shootOnPlayerDetection = false;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private LayerMask playerLayer = 1; // Layer del player
    
    [Header("Audio")]
    [SerializeField] private AudioClip shootSound;
    
    [Header("Visual Effects")]
    [SerializeField] private bool rotateToShootDirection = false; // Trampa no se rota por defecto
    [SerializeField] private ArrowRotationMode arrowRotationMode = ArrowRotationMode.FollowDirection;
    [SerializeField] private float customArrowRotation = 0f; // Rotación manual en grados
    [SerializeField] private ParticleSystem shootEffect; // Opcional: efecto de partículas
    
    public enum ArrowRotationMode
    {
        FollowDirection,    // La flecha apunta hacia donde va (automático)
        NoRotation,         // La flecha mantiene rotación 0
        CustomRotation,     // Rotación manual específica
        KeepPrefabRotation  // Mantiene la rotación del prefab original
    }
    
    private float shootTimer;
    private bool canShoot = true;
    private Transform player;
    
    private void Start()
    {
        // Buscar al player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // Solo rotar la trampa si está habilitado
        if (rotateToShootDirection)
        {
            float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        // Empezar timer
        shootTimer = shootInterval;
    }
    
    private void Update()
    {
        if (shootOnPlayerDetection)
        {
            DetectPlayer();
        }
        else if (startShooting)
        {
            AutomaticShooting();
        }
    }
    
    private void AutomaticShooting()
    {
        if (!canShoot) return;
        
        shootTimer -= Time.deltaTime;
        
        if (shootTimer <= 0f)
        {
            Shoot();
            shootTimer = shootInterval;
        }
    }
    
    private void DetectPlayer()
    {
        if (player == null || !canShoot) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= detectionRange)
        {
            // Opcional: Apuntar hacia el player
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            
            Shoot(directionToPlayer);
            
            // Cooldown después de disparar
            canShoot = false;
            Invoke(nameof(ResetShooting), shootInterval);
        }
    }
    
    public void Shoot()
    {
        Shoot(shootDirection);
    }
    
    public void Shoot(Vector2 direction)
    {
        if (arrowPrefab == null) return;
        
        // Crear flecha
        Vector3 spawnPosition = shootPoint != null ? shootPoint.position : transform.position;
        GameObject arrow = Instantiate(arrowPrefab, spawnPosition, Quaternion.identity);
        
        // Configurar la flecha
        Arrow arrowComponent = arrow.GetComponent<Arrow>();
        if (arrowComponent != null)
        {
            arrowComponent.Initialize(direction, arrowSpeed, arrowLifetime, arrowRotationMode, customArrowRotation);
        }
        else
        {
            // Fallback: usar Rigidbody2D directamente
            Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
            if (arrowRb != null)
            {
                arrowRb.velocity = direction.normalized * arrowSpeed;
                
                // Aplicar rotación según el modo seleccionado
                ApplyArrowRotation(arrow, direction);
                
                // Destruir después del lifetime
                Destroy(arrow, arrowLifetime);
            }
        }
        
        // Efectos
        PlayShootEffects();
        
        Debug.Log($"Arrow trap shot arrow in direction: {direction}");
    }
    
    private void ApplyArrowRotation(GameObject arrow, Vector2 direction)
    {
        switch (arrowRotationMode)
        {
            case ArrowRotationMode.FollowDirection:
                // Rotar hacia la dirección de movimiento
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                break;
                
            case ArrowRotationMode.NoRotation:
                // Sin rotación (0 grados)
                arrow.transform.rotation = Quaternion.identity;
                break;
                
            case ArrowRotationMode.CustomRotation:
                // Rotación personalizada
                arrow.transform.rotation = Quaternion.AngleAxis(customArrowRotation, Vector3.forward);
                break;
                
            case ArrowRotationMode.KeepPrefabRotation:
                // Mantener la rotación original del prefab (no hacer nada)
                break;
        }
    }
    
    private void PlayShootEffects()
    {
        // Sonido
        if (shootSound != null)
        {
            AudioSource.PlayClipAtPoint(shootSound, transform.position);
        }
        
        // Efecto de partículas
        if (shootEffect != null)
        {
            shootEffect.Play();
        }
    }
    
    private void ResetShooting()
    {
        canShoot = true;
    }
    
    // Métodos públicos para control externo
    public void StartShooting()
    {
        startShooting = true;
        canShoot = true;
    }
    
    public void StopShooting()
    {
        startShooting = false;
        canShoot = false;
    }
    
    public void SetShootInterval(float interval)
    {
        shootInterval = interval;
    }
    
    public void SetShootDirection(Vector2 direction)
    {
        shootDirection = direction.normalized;
    }
    
    // Debug visual
    private void OnDrawGizmosSelected()
    {
        // Dibujar dirección de disparo
        Gizmos.color = Color.red;
        Vector3 shootDir = new Vector3(shootDirection.x, shootDirection.y, 0);
        Gizmos.DrawRay(transform.position, shootDir * 3f);
        
        // Dibujar rango de detección si está habilitado
        if (shootOnPlayerDetection)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
        
        // Dibujar punto de disparo
        if (shootPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(shootPoint.position, 0.2f);
        }
    }
}