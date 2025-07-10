using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;
    
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text gameOverText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    
    [Header("UI Elements to Hide")]
    [SerializeField] private List<GameObject> uiElementsToHide = new List<GameObject>();
    [SerializeField] private bool autoFindUIElements = true; // Encontrar automáticamente elementos de UI
    
    [Header("Settings")]
    [SerializeField] private string level1SceneName = "Level1"; // Nombre de tu escena del nivel 1
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Opcional: menú principal
    [SerializeField] private float delayBeforeGameOver = 2f; // Tiempo antes de mostrar Game Over
    
    [Header("Audio")]
    [SerializeField] private AudioClip gameOverMusic;
    
    private List<GameObject> hiddenUIElements = new List<GameObject>();
    
    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        Debug.Log("GameOverManager Awake called");
        
        // Auto-encontrar elementos de UI si está habilitado
        if (autoFindUIElements)
        {
            FindUIElementsToHide();
        }
        
        // Asegurar que el panel esté oculto al inicio
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            Debug.Log("GameOver panel found and hidden");
        }
        else
        {
            Debug.LogError("GameOverPanel is not assigned! Drag it from your Canvas to the GameOverManager in inspector");
        }
        
        // Configurar botones
        SetupButtons();
    }
    
    private void FindUIElementsToHide()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            // Buscar todos los elementos hijos del Canvas excepto el GameOverPanel
            for (int i = 0; i < canvas.transform.childCount; i++)
            {
                GameObject child = canvas.transform.GetChild(i).gameObject;
                
                // No agregar el GameOverPanel a la lista
                if (child != gameOverPanel && child.name != "GameOverPanel")
                {
                    uiElementsToHide.Add(child);
                    Debug.Log($"Found UI element to hide: {child.name}");
                }
            }
        }
    }
    
    private void SetupButtons()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartLevel);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
    }
    
    public void TriggerGameOver()
    {
        Debug.Log("TriggerGameOver called!");
        StartCoroutine(GameOverSequence());
    }
    
    private IEnumerator GameOverSequence()
    {
        Debug.Log("GameOverSequence started");
        
        // Esperar un poco antes de mostrar Game Over
        yield return new WaitForSeconds(delayBeforeGameOver);
        
        Debug.Log("About to show Game Over UI");
        
        // Ocultar otros elementos de UI
        HideOtherUIElements();
        
        // Pausar el juego
        Time.timeScale = 0f;
        Debug.Log("Time.timeScale set to 0");
        
        // Mostrar panel de Game Over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("GameOver panel activated");
        }
        else
        {
            Debug.LogError("gameOverPanel is NULL!");
        }
        
        // Reproducir música de Game Over
        if (gameOverMusic != null)
        {
            AudioSource.PlayClipAtPoint(gameOverMusic, Camera.main.transform.position);
        }
        
        // Configurar texto
        if (gameOverText != null)
        {
            gameOverText.text = "El jugador murió, perdiste!";
            Debug.Log("GameOver text set");
        }
        else
        {
            Debug.LogError("gameOverText is NULL!");
        }
    }
    
    private void HideOtherUIElements()
    {
        hiddenUIElements.Clear();
        
        foreach (GameObject uiElement in uiElementsToHide)
        {
            if (uiElement != null && uiElement.activeInHierarchy)
            {
                uiElement.SetActive(false);
                hiddenUIElements.Add(uiElement);
                Debug.Log($"Hidden UI element: {uiElement.name}");
            }
        }
    }
    
    private void ShowOtherUIElements()
    {
        foreach (GameObject uiElement in hiddenUIElements)
        {
            if (uiElement != null)
            {
                uiElement.SetActive(true);
                Debug.Log($"Restored UI element: {uiElement.name}");
            }
        }
        hiddenUIElements.Clear();
    }
    
    public void RestartLevel()
    {
        // Restaurar elementos de UI antes de cambiar escena
        ShowOtherUIElements();
        
        // Restaurar tiempo normal
        Time.timeScale = 1f;
        
        // Cargar nivel 1
        SceneManager.LoadScene(level1SceneName);
    }
    
    public void GoToMainMenu()
    {
        // Restaurar elementos de UI antes de cambiar escena
        ShowOtherUIElements();
        
        // Restaurar tiempo normal
        Time.timeScale = 1f;
        
        // Cargar menú principal
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            // Si no hay menú principal, reiniciar nivel
            RestartLevel();
        }
    }
    
    // Método alternativo para reiniciar rápido con tecla
    private void Update()
    {
        if (gameOverPanel != null && gameOverPanel.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartLevel();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GoToMainMenu();
            }
        }
    }
}