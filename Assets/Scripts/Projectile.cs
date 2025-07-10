using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private int damage = 1;
    
    private float direction;
    private bool isActive;
    private float currentLifetime;
    
    private Collider2D projectileCollider;
    
    private void Awake()
    {
        projectileCollider = GetComponent<Collider2D>();
    }
    
    private void Update()
    {
        if (!isActive) return;
        
        // Move projectile
        MoveProjectile();
        
        // Update lifetime
        UpdateLifetime();
    }
    
    private void MoveProjectile()
    {
        float moveDistance = speed * direction * Time.deltaTime;
        transform.Translate(moveDistance, 0, 0);
    }
    
    private void UpdateLifetime()
    {
        currentLifetime += Time.deltaTime;
        
        if (currentLifetime >= lifetime)
        {
            DeactivateProjectile();
        }
    }
    
    public void Launch(float launchDirection)
    {
        Debug.Log($"Projectile.Launch() called with direction: {launchDirection}");
        
        // Reset state
        direction = launchDirection;
        isActive = true;
        currentLifetime = 0f;
        
        // Enable components
        if (projectileCollider != null)
            projectileCollider.enabled = true;
        
        // Set facing direction
        SetFacingDirection(direction);
        
        Debug.Log($"Projectile is now active: {isActive}, position: {transform.position}");
    }
    
    private void SetFacingDirection(float dir)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * dir;
        transform.localScale = scale;
    }
    
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignorar al player que lo disparó
        if (other.CompareTag("Player")) return;
        
        // Verificar si es un enemigo y tiene componente Health
        if (other.CompareTag("Enemy"))
        {
            Health enemyHealth = other.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log($"Projectile hit enemy and dealt {damage} damage");
            }
            else
            {
                Debug.LogWarning($"Enemy {other.name} doesn't have Health component");
            }
        }
        
        // Desactivar al tocar cualquier cosa
        DeactivateProjectile();
    }
    
    private void DeactivateProjectile()
    {
        isActive = false;
        gameObject.SetActive(false);
    }
    
    private void OnDisable()
    {
        // Reset state when disabled
        isActive = false;
        currentLifetime = 0f;
    }
    
    // Método para compatibilidad con animaciones existentes (por si acaso)
    public void Deactivate()
    {
        DeactivateProjectile();
    }
}