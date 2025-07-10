using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Follow Player Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -10);
    
    [Header("Look Ahead")]
    [SerializeField] private bool useLookAhead = true;
    [SerializeField] private float lookAheadDistance = 2f;
    [SerializeField] private float lookAheadSpeed = 2f;
    
    [Header("Wide View Settings")]
    [SerializeField] private bool useWideView = false;
    [SerializeField] private float zoomOutDistance = 3f; // Qué tan lejos hacer zoom out
    [SerializeField] private float verticalExpansion = 2f; // Expansión vertical adicional
    [SerializeField] private float horizontalExpansion = 4f; // Expansión horizontal adicional
    
    private float currentLookAhead;
    private Camera cameraComponent;
    private float originalOrthographicSize;
    
    private void Start()
    {
        cameraComponent = GetComponent<Camera>();
        if (cameraComponent != null)
        {
            originalOrthographicSize = cameraComponent.orthographicSize;
        }
    }
    
    private void LateUpdate()
    {
        if (player == null) return;
        
        // Calcular look ahead basado en la dirección del jugador
        if (useLookAhead)
        {
            float targetLookAhead = lookAheadDistance * Mathf.Sign(player.localScale.x);
            currentLookAhead = Mathf.Lerp(currentLookAhead, targetLookAhead, Time.deltaTime * lookAheadSpeed);
        }
        else
        {
            currentLookAhead = 0f;
        }
        
        // Calcular offset adicional para visión amplia
        Vector3 wideViewOffset = Vector3.zero;
        if (useWideView)
        {
            // Expandir la vista horizontalmente basado en la dirección del player
            float horizontalOffset = currentLookAhead * (horizontalExpansion / lookAheadDistance);
            wideViewOffset = new Vector3(horizontalOffset, verticalExpansion, -zoomOutDistance);
            
            // Ajustar el zoom de la cámara para visión más amplia
            if (cameraComponent != null)
            {
                float targetSize = originalOrthographicSize + zoomOutDistance;
                cameraComponent.orthographicSize = Mathf.Lerp(cameraComponent.orthographicSize, targetSize, Time.deltaTime * smoothSpeed);
            }
        }
        else
        {
            // Restaurar zoom original
            if (cameraComponent != null)
            {
                cameraComponent.orthographicSize = Mathf.Lerp(cameraComponent.orthographicSize, originalOrthographicSize, Time.deltaTime * smoothSpeed);
            }
        }
        
        // Posición objetivo de la cámara
        Vector3 targetPosition = player.position + offset + new Vector3(currentLookAhead, 0, 0) + wideViewOffset;
        
        // Suavizar el movimiento de la cámara
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
        
        // Aplicar la nueva posición
        transform.position = smoothedPosition;
    }
    
    // Métodos públicos para cambiar la vista en runtime
    public void EnableWideView()
    {
        useWideView = true;
    }
    
    public void DisableWideView()
    {
        useWideView = false;
    }
    
    public void ToggleWideView()
    {
        useWideView = !useWideView;
    }
    
    public void SetWideViewDistance(float distance)
    {
        zoomOutDistance = distance;
    }
}