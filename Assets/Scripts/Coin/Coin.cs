using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private int coinValue = 1;
    [SerializeField] private AudioClip collectSound; // Opcional: sonido al recoger
    
    [Header("Animation")]
    [SerializeField] private float floatAmplitude = 0.5f;
    [SerializeField] private float floatSpeed = 2f;
    
    private Vector3 startPosition;
    private bool isCollected = false;
    
    private void Start()
    {
        startPosition = transform.position;
    }
    
    private void Update()
    {
        // Movimiento flotante opcional (si no tienes animación)
        if (!isCollected)
        {
            FloatAnimation();
        }
    }
    
    private void FloatAnimation()
    {
        // Movimiento suave arriba y abajo
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            CollectCoin(other.gameObject);
        }
    }
    
    private void CollectCoin(GameObject player)
    {
        isCollected = true;
        
        Debug.Log("Coin collected!");
        
        // Agregar puntos al GameManager
        GameManager.Instance.AddCoins(coinValue);
        
        // Reproducir sonido usando PlayClipAtPoint (no necesita AudioSource)
        if (collectSound != null)
        {
            Debug.Log("Playing coin sound with PlayClipAtPoint!");
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        else
        {
            Debug.Log("collectSound is null - assign an AudioClip!");
        }
        
        // Opcional: Efecto visual antes de destruir
        StartCoroutine(CollectEffect());
    }
    
    private System.Collections.IEnumerator CollectEffect()
    {
        // Animación rápida de recogida
        float collectTime = 0.2f;
        float timer = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 startPos = transform.position;
        
        while (timer < collectTime)
        {
            timer += Time.deltaTime;
            float progress = timer / collectTime;
            
            // Escalar hacia arriba y luego desaparecer
            transform.localScale = Vector3.Lerp(startScale, startScale * 1.2f, progress);
            transform.position = Vector3.Lerp(startPos, startPos + Vector3.up * 0.5f, progress);
            
            yield return null;
        }
        
        // Destruir la moneda
        Destroy(gameObject);
    }
}