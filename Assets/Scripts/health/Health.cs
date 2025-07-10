using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float startingHealth;
    public float currentHealth { get; private set; }
    
    [Header("Audio")]
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip dieSound;
    
    [Header("Drop on Death")]
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private string nextLevelName;
    
    private Animator anim;
    private bool dead;

    private void Awake()
    {
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();
    }
    
    public void TakeDamage(float _damage)
    {
        if (dead) return;
        
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);

        if (currentHealth > 0)
        {
            // Entidad herida pero viva
            anim.SetTrigger("hurt");
            
            // Reproducir sonido de daño
            if (hurtSound != null)
            {
                AudioSource.PlayClipAtPoint(hurtSound, transform.position);
            }
        }
        else
        {
            // Entidad muerta
            if (!dead)
            {
                dead = true;
                anim.SetTrigger("die");
                
                // Reproducir sonido de muerte
                if (dieSound != null)
                {
                    AudioSource.PlayClipAtPoint(dieSound, transform.position);
                }
                
                // Si es el player, activar Game Over
                if (gameObject.CompareTag("Player"))
                {
                    PlayerMovement playerMovement = GetComponent<PlayerMovement>();
                    if (playerMovement != null)
                        playerMovement.enabled = false;
                    
                    if (GameOverManager.Instance != null)
                    {
                        GameOverManager.Instance.TriggerGameOver();
                    }
                }
                else
                {
                    // Si es enemigo, instanciar llave donde murió
                    if (keyPrefab != null)
                    {
                        GameObject keyInstance = Instantiate(keyPrefab, transform.position, Quaternion.identity);
                        
                        // Buscar el componente de la llave y asignar el siguiente nivel
                        if (!string.IsNullOrEmpty(nextLevelName))
                        {
                            var keyComponent = keyInstance.GetComponent<Key>();
                            if (keyComponent != null)
                            {
                                // Usar reflection para acceder al campo privado nextLevelName
                                var field = typeof(Key).GetField("nextLevelName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                if (field != null)
                                {
                                    field.SetValue(keyComponent, nextLevelName);
                                }
                            }
                        }
                    }
                    
                    // Desactivar después de un pequeño delay para que se vea la animación
                    StartCoroutine(DeactivateAfterDelay());
                }
            }
        }
    }
    
    private IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(0.5f); // Espera medio segundo para que se vea la animación de muerte
        gameObject.SetActive(false);
    }
    
    public void AddHealth(float _value)
    {
        currentHealth = Mathf.Clamp(currentHealth + _value, 0, startingHealth);
    }
}