using UnityEngine;

public class HealthCollectible : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float healthValue;
    
    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;
    
    [Header("Visual Effect (Optional)")]
    [SerializeField] private float collectEffectDuration = 0.3f;
    
    private bool isCollected = false;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isCollected)
        {
            CollectHealth(collision);
        }
    }
    
    private void CollectHealth(Collider2D player)
    {
        isCollected = true;
        
        // Agregar salud al jugador
        Health playerHealth = player.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.AddHealth(healthValue);
            Debug.Log($"Health collected! Added {healthValue} health.");
        }
        else
        {
            Debug.LogWarning("Player doesn't have Health component!");
        }
        
        // Reproducir sonido
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        
        // Efecto visual opcional antes de desactivar
        if (collectEffectDuration > 0)
        {
            StartCoroutine(CollectEffect());
        }
        else
        {
            // Desactivar inmediatamente
            gameObject.SetActive(false);
        }
    }
    
    private System.Collections.IEnumerator CollectEffect()
    {
        float timer = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 startPosition = transform.position;
        
        while (timer < collectEffectDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / collectEffectDuration;
            
            // Efecto de escala y movimiento hacia arriba
            transform.localScale = Vector3.Lerp(startScale, startScale * 1.3f, progress);
            transform.position = Vector3.Lerp(startPosition, startPosition + Vector3.up * 0.5f, progress);
            
            // Fade out (si tienes SpriteRenderer)
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Lerp(1f, 0f, progress);
                spriteRenderer.color = color;
            }
            
            yield return null;
        }
        
        // Desactivar después del efecto
        gameObject.SetActive(false);
    }
}