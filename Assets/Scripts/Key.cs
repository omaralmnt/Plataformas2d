using UnityEngine;

public class Key : MonoBehaviour
{
    [Header("Key Settings")]
    [SerializeField] private string nextLevelName = "Level2"; // Nombre de la siguiente escena
    
    [Header("Audio")]
    [SerializeField] private AudioClip keyCollectSound;
    
    [Header("Visual Effects")]
    [SerializeField] private float collectEffectDuration = 1f;
    [SerializeField] private float floatAmplitude = 0.3f;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float rotateSpeed = 90f; // Grados por segundo
    
    private Vector3 startPosition;
    private bool isCollected = false;
    
    private void Start()
    {
        startPosition = transform.position;
    }
    
    private void Update()
    {
        if (!isCollected)
        {
            // Animación flotante y rotación
            AnimateKey();
        }
    }
    
    private void AnimateKey()
    {
        // Movimiento flotante arriba y abajo
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        
        // Rotación suave
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            CollectKey();
        }
    }
    
    private void CollectKey()
    {
        isCollected = true;
        
        Debug.Log($"Key collected! Advancing to level: {nextLevelName}");
        
        // Reproducir sonido
        if (keyCollectSound != null)
        {
            AudioSource.PlayClipAtPoint(keyCollectSound, transform.position);
        }
        
        // Efecto visual y cambio de nivel
        StartCoroutine(CollectAndAdvanceLevel());
    }
    
    private System.Collections.IEnumerator CollectAndAdvanceLevel()
    {
        float timer = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 startPos = transform.position;
        
        // Efecto visual de recogida
        while (timer < collectEffectDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / collectEffectDuration;
            
            // Escalar hacia arriba y mover hacia arriba
            transform.localScale = Vector3.Lerp(startScale, startScale * 1.5f, progress);
            transform.position = Vector3.Lerp(startPos, startPos + Vector3.up * 1f, progress);
            
            // Fade out si tiene SpriteRenderer
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Lerp(1f, 0f, progress);
                spriteRenderer.color = color;
            }
            
            yield return null;
        }
        
        // Avanzar al siguiente nivel
        LevelManager.Instance.LoadNextLevel(nextLevelName);
    }
}