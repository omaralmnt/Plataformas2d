using UnityEngine;
using System.Collections;

public class LaserTrap : MonoBehaviour
{
    [Header("Laser Settings")]
    [SerializeField] private LineRenderer laserLine; // Para dibujar el laser
    [SerializeField] private GameObject laserEffect; // Efecto visual en el punto final (opcional)
    [SerializeField] private Transform laserStartPoint; // Punto donde inicia el laser
    [SerializeField] private float laserActiveTime = 2f; // Tiempo que el laser está activo
    [SerializeField] private float laserInactiveTime = 3f; // Tiempo que el laser está inactivo
    [SerializeField] private float laserRange = 10f; // Distancia que alcanza el laser
    [SerializeField] private int damage = 1;
    [SerializeField] private LayerMask obstacleLayer = 1; // Qué capas bloquean el laser
    
    [Header("Manual Collider")]
    [SerializeField] private Collider2D manualLaserCollider; // Collider configurado manualmente
    
    [Header("Direction")]
    [SerializeField] private Vector2 laserDirection = Vector2.right; // Dirección del laser
    
    [Header("Visual")]
    [SerializeField] private Color laserColor = Color.red;
    [SerializeField] private float laserWidth = 0.1f;
    [SerializeField] private Material laserMaterial; // Material del laser (opcional)
    
    [Header("Audio")]
    [SerializeField] private AudioClip laserSound;
    [SerializeField] private AudioClip warningSound; // Sonido de advertencia antes de disparar
    [SerializeField] private AudioClip chargingSound; // Sonido de carga
    
    [Header("Warning")]
    [SerializeField] private float warningTime = 1f; // Tiempo de advertencia antes de disparar
    [SerializeField] private GameObject warningLight; // Luz de advertencia (opcional)
    
    private AudioSource audioSource;
    private bool isActive = false;
    private Vector3 laserEndPoint;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Si no hay AudioSource, crearlo
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 1f;
          
        }
        
        // Si no hay un start point asignado, usar la posición de este objeto
        if (laserStartPoint == null)
            laserStartPoint = transform;
        
        // Normalizar la dirección
        laserDirection = laserDirection.normalized;
        
        // Configurar LineRenderer si existe
        SetupLineRenderer();
        
        // Configurar el collider manual si existe
        if (manualLaserCollider != null)
        {
            manualLaserCollider.isTrigger = true;
            manualLaserCollider.enabled = false;
            
            // Agregar el componente que detecta colisiones
            LaserDamage laserDamage = manualLaserCollider.gameObject.GetComponent<LaserDamage>();
            if (laserDamage == null)
            {
                laserDamage = manualLaserCollider.gameObject.AddComponent<LaserDamage>();
            }
            laserDamage.Initialize(this);
        }
        else
        {
  
        }
    }
    
    private void SetupLineRenderer()
    {
        if (laserLine == null)
        {
            // Crear LineRenderer si no existe
            GameObject laserObject = new GameObject("LaserLine");
            laserObject.transform.SetParent(transform);
            laserLine = laserObject.AddComponent<LineRenderer>();
        }
        
        // Crear material si no existe
        if (laserMaterial == null)
        {
            laserMaterial = new Material(Shader.Find("Sprites/Default"));
        }
        
        laserLine.material = laserMaterial;
        laserLine.material.color = laserColor;
        laserLine.startWidth = laserWidth;
        laserLine.endWidth = laserWidth;
        laserLine.positionCount = 2;
        laserLine.useWorldSpace = true;
        laserLine.enabled = false;
        
        // Hacer que el laser se renderice por encima de otros objetos
        laserLine.sortingOrder = 10;
    }
    
    private void Start()
    {
        // Comenzar el ciclo de la trampa
        StartCoroutine(LaserCycle());
    }
    
    private IEnumerator LaserCycle()
    {
        while (true)
        {
            // Esperar tiempo inactivo
            yield return new WaitForSeconds(laserInactiveTime);
            
            // Fase de advertencia
            yield return StartCoroutine(WarningPhase());
            
            // Activar laser
            yield return StartCoroutine(ActivateLaser());
        }
    }
    
    private IEnumerator WarningPhase()
    {
        // Activar luz de advertencia
        if (warningLight != null)
            warningLight.SetActive(true);
        
        // Reproducir sonido de advertencia
        if (warningSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(warningSound);
        }
     
        
        // Mostrar laser parpadeante como advertencia
        for (float timer = 0; timer < warningTime; timer += 0.2f)
        {
            CalculateLaserPath();
            laserLine.enabled = true;
            laserLine.material.color = laserColor * 0.3f; // Laser tenue
            yield return new WaitForSeconds(0.1f);
            laserLine.enabled = false;
            yield return new WaitForSeconds(0.1f);
        }
        
        // Desactivar luz de advertencia
        if (warningLight != null)
            warningLight.SetActive(false);
    }
    
    private IEnumerator ActivateLaser()
    {
        isActive = true;
        
        // Reproducir sonido de carga
        if (chargingSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(chargingSound);
        
        }
    
        // Calcular el camino del laser
        CalculateLaserPath();
        
        // Activar visual del laser
        laserLine.enabled = true;
        laserLine.material.color = laserColor; // Color completo
        
        // Activar collider del laser
        if (manualLaserCollider != null)
            manualLaserCollider.enabled = true;
        
        
        // Activar efecto en el punto final
        if (laserEffect != null)
        {
            GameObject effect = Instantiate(laserEffect, laserEndPoint, Quaternion.identity);
            Destroy(effect, laserActiveTime);
        }
        
        // Reproducir sonido de laser
        if (laserSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(laserSound);
          
        }
   
        
        // Mantener el laser activo
        yield return new WaitForSeconds(laserActiveTime);
        
        // Desactivar el laser
        laserLine.enabled = false;
        if (manualLaserCollider != null)
            manualLaserCollider.enabled = false;
        isActive = false;
        
    
    }
    
    private void CalculateLaserPath()
    {
        Vector3 startPos = laserStartPoint.position;
        Vector3 direction = laserDirection;
        
        // Hacer raycast para ver si el laser golpea algo
        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, laserRange, obstacleLayer);
        
        if (hit.collider != null)
        {
            laserEndPoint = hit.point;
        }
        else
        {
            laserEndPoint = startPos + (Vector3)(direction * laserRange);
        }
        
        // Actualizar posiciones del LineRenderer
        laserLine.SetPosition(0, startPos);
        laserLine.SetPosition(1, laserEndPoint);
    }
    
    public void DealDamage(Collider2D target)
    {
        // Verificaciones de seguridad
        if (!isActive)
        {
          
            return;
        }
        
        if (!laserLine.enabled)
        {
            return;
        }
        
        if (target.CompareTag("Player"))
        {
            
            Health playerHealth = target.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
    
    // Método para activar manualmente la trampa
    public void TriggerLaser()
    {
        if (!isActive)
        {
            StopAllCoroutines();
            StartCoroutine(ActivateLaser());
            StartCoroutine(LaserCycle());
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Dibujar la dirección y rango del laser en el editor
        Gizmos.color = Color.red;
        Vector3 startPos = laserStartPoint != null ? laserStartPoint.position : transform.position;
        Vector3 endPos = startPos + (Vector3)(laserDirection * laserRange);
        
        Gizmos.DrawLine(startPos, endPos);
        Gizmos.DrawWireSphere(endPos, 0.2f);
    }
}

// Clase auxiliar para manejar las colisiones del laser
public class LaserDamage : MonoBehaviour
{
    private LaserTrap parentTrap;
    
    public void Initialize(LaserTrap trap)
    {
        parentTrap = trap;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (parentTrap != null)
            parentTrap.DealDamage(other);
    }
}