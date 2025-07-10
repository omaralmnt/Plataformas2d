using UnityEngine;
using System.Collections;

public class FallingTile : MonoBehaviour
{
    [Header("Fall Settings")]
    [SerializeField] private float fallDelay = 0.5f; // Tiempo antes de caer
    [SerializeField] private float respawnTime = 3f; // Tiempo para reaparecer (0 = no reaparece)
    [SerializeField] private bool destroyOnFall = false; // Si se destruye al caer
    
    [Header("Warning")]
    [SerializeField] private float shakeTime = 0.3f; // Tiempo de temblor antes de caer
    [SerializeField] private float shakeIntensity = 0.05f; // Intensidad del temblor
    [SerializeField] private Color warningColor = Color.red; // Color de advertencia
    
    [Header("Audio")]
    [SerializeField] private AudioClip stepSound; // Sonido al pisar
    [SerializeField] private AudioClip crackSound; // Sonido de grieta/advertencia
    [SerializeField] private AudioClip fallSound; // Sonido al caer
    [SerializeField] private AudioClip respawnSound; // Sonido al reaparecer
    
    [Header("Physics")]
    [SerializeField] private float fallGravity = 2f; // Gravedad al caer
    [SerializeField] private LayerMask groundLayer = 1; // Capas que detienen la caída
    
    private Vector3 originalPosition;
    private Color originalColor;
    private bool isTriggered = false;
    private bool isFalling = false;
    private bool hasRespawned = true;
    
    private Rigidbody2D rb;
    private Collider2D tileCollider;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    
    private void Awake()
    {
        // Obtener componentes
        rb = GetComponent<Rigidbody2D>();
        tileCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        // Crear componentes si no existen
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Configurar Rigidbody2D
        rb.bodyType = RigidbodyType2D.Kinematic; // Empieza como kinematic
        rb.gravityScale = fallGravity;
        
        // Guardar valores originales
        originalPosition = transform.position;
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Solo activar si es el jugador y la tile no ha sido activada
        if (other.CompareTag("Player") && !isTriggered && hasRespawned)
        {
            TriggerFall();
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // También puede activarse al caminar encima (con Collider normal)
        if (collision.gameObject.CompareTag("Player") && !isTriggered && hasRespawned)
        {
            TriggerFall();
        }
        
        // Detectar cuando la tile toca el suelo después de caer
        if (isFalling && IsGroundLayer(collision.gameObject.layer))
        {
            OnHitGround();
        }
    }
    
    public void TriggerFall()
    {
        if (isTriggered) return;
        
        isTriggered = true;
        StartCoroutine(FallSequence());
    }
    
    private IEnumerator FallSequence()
    {
        // Sonido al pisar
        PlaySound(stepSound);
        
        // Fase de advertencia (temblor y cambio de color)
        yield return StartCoroutine(WarningPhase());
        
        // Esperar el delay antes de caer
        yield return new WaitForSeconds(fallDelay);
        
        // Hacer que la tile caiga
        StartFalling();
        
        // Esperar a que termine de caer o que pase el tiempo de respawn
        if (respawnTime > 0 && !destroyOnFall)
        {
            yield return new WaitForSeconds(respawnTime);
            RespawnTile();
        }
        else if (destroyOnFall)
        {
            // Esperar un poco antes de destruir (para que se vea la caída)
            yield return new WaitForSeconds(2f);
            Destroy(gameObject);
        }
    }
    
    private IEnumerator WarningPhase()
    {
        // Sonido de grieta/advertencia
        PlaySound(crackSound);
        
        Vector3 originalPos = transform.position;
        Color currentColor = originalColor;
        
        float timer = 0f;
        while (timer < shakeTime)
        {
            // Efecto de temblor
            float shakeX = Random.Range(-shakeIntensity, shakeIntensity);
            float shakeY = Random.Range(-shakeIntensity, shakeIntensity);
            transform.position = originalPos + new Vector3(shakeX, shakeY, 0);
            
            // Efecto de parpadeo de color
            float colorProgress = Mathf.PingPong(timer * 8, 1); // Parpadea rápido
            Color lerpedColor = Color.Lerp(originalColor, warningColor, colorProgress);
            
            if (spriteRenderer != null)
                spriteRenderer.color = lerpedColor;
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        // Restaurar posición y color
        transform.position = originalPos;
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }
    
    private void StartFalling()
    {
        // Cambiar a Dynamic para que caiga
        rb.bodyType = RigidbodyType2D.Dynamic;
        isFalling = true;
        hasRespawned = false;
        
        // Sonido de caída
        PlaySound(fallSound);
        
    }
    
    private void OnHitGround()
    {
        // Detener la caída
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;
        isFalling = false;
        
    }
    
    private void RespawnTile()
    {
        // Resetear estado
        isTriggered = false;
        isFalling = false;
        hasRespawned = true;
        
        // Volver a la posición original
        transform.position = originalPosition;
        
        // Configurar como kinematic de nuevo
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.velocity = Vector2.zero;
        
        // Restaurar color original
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
        
        // Reactivar collider si estaba desactivado
        if (tileCollider != null)
            tileCollider.enabled = true;
        
        // Sonido de respawn
        PlaySound(respawnSound);
        
      
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    private bool IsGroundLayer(int layer)
    {
        return (groundLayer.value & (1 << layer)) != 0;
    }
    
    // Método para activar manualmente desde otros scripts
    public void TriggerFallManually()
    {
        TriggerFall();
    }
    
    // Método para resetear manualmente
    public void ResetTile()
    {
        StopAllCoroutines();
        RespawnTile();
    }
    
    // Configuración en runtime
    public void SetFallDelay(float delay)
    {
        fallDelay = delay;
    }
    
    public void SetRespawnTime(float time)
    {
        respawnTime = time;
    }
    
    private void OnDrawGizmosSelected()
    {
        // Dibujar posición original
        Gizmos.color = Color.green;
        Vector3 pos = Application.isPlaying ? originalPosition : transform.position;
        Gizmos.DrawWireCube(pos, transform.localScale);
        
        // Dibujar dirección de caída
        Gizmos.color = Color.red;
        Gizmos.DrawRay(pos, Vector3.down * 3f);
    }
}