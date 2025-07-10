using UnityEngine;

public class SpikesTrap : MonoBehaviour
{
    [Header("Spikes Settings")]
    [SerializeField] private float damage = 2f;
    [SerializeField] private float damageInterval = 0.5f; // Tiempo entre daños (para evitar spam)
    
    [Header("Animation Settings")]
    [SerializeField] private bool useAnimation = true;
    [SerializeField] private float animationSpeed = 5f;
    [SerializeField] private AnimationCurve spikeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Trigger Behavior")]
    [SerializeField] private TriggerMode triggerMode = TriggerMode.AlwaysActive;
    [SerializeField] private float activationDelay = 0.2f; // Delay antes de activarse
    [SerializeField] private float activeTime = 2f; // Tiempo que permanecen activos
    [SerializeField] private float cooldownTime = 1f; // Tiempo antes de poder activarse otra vez
    
    [Header("Visual Settings")]
    [SerializeField] private Vector3 hiddenPosition = Vector3.down * 0.5f; // Posición cuando están ocultos
    [SerializeField] private Vector3 activePosition = Vector3.zero; // Posición cuando están activos
    [SerializeField] private bool useScale = false; // Usar escala en lugar de posición
    [SerializeField] private Vector3 hiddenScale = new Vector3(1, 0.1f, 1);
    [SerializeField] private Vector3 activeScale = Vector3.one;
    
    [Header("Audio")]
    [SerializeField] private AudioClip activateSound;
    [SerializeField] private AudioClip deactivateSound;
    [SerializeField] private AudioClip damageSound;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem activateEffect;
    [SerializeField] private GameObject warningIndicator; // Opcional: indicador visual antes de activarse
    
    public enum TriggerMode
    {
        AlwaysActive,      // Siempre hacen daño
        OnPlayerEnter,     // Se activan cuando el player entra
        OnPlayerStay,      // Se activan mientras el player esté encima
        Periodic,          // Se activan automáticamente cada X tiempo
        Manual            // Se activan manualmente desde código
    }
    
    private bool isActive = false;
    private bool isAnimating = false;
    private bool canDamage = true;
    private bool canActivate = true;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Transform spikesVisual;
    private Collider2D trapCollider;
    
    // Para tracking de player
    private bool playerOnTrap = false;
    private Health playerHealth;
    
    private void Awake()
    {
        trapCollider = GetComponent<Collider2D>();
        
        // Buscar el visual de los pinchos (puede ser este mismo objeto o un hijo)
        spikesVisual = transform.childCount > 0 ? transform.GetChild(0) : transform;
        
        originalPosition = spikesVisual.localPosition;
        originalScale = spikesVisual.localScale;
        
        // Ajustar posiciones relativas si no están configuradas
        if (hiddenPosition == Vector3.down * 0.5f)
            hiddenPosition = originalPosition + Vector3.down * 0.5f;
        if (activePosition == Vector3.zero)
            activePosition = originalPosition;
    }
    
    private void Start()
    {
        // Configurar estado inicial según el modo
        switch (triggerMode)
        {
            case TriggerMode.AlwaysActive:
                SetSpikesState(true, false); // Activos sin animación
                break;
            case TriggerMode.Periodic:
                InvokeRepeating(nameof(PeriodicActivation), activationDelay, activeTime + cooldownTime);
                SetSpikesState(false, false);
                break;
            default:
                SetSpikesState(false, false); // Empezar ocultos
                break;
        }
        
        // Ocultar indicador de advertencia al inicio
        if (warningIndicator != null)
            warningIndicator.SetActive(false);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnTrap = true;
            playerHealth = other.GetComponent<Health>();
            
            switch (triggerMode)
            {
                case TriggerMode.OnPlayerEnter:
                    if (canActivate)
                        ActivateSpikes();
                    break;
                case TriggerMode.OnPlayerStay:
                    if (canActivate)
                        ActivateSpikes();
                    break;
            }
            
            // Si ya están activos, hacer daño inmediatamente
            if (isActive && triggerMode == TriggerMode.AlwaysActive)
            {
                DamagePlayer();
            }
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isActive && canDamage)
        {
            DamagePlayer();
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnTrap = false;
            playerHealth = null;
            
            if (triggerMode == TriggerMode.OnPlayerStay)
            {
                DeactivateSpikes();
            }
        }
    }
    
    private void ActivateSpikes()
    {
        if (!canActivate || isActive) return;
        
        canActivate = false;
        
        // Mostrar advertencia si existe
        if (warningIndicator != null)
        {
            warningIndicator.SetActive(true);
        }
        
        // Activar después del delay
        Invoke(nameof(DoActivateSpikes), activationDelay);
    }
    
    private void DoActivateSpikes()
    {
        SetSpikesState(true, useAnimation);
        
        // Reproducir efectos
        if (activateSound != null)
            AudioSource.PlayClipAtPoint(activateSound, transform.position);
        
        if (activateEffect != null)
            activateEffect.Play();
        
        // Ocultar advertencia
        if (warningIndicator != null)
            warningIndicator.SetActive(false);
        
        // Desactivar automáticamente si no es modo AlwaysActive
        if (triggerMode != TriggerMode.AlwaysActive && triggerMode != TriggerMode.OnPlayerStay)
        {
            Invoke(nameof(DeactivateSpikes), activeTime);
        }
        
        Debug.Log("Spikes activated!");
    }
    
    private void DeactivateSpikes()
    {
        if (!isActive) return;
        
        SetSpikesState(false, useAnimation);
        
        // Reproducir sonido
        if (deactivateSound != null)
            AudioSource.PlayClipAtPoint(deactivateSound, transform.position);
        
        // Reiniciar cooldown
        Invoke(nameof(ResetActivation), cooldownTime);
        
        Debug.Log("Spikes deactivated!");
    }
    
    private void SetSpikesState(bool active, bool animate)
    {
        isActive = active;
        
        if (animate && useAnimation)
        {
            StartCoroutine(AnimateSpikes(active));
        }
        else
        {
            // Cambio instantáneo
            if (useScale)
            {
                spikesVisual.localScale = active ? activeScale : hiddenScale;
            }
            else
            {
                spikesVisual.localPosition = active ? activePosition : hiddenPosition;
            }
        }
    }
    
    private System.Collections.IEnumerator AnimateSpikes(bool activating)
    {
        isAnimating = true;
        float timer = 0f;
        float duration = 1f / animationSpeed;
        
        Vector3 startPos = spikesVisual.localPosition;
        Vector3 startScale = spikesVisual.localScale;
        Vector3 targetPos = activating ? activePosition : hiddenPosition;
        Vector3 targetScale = activating ? activeScale : hiddenScale;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = spikeCurve.Evaluate(timer / duration);
            
            if (useScale)
            {
                spikesVisual.localScale = Vector3.Lerp(startScale, targetScale, progress);
            }
            else
            {
                spikesVisual.localPosition = Vector3.Lerp(startPos, targetPos, progress);
            }
            
            yield return null;
        }
        
        // Asegurar posición final
        if (useScale)
            spikesVisual.localScale = targetScale;
        else
            spikesVisual.localPosition = targetPos;
        
        isAnimating = false;
    }
    
    private void DamagePlayer()
    {
        if (playerHealth != null && canDamage)
        {
            playerHealth.TakeDamage(damage);
            canDamage = false;
            
            // Reproducir sonido de daño
            if (damageSound != null)
                AudioSource.PlayClipAtPoint(damageSound, transform.position);
            
            // Reiniciar capacidad de hacer daño
            Invoke(nameof(ResetDamage), damageInterval);
            
            Debug.Log($"Spikes dealt {damage} damage to player!");
        }
    }
    
    private void ResetDamage()
    {
        canDamage = true;
    }
    
    private void ResetActivation()
    {
        canActivate = true;
    }
    
    private void PeriodicActivation()
    {
        if (canActivate)
            ActivateSpikes();
    }
    
    // Métodos públicos para control manual
    public void ManualActivate()
    {
        if (triggerMode == TriggerMode.Manual)
            ActivateSpikes();
    }
    
    public void ManualDeactivate()
    {
        if (triggerMode == TriggerMode.Manual)
            DeactivateSpikes();
    }
    
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
    
    // Debug visual
    private void OnDrawGizmosSelected()
    {
        if (spikesVisual == null) return;
        
        // Dibujar posición activa
        Gizmos.color = Color.red;
        Vector3 activeWorldPos = transform.position + activePosition;
        Gizmos.DrawWireCube(activeWorldPos, Vector3.one * 0.5f);
        
        // Dibujar posición oculta
        Gizmos.color = Color.gray;
        Vector3 hiddenWorldPos = transform.position + hiddenPosition;
        Gizmos.DrawWireCube(hiddenWorldPos, Vector3.one * 0.3f);
        
        // Línea entre posiciones
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(hiddenWorldPos, activeWorldPos);
    }
}