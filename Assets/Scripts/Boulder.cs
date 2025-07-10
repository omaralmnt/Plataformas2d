using UnityEngine;

public class Boulder : MonoBehaviour
{
    [Header("Boulder Properties")]
    [SerializeField] private float damage = 3f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float gravityScale = 1f;
    
    private Vector2 rollDirection;
    private float speed;
    private Rigidbody2D rb;
    private bool hasHitPlayer = false;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Configurar collider si no existe
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
        }
        
        rb.gravityScale = gravityScale;
    }
    
    public void Initialize(Vector2 direction, float boulderSpeed, float boulderDamage)
    {
        rollDirection = direction.normalized;
        speed = boulderSpeed;
        damage = boulderDamage;
        
        Debug.Log($"Boulder initialized with direction: {rollDirection}, speed: {speed}");
        
        // Aplicar velocidad inicial
        if (rb != null)
        {
            rb.velocity = rollDirection * speed;
            Debug.Log($"Boulder velocity set to: {rb.velocity}");
        }
    }
    
    private void Update()
    {
        // Debug de velocidad
        if (rb != null)
        {
            // Mantener la velocidad horizontal constante
            Vector2 currentVel = rb.velocity;
            currentVel.x = rollDirection.x * speed;
            rb.velocity = currentVel;
        }
        
        // Rotar el boulder basado en su velocidad
        if (rb != null && rb.velocity.magnitude > 0.1f)
        {
            float rotationDirection = rollDirection.x >= 0 ? -1 : 1;
            transform.Rotate(0, 0, rotationSpeed * rotationDirection * Time.deltaTime);
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject hitObject = collision.gameObject;
        
        if (hitObject.CompareTag("Player") && !hasHitPlayer)
        {
            HitPlayer(hitObject);
        }
    }
    
    private void HitPlayer(GameObject player)
    {
        hasHitPlayer = true;
        
        // Aplicar daño
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"Boulder causó {damage} de daño al jugador");
        }
        
        // Empujar al jugador
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            Vector2 pushDirection = (player.transform.position - transform.position).normalized;
            playerRb.AddForce(pushDirection * 8f, ForceMode2D.Impulse);
        }
        
        // Resetear después de un momento para permitir más hits
        Invoke(nameof(ResetPlayerHit), 1f);
    }
    
    private void ResetPlayerHit()
    {
        hasHitPlayer = false;
    }
    
    // Método para cambiar dirección (útil para rebotes)
    public void ChangeDirection(Vector2 newDirection)
    {
        rollDirection = newDirection.normalized;
        if (rb != null)
        {
            rb.velocity = rollDirection * speed;
        }
    }
}