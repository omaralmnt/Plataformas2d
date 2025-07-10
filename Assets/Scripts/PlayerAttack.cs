using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 1f;
    
    [Header("Fireball Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private int maxFireballs = 5;
    [SerializeField] private float fireballSpeed = 10f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip fireballLaunchSound;
    
    private GameObject[] fireballPool;
    private Animator playerAnimator;
    private PlayerMovement playerMovement;
    private float lastAttackTime;
    
    private void Awake()
    {
        playerAnimator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        
        // Crear pool de fireballs
        CreateFireballPool();
    }
    
    private void CreateFireballPool()
    {
        fireballPool = new GameObject[maxFireballs];
        
        for (int i = 0; i < maxFireballs; i++)
        {
            fireballPool[i] = Instantiate(fireballPrefab);
            fireballPool[i].SetActive(false);
            
            // Configurar el projectile component
            Projectile projectile = fireballPool[i].GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.SetSpeed(fireballSpeed);
            }
        }
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    private void HandleInput()
    {
        // VERSION SUPER SIMPLE PARA DEBUG
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("FORCING ATTACK!");
            Attack(); // Forzar ataque sin condiciones
        }
    }
    
    private bool CanAttack()
    {
        bool cooldownReady = Time.time >= lastAttackTime + attackCooldown;
        bool playerCanAttack = playerMovement != null ? playerMovement.canAttack() : true;
        
        Debug.Log($"Cooldown ready: {cooldownReady}, Player can attack: {playerCanAttack}");
        
        return cooldownReady && playerCanAttack;
    }
    
    private void Attack()
    {
        Debug.Log("Attack() called!");
        
        // Reproducir sonido de ataque
        if (attackSound != null)
        {
            AudioSource.PlayClipAtPoint(attackSound, transform.position);
        }
        
        // Trigger animation
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("attack");
        }
        
        // Spawn fireball
        SpawnFireball();
        
        // Update cooldown
        lastAttackTime = Time.time;
    }
    
    private void SpawnFireball()
    {
        Debug.Log("SpawnFireball() called!");
        
        GameObject availableFireball = GetAvailableFireball();
        
        if (availableFireball == null) 
        {
            Debug.LogError("No available fireball found!");
            return;
        }
        
        Debug.Log($"Using fireball: {availableFireball.name}");
        
        // Position fireball at fire point
        availableFireball.transform.position = firePoint.position;
        Debug.Log($"Fireball positioned at: {firePoint.position}");
        
        // Determine direction based on player facing
        float direction = Mathf.Sign(transform.localScale.x);
        Debug.Log($"Direction: {direction}");
        
        // Configure and activate fireball
        Projectile projectile = availableFireball.GetComponent<Projectile>();
        if (projectile != null)
        {
            availableFireball.SetActive(true);
            Debug.Log($"Fireball activated: {availableFireball.activeInHierarchy}");
            
            // Reproducir sonido de lanzamiento de fireball
            if (fireballLaunchSound != null)
            {
                AudioSource.PlayClipAtPoint(fireballLaunchSound, firePoint.position);
            }
            
            projectile.Launch(direction);
        }
        else
        {
            Debug.LogError("No Projectile component found on fireball!");
        }
    }
    
    private GameObject GetAvailableFireball()
    {
        for (int i = 0; i < fireballPool.Length; i++)
        {
            if (!fireballPool[i].activeInHierarchy)
            {
                return fireballPool[i];
            }
        }
        
        // Si no hay fireballs disponibles, reutilizar la más antigua
        return fireballPool[0];
    }
    
    // Método público para obtener información del estado
    public bool IsOnCooldown()
    {
        return Time.time < lastAttackTime + attackCooldown;
    }
    
    public float GetCooldownProgress()
    {
        float timeSinceAttack = Time.time - lastAttackTime;
        return Mathf.Clamp01(timeSinceAttack / attackCooldown);
    }
}