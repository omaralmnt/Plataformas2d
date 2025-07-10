using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Score System")]
    [SerializeField] private int currentCoins = 0;
    [SerializeField] private string coinTextPrefix = "Coins: ";
    
    [Header("Audio")]
    [SerializeField] private AudioClip coinCollectSound;
    private AudioSource audioSource;
    
    // Referencia que se actualiza en cada escena
    private Text coinText;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Suscribirse al evento de cambio de escena
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    private void Start()
    {
        FindAndSetupUI();
    }
    
    // Se llama automáticamente cuando se carga una nueva escena
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Resetear coins si es el nivel 1
        if (scene.name == "Nivel1" || scene.name == "Level1") // Ajusta el nombre según tu escena
        {
            currentCoins = 0;
            Debug.Log("Coins reseteados a 0 - Nivel 1 cargado");
        }
        
        FindAndSetupUI();
    }
    
    private void FindAndSetupUI()
    {
        // Obtener el nombre de la escena actual
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        // Si es el menú principal, no buscar UI de score
        if (currentSceneName == "MenuPrincipal")
        {
            coinText = null;
            Debug.Log("Escena MenuPrincipal - No se busca UI de score");
            return;
        }
        
        // Buscar el UI de coins en la escena actual por nombre
        GameObject coinUI = GameObject.Find("score");
        
        if (coinUI != null)
        {
            coinText = coinUI.GetComponent<Text>();
        }
        
        // Actualizar UI después de encontrarla
        if (coinText != null)
        {
            UpdateCoinUI();
            Debug.Log("UI de coins encontrado y actualizado en la escena: " + currentSceneName);
        }
        else
        {
            Debug.LogWarning("No se encontró el GameObject 'Score' en la escena: " + currentSceneName);
        }
    }
    
    private void OnDestroy()
    {
        // Desuscribirse del evento para evitar memory leaks
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    
    public void AddCoins(int amount)
    {
        currentCoins += amount;
        UpdateCoinUI();
        
        if (coinCollectSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(coinCollectSound);
        }
        
        Debug.Log($"Coins collected! Total: {currentCoins}");
    }
    
    public void RemoveCoins(int amount)
    {
        currentCoins = Mathf.Max(0, currentCoins - amount);
        UpdateCoinUI();
    }
    
    public int GetCoins()
    {
        return currentCoins;
    }
    
    public bool HasEnoughCoins(int amount)
    {
        return currentCoins >= amount;
    }
    
    private void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = coinTextPrefix + currentCoins.ToString();
        }
    }
    
    public void ForceUpdateUI()
    {
        UpdateCoinUI();
    }
    
    public void SetCoinTextReference(Text textComponent)
    {
        coinText = textComponent;
        UpdateCoinUI();
    }
}