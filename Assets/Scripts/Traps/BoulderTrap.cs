using UnityEngine;

public class BoulderTrap : MonoBehaviour
{
    [Header("Boulder Settings")]
    [SerializeField] private GameObject boulderPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float boulderSpeed = 5f;
    [SerializeField] private float damage = 3f;
    [SerializeField] private Vector2 rollDirection = Vector2.right;
    
    [Header("Trap Settings")]
    [SerializeField] private float activationDelay = 0.5f;
    [SerializeField] private bool canRetrigger = true;
    [SerializeField] private float cooldownTime = 3f;
    [SerializeField] private float boulderLifetime = 8f;
    
    [Header("Auto Spawn")]
    [SerializeField] private bool autoSpawn = true;
    [SerializeField] private float autoSpawnInterval = 2f;
    
    private bool canTrigger = true;
    
    private void Start()
    {
        // Iniciar spawn automático si está activado
        if (autoSpawn)
        {
            InvokeRepeating(nameof(AutoSpawnBoulder), autoSpawnInterval, autoSpawnInterval);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && canTrigger)
        {
            TriggerTrap();
        }
    }
    
    private void TriggerTrap()
    {
        if (!canTrigger) return;
        
        canTrigger = false;
        
        // Spawn boulder después del delay
        Invoke(nameof(SpawnBoulder), activationDelay);
        
        // Reactivar trap si se puede retriggear
        if (canRetrigger)
        {
            Invoke(nameof(ResetTrap), cooldownTime);
        }
    }
    
    private void SpawnBoulder()
    {
        if (boulderPrefab == null)
        {
            Debug.LogError("Boulder Prefab no asignado!");
            return;
        }
        
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
        Debug.Log($"Spawning boulder at: {spawnPos}");
        
        GameObject boulder = Instantiate(boulderPrefab, spawnPos, Quaternion.identity);
        Debug.Log($"Boulder created: {boulder.name}");
        
        // Verificar que el boulder sea visible
        SpriteRenderer spriteRenderer = boulder.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Debug.Log($"Boulder sprite: {spriteRenderer.sprite?.name}, enabled: {spriteRenderer.enabled}");
        }
        else
        {
            Debug.LogWarning("Boulder no tiene SpriteRenderer!");
        }
        
        // Configurar el boulder
        Boulder boulderScript = boulder.GetComponent<Boulder>();
        if (boulderScript != null)
        {
            boulderScript.Initialize(rollDirection, boulderSpeed, damage);
        }
        else
        {
            // Configuración básica si no hay script
            Rigidbody2D rb = boulder.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = boulder.AddComponent<Rigidbody2D>();
            }
            rb.velocity = rollDirection.normalized * boulderSpeed;
            Debug.Log($"Boulder velocity set to: {rb.velocity}");
        }
        
        // Destruir boulder después del lifetime
        Destroy(boulder, boulderLifetime);
    }
    
    private void ResetTrap()
    {
        canTrigger = true;
    }
    
    // Método para triggear manualmente
    public void ManualTrigger()
    {
        TriggerTrap();
    }
    
    // Método para spawn automático
    private void AutoSpawnBoulder()
    {
        SpawnBoulder();
    }
    
    // Visualización en editor
    private void OnDrawGizmosSelected()
    {
        // Dirección de la roca
        Gizmos.color = Color.red;
        Vector3 startPos = spawnPoint != null ? spawnPoint.position : transform.position;
        Gizmos.DrawRay(startPos, new Vector3(rollDirection.x, rollDirection.y, 0) * 3f);
        
        // Punto de spawn
        if (spawnPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.3f);
        }
        
        // Área del trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(1, 1, 0, 0.3f);
            Gizmos.DrawCube(transform.position, col.bounds.size);
        }
    }
}