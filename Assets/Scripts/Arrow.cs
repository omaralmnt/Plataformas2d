using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("Arrow Settings")]
    [SerializeField] private float damage = 1f;
    [SerializeField] private bool stickToWalls = true;
    [SerializeField] private bool penetrateEnemies = false;
    
    [Header("Audio")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip stickSound;
    
    private Vector2 direction;
    private float speed;
    private float lifetime;
    private bool hasHit = false;
    
    private Rigidbody2D rb;
    private Collider2D arrowCollider;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        arrowCollider = GetComponent<Collider2D>();
    }
    
    public void Initialize(Vector2 shootDirection, float arrowSpeed, float arrowLifetime)
    {
        Initialize(shootDirection, arrowSpeed, arrowLifetime, ArrowTrap.ArrowRotationMode.FollowDirection, 0f);
    }
    
    public void Initialize(Vector2 shootDirection, float arrowSpeed, float arrowLifetime, ArrowTrap.ArrowRotationMode rotationMode, float customRotation)
    {
        direction = shootDirection.normalized;
        speed = arrowSpeed;
        lifetime = arrowLifetime;
        
        // Configurar movimiento
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
        
        // Aplicar rotación según el modo
        ApplyRotation(rotationMode, customRotation);
        
        // Destruir después del lifetime
        Destroy(gameObject, lifetime);
    }
    
    private void ApplyRotation(ArrowTrap.ArrowRotationMode rotationMode, float customRotation)
    {
        switch (rotationMode)
        {
            case ArrowTrap.ArrowRotationMode.FollowDirection:
                // Rotar hacia la dirección de movimiento
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                break;
                
            case ArrowTrap.ArrowRotationMode.NoRotation:
                // Sin rotación (0 grados)
                transform.rotation = Quaternion.identity;
                break;
                
            case ArrowTrap.ArrowRotationMode.CustomRotation:
                // Rotación personalizada
                transform.rotation = Quaternion.AngleAxis(customRotation, Vector3.forward);
                break;
                
            case ArrowTrap.ArrowRotationMode.KeepPrefabRotation:
                // Mantener la rotación original del prefab (no hacer nada)
                break;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        
        // Verificar si golpeó al player
        if (other.CompareTag("Player"))
        {
            HitPlayer(other);
            return;
        }
        
        // Verificar si golpeó una pared/suelo
        if (other.CompareTag("Ground") || other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            HitWall(other);
            return;
        }
        
        // Verificar otros objetos (enemigos, obstáculos, etc.)
        HitObject(other);
    }
    
    private void HitPlayer(Collider2D player)
    {
        hasHit = true;
        
        // Aplicar daño al player
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
        
        // Efectos
        PlayHitEffect();
        
        Debug.Log($"Arrow hit player for {damage} damage!");
        
        // Destruir la flecha
        DestroyArrow();
    }
    
    private void HitWall(Collider2D wall)
    {
        hasHit = true;
        
        if (stickToWalls)
        {
            // Parar el movimiento y pegarse a la pared
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
            }
            
            // Sonido de clavarse
            if (stickSound != null)
            {
                AudioSource.PlayClipAtPoint(stickSound, transform.position);
            }
            
            Debug.Log("Arrow stuck to wall");
            
            // Destruir después de un tiempo
            Destroy(gameObject, 3f);
        }
        else
        {
            // Destruir inmediatamente
            DestroyArrow();
        }
    }
    
    private void HitObject(Collider2D other)
    {
        hasHit = true;
        
        // Lógica para otros objetos (cajas, enemigos, etc.)
        Debug.Log($"Arrow hit: {other.name}");
        
        if (!penetrateEnemies)
        {
            DestroyArrow();
        }
    }
    
    private void PlayHitEffect()
    {
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }
        
        // Aquí podrías agregar efectos de partículas de sangre, etc.
    }
    
    private void DestroyArrow()
    {
        // Desactivar collider para evitar múltiples hits
        if (arrowCollider != null)
        {
            arrowCollider.enabled = false;
        }
        
        // Destruir inmediatamente
        Destroy(gameObject);
    }
    
    // Método para cambiar el daño desde la trampa
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
}