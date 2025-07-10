using UnityEngine;
using System.Collections;

public enum CrushDirection
{
    Down,
    Up,
    Left,
    Right
}

public enum DetectionType
{
    Radial,    // Detección circular (original)
    Linear     // Detección rectangular/linear
}

public class CrusherTrap : MonoBehaviour
{
    [Header("Crusher Settings")]
    [SerializeField] private Transform crusherBlock; // El bloque que aplasta
    [SerializeField] private Transform triggerZone; // Zona que activa la trampa
    [SerializeField] private CrushDirection crushDirection = CrushDirection.Down; // Dirección del aplastamiento
    [SerializeField] private float crushDistance = 5f; // Qué tan lejos se mueve
    [SerializeField] private float crushSpeed = 8f; // Velocidad de movimiento
    [SerializeField] private float returnSpeed = 2f; // Velocidad de retorno
    [SerializeField] private float stayOutTime = 1f; // Tiempo que se queda en posición final
    [SerializeField] private int damage = 1;
    
    [Header("Warning")]
    [SerializeField] private float warningTime = 1f; // Tiempo de advertencia
    [SerializeField] private float shakeIntensity = 0.1f; // Intensidad del temblor
    
    [Header("Audio")]
    [SerializeField] private AudioClip warningSound;
    [SerializeField] private AudioClip crushSound;
    [SerializeField] private AudioClip returnSound;
    
    [Header("Detection")]
    [SerializeField] private bool autoTrigger = true; // Se activa automáticamente
    [SerializeField] private DetectionType detectionType = DetectionType.Radial;
    [SerializeField] private GameObject targetPlayer; // GameObject específico del jugador
    [SerializeField] private float detectionRange = 3f; // Rango de detección
    [SerializeField] private Vector2 detectionBoxSize = new Vector2(2f, 1f); // Tamaño de detección linear
    
    private Vector3 originalPosition;
    private Vector3 crushPosition;
    private Vector3 directionVector;
    private AudioSource audioSource;
    private bool isActive = false;
    private bool isPlayerInTrigger = false;
    private Collider2D crusherCollider;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Crear AudioSource si no existe
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 1f;
        }
        
        // Si no se asigna crusher block, usar este mismo objeto
        if (crusherBlock == null)
            crusherBlock = transform;
        
        // Guardar posición original y calcular dirección
        originalPosition = crusherBlock.position;
        CalculateDirectionAndPositions();
        
        // Obtener collider del crusher
        crusherCollider = crusherBlock.GetComponent<Collider2D>();
        if (crusherCollider == null)
        {
            Debug.LogWarning("Crusher block needs a Collider2D component!");
        }
        
        // Verificar que el target player esté asignado
        if (targetPlayer == null)
        {
            // Intentar encontrar automáticamente al jugador
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
            {
                targetPlayer = foundPlayer;
                Debug.Log($"CrusherTrap auto-assigned player: {foundPlayer.name}");
            }
            else
            {
                Debug.LogWarning("No target player assigned and no GameObject with 'Player' tag found!");
            }
        }
    }
    
    private void CalculateDirectionAndPositions()
    {
        // Calcular vector de dirección basado en la dirección seleccionada
        switch (crushDirection)
        {
            case CrushDirection.Down:
                directionVector = Vector3.down;
                break;
            case CrushDirection.Up:
                directionVector = Vector3.up;
                break;
            case CrushDirection.Left:
                directionVector = Vector3.left;
                break;
            case CrushDirection.Right:
                directionVector = Vector3.right;
                break;
        }
        
        // Calcular posición final del crush
        crushPosition = originalPosition + directionVector * crushDistance;
    }
    
    private void Update()
    {
        if (autoTrigger && !isActive)
        {
            CheckForPlayer();
        }
    }
    
    private void CheckForPlayer()
    {
        if (targetPlayer == null) return;
        
        // Detectar según el tipo de detección
        Vector3 detectionCenter = triggerZone != null ? triggerZone.position : transform.position;
        bool playerDetected = false;
        float currentDistance = Vector3.Distance(detectionCenter, targetPlayer.transform.position);
        
        if (detectionType == DetectionType.Radial)
        {
            // Detección circular
            playerDetected = currentDistance <= detectionRange;
        }
        else // DetectionType.Linear
        {
            // Detección rectangular
            Vector3 playerPos = targetPlayer.transform.position;
            Vector3 boxCenter = detectionCenter;
            
            // Verificar si el jugador está dentro del rectángulo
            bool withinX = Mathf.Abs(playerPos.x - boxCenter.x) <= detectionBoxSize.x * 0.5f;
            bool withinY = Mathf.Abs(playerPos.y - boxCenter.y) <= detectionBoxSize.y * 0.5f;
            
            playerDetected = withinX && withinY;
        }
        
        if (playerDetected && !isPlayerInTrigger)
        {
            isPlayerInTrigger = true;
            TriggerCrusher();
        }
        else if (!playerDetected && isPlayerInTrigger)
        {
            isPlayerInTrigger = false;
        }
    }
    
    public void TriggerCrusher()
    {
        if (!isActive)
        {
            StartCoroutine(CrusherSequence());
        }
    }
    
    private IEnumerator CrusherSequence()
    {
        isActive = true;
        
        // Fase de advertencia
        yield return StartCoroutine(WarningPhase());
        
        // Crush
        yield return StartCoroutine(CrushMovement());
        
        // Stay in final position
        yield return new WaitForSeconds(stayOutTime);
        
        // Return to original position
        yield return StartCoroutine(ReturnMovement());
        
        isActive = false;
    }
    
    private IEnumerator WarningPhase()
    {
        // Reproducir sonido de advertencia
        if (warningSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(warningSound);
        }
        
        // Temblor de advertencia
        Vector3 originalPos = crusherBlock.position;
        float timer = 0f;
        
        while (timer < warningTime)
        {
            // Crear efecto de temblor
            float shakeX = Random.Range(-shakeIntensity, shakeIntensity);
            float shakeY = Random.Range(-shakeIntensity, shakeIntensity);
            
            Vector3 newPos = originalPos + new Vector3(shakeX, shakeY, 0);
            crusherBlock.position = newPos;
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        // Volver a la posición original
        crusherBlock.position = originalPos;
    }
    
    private IEnumerator CrushMovement()
    {
        // Reproducir sonido de crush
        if (crushSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(crushSound);
        }
        
        Vector3 startPos = crusherBlock.position;
        float timer = 0f;
        float journeyLength = Vector3.Distance(startPos, crushPosition);
        float journeyTime = journeyLength / crushSpeed;
        
        while (timer < journeyTime)
        {
            timer += Time.deltaTime;
            float progress = timer / journeyTime;
            
            // Movimiento con curva de aceleración (más rápido al final)
            float easedProgress = progress * progress;
            Vector3 newPosition = Vector3.Lerp(startPos, crushPosition, easedProgress);
            crusherBlock.position = newPosition;
            
            yield return null;
        }
        
        crusherBlock.position = crushPosition;
    }
    
    private IEnumerator ReturnMovement()
    {
        // Reproducir sonido de retorno
        if (returnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(returnSound);
        }
        
        Vector3 startPos = crusherBlock.position;
        float timer = 0f;
        float journeyLength = Vector3.Distance(startPos, originalPosition);
        float journeyTime = journeyLength / returnSpeed;
        
        while (timer < journeyTime)
        {
            timer += Time.deltaTime;
            float progress = timer / journeyTime;
            
            // Movimiento linear de retorno
            crusherBlock.position = Vector3.Lerp(startPos, originalPosition, progress);
            
            yield return null;
        }
        
        crusherBlock.position = originalPosition;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debug solo para el sistema de daño
        Debug.Log($"OnTriggerEnter2D: {other.gameObject.name} | Active: {isActive} | Target: {(targetPlayer != null ? targetPlayer.name : "NULL")}");
        
        // Solo hacer daño cuando el crusher está activo y es el target player
        if (isActive && targetPlayer != null && other.gameObject == targetPlayer)
        {
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"💥 Crusher dealt {damage} damage to {targetPlayer.name}");
            }
            else
            {
                Debug.LogError($"❌ {targetPlayer.name} has no Health component!");
            }
        }
    }
    
    // Agregar estos métodos de debug para verificar setup
    private void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log($"OnTriggerStay2D: {other.gameObject.name} is inside crusher trigger");
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"OnTriggerExit2D: {other.gameObject.name} left crusher trigger");
    }
    
    // Método para verificar setup manualmente
    [ContextMenu("Debug Crusher Setup")]
    public void DebugCrusherSetup()
    {
        Debug.Log("=== CRUSHER SETUP DEBUG ===");
        Debug.Log($"CrusherTrap GameObject: {gameObject.name}");
        Debug.Log($"CrusherBlock assigned: {(crusherBlock != null ? crusherBlock.name : "NULL")}");
        
        if (crusherBlock != null)
        {
            Debug.Log($"CrusherBlock position: {crusherBlock.position}");
            
            // Verificar Collider2D
            Collider2D collider = crusherBlock.GetComponent<Collider2D>();
            if (collider != null)
            {
                Debug.Log($"✅ Collider found: {collider.GetType().Name}");
                Debug.Log($"   - IsTrigger: {collider.isTrigger}");
                Debug.Log($"   - Enabled: {collider.enabled}");
                Debug.Log($"   - Bounds: {collider.bounds}");
            }
            else
            {
                Debug.LogError("❌ NO COLLIDER2D found on CrusherBlock!");
            }
            
            // Verificar Rigidbody2D
            Rigidbody2D rb = crusherBlock.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Debug.Log($"✅ Rigidbody2D found: BodyType = {rb.bodyType}, Simulated = {rb.simulated}");
            }
            else
            {
                Debug.LogError("❌ NO RIGIDBODY2D found on CrusherBlock!");
            }
        }
        
        // Verificar target player
        if (targetPlayer != null)
        {
            Debug.Log($"✅ Target player: {targetPlayer.name}");
            Debug.Log($"   - Position: {targetPlayer.transform.position}");
            Debug.Log($"   - Tag: {targetPlayer.tag}");
            
            // Verificar colliders del player
            Collider2D[] playerColliders = targetPlayer.GetComponents<Collider2D>();
            Debug.Log($"   - Player colliders: {playerColliders.Length}");
            foreach (var col in playerColliders)
            {
                Debug.Log($"     * {col.GetType().Name}: IsTrigger={col.isTrigger}, Enabled={col.enabled}");
            }
            
            // Verificar Rigidbody2D del player
            Rigidbody2D playerRb = targetPlayer.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Debug.Log($"   - Player Rigidbody2D: BodyType={playerRb.bodyType}");
            }
            else
            {
                Debug.Log("   - Player has NO Rigidbody2D");
            }
        }
        else
        {
            Debug.LogError("❌ NO TARGET PLAYER assigned!");
        }
        
        Debug.Log("=== END CRUSHER SETUP DEBUG ===");
    }
    
    // Método para activar manualmente desde otros scripts
    public void ActivateManually()
    {
        TriggerCrusher();
    }
    
    // Métodos para configurar en runtime
    public void SetCrushDistance(float distance)
    {
        crushDistance = distance;
        CalculateDirectionAndPositions();
    }
    
    public void SetCrushDirection(CrushDirection newDirection)
    {
        crushDirection = newDirection;
        CalculateDirectionAndPositions();
    }
    
    public void SetSpeed(float newCrushSpeed, float newReturnSpeed)
    {
        crushSpeed = newCrushSpeed;
        returnSpeed = newReturnSpeed;
    }
    
    private void OnDrawGizmosSelected()
    {
        // Dibujar zona de detección
        Vector3 detectionCenter = triggerZone != null ? triggerZone.position : transform.position;
        
        if (detectionType == DetectionType.Radial)
        {
            // Detección circular
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(detectionCenter, detectionRange);
        }
        else // DetectionType.Linear
        {
            // Detección rectangular
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(detectionCenter, new Vector3(detectionBoxSize.x, detectionBoxSize.y, 0.1f));
        }
        
        // Dibujar rango de movimiento del crusher
        Vector3 crusherPos = crusherBlock != null ? crusherBlock.position : transform.position;
        
        // Calcular posición final basada en la dirección
        Vector3 finalDirection = Vector3.down; // Default
        switch (crushDirection)
        {
            case CrushDirection.Down: finalDirection = Vector3.down; break;
            case CrushDirection.Up: finalDirection = Vector3.up; break;
            case CrushDirection.Left: finalDirection = Vector3.left; break;
            case CrushDirection.Right: finalDirection = Vector3.right; break;
        }
        
        Vector3 crushPos = crusherPos + finalDirection * crushDistance;
        
        // Dibujar línea de movimiento
        Gizmos.color = Color.red;
        Gizmos.DrawLine(crusherPos, crushPos);
        
        // Dibujar posición final
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(crushPos, Vector3.one);
        
        // Dibujar posición original
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(crusherPos, Vector3.one);
        
        // Dibujar flecha de dirección
        Gizmos.color = Color.cyan;
        Vector3 arrowPos = crusherPos + finalDirection * (crushDistance * 0.5f);
        Gizmos.DrawRay(arrowPos, finalDirection * 0.5f);
        
        // Información del target player
        if (targetPlayer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetPlayer.transform.position, 0.3f);
            Gizmos.DrawLine(detectionCenter, targetPlayer.transform.position);
        }
    }
}