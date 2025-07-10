using UnityEngine;
using System.Collections;

public class LavaDamageZone : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damagePerSecond = 1;
    [SerializeField] private float damageInterval = 1f; // Cada cuánto hace daño
    
    [Header("Player Effect")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float colorIntensity = 0.5f; // Qué tan rojo se pone
    [SerializeField] private float flashSpeed = 5f; // Velocidad del parpadeo
    
    [Header("Audio")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip enterLavaSound;
    [SerializeField] private AudioClip exitLavaSound;
    
    private bool playerInLava = false;
    private GameObject currentPlayer = null;
    private SpriteRenderer playerSprite = null;
    private Color originalPlayerColor;
    private Health playerHealth = null;
    private AudioSource audioSource;
    
    private void Awake()
    {
        // Crear AudioSource si no existe
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Asegurar que el collider sea trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogWarning("LavaDamageZone needs a Collider2D marked as Trigger!");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered lava zone!");
            
            // Configurar referencias del jugador
            currentPlayer = other.gameObject;
            playerSprite = currentPlayer.GetComponent<SpriteRenderer>();
            playerHealth = currentPlayer.GetComponent<Health>();
            
            if (playerSprite != null)
            {
                originalPlayerColor = playerSprite.color;
            }
            
            // Activar efecto de lava
            playerInLava = true;
            
            // Reproducir sonido de entrada
            if (enterLavaSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(enterLavaSound);
            }
            
            // Comenzar corrutina de daño
            StartCoroutine(LavaDamageCoroutine());
            
            // Comenzar efecto visual
            if (playerSprite != null)
            {
                StartCoroutine(DamageColorEffect());
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && playerInLava)
        {
            Debug.Log("Player exited lava zone!");
            
            // Desactivar efecto de lava
            playerInLava = false;
            
            // Restaurar color original
            if (playerSprite != null)
            {
                playerSprite.color = originalPlayerColor;
            }
            
            // Reproducir sonido de salida
            if (exitLavaSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(exitLavaSound);
            }
            
            // Limpiar referencias
            currentPlayer = null;
            playerSprite = null;
            playerHealth = null;
        }
    }
    
    private IEnumerator LavaDamageCoroutine()
    {
        while (playerInLava)
        {
            // Hacer daño al jugador
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damagePerSecond);
                Debug.Log($"Lava dealt {damagePerSecond} damage to player");
                
                // Reproducir sonido de daño
                if (damageSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(damageSound);
                }
            }
            
            // Esperar el intervalo antes del siguiente daño
            yield return new WaitForSeconds(damageInterval);
        }
    }
    
    private IEnumerator DamageColorEffect()
    {
        while (playerInLava && playerSprite != null)
        {
            // Calcular color con efecto pulsante
            float colorLerp = (Mathf.Sin(Time.time * flashSpeed) + 1f) * 0.5f; // Oscila entre 0 y 1
            Color targetColor = Color.Lerp(originalPlayerColor, damageColor, colorIntensity * colorLerp);
            
            // Aplicar color al jugador
            playerSprite.color = targetColor;
            
            yield return null; // Esperar un frame
        }
        
        // Restaurar color original cuando salga
        if (playerSprite != null)
        {
            playerSprite.color = originalPlayerColor;
        }
    }
    
    // Método para configurar en runtime
    public void SetDamagePerSecond(int newDamage)
    {
        damagePerSecond = newDamage;
    }
    
    public void SetDamageInterval(float newInterval)
    {
        damageInterval = newInterval;
    }
    
    // Método para forzar al jugador a salir (útil para testing)
    public void ForcePlayerExit()
    {
        if (playerInLava)
        {
            playerInLava = false;
            if (playerSprite != null)
            {
                playerSprite.color = originalPlayerColor;
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Dibujar área de daño
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(1, 0.5f, 0, 0.3f); // Naranja transparente
            Gizmos.DrawCube(transform.position, col.bounds.size);
            
            // Borde rojo para indicar peligro
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
    }
}