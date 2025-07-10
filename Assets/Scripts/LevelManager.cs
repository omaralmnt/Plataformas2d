using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    
    [Header("Level Complete UI")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private Text levelCompleteText;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button restartButton;
    
    [Header("Transition Settings")]
    [SerializeField] private float transitionDelay = 2f;
    [SerializeField] private bool showLevelCompleteUI = true;
    
    [Header("Audio")]
    [SerializeField] private AudioClip levelCompleteSound;
    
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
        
        // Ocultar panel al inicio
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
        
        SetupButtons();
    }
    
    private void SetupButtons()
    {
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(() => {
                if (!string.IsNullOrEmpty(pendingLevelName))
                {
                    SceneManager.LoadScene(pendingLevelName);
                }
            });
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(() => {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            });
        }
    }
    
    private string pendingLevelName;
    
    public void LoadNextLevel(string levelName)
    {
        pendingLevelName = levelName;
        StartCoroutine(LevelCompleteSequence(levelName));
    }
    
    private IEnumerator LevelCompleteSequence(string levelName)
    {
        Debug.Log($"Level Complete! Loading: {levelName}");
        
        // Reproducir sonido de nivel completado
        if (levelCompleteSound != null)
        {
            AudioSource.PlayClipAtPoint(levelCompleteSound, Camera.main.transform.position);
        }
        
        if (showLevelCompleteUI && levelCompletePanel != null)
        {
            // Mostrar UI de nivel completado
            levelCompletePanel.SetActive(true);
            
            if (levelCompleteText != null)
            {
                levelCompleteText.text = $"¡Nivel Completado!\nAvanzando a {levelName}";
            }
            
            // Esperar un poco más si se muestra UI
            yield return new WaitForSeconds(transitionDelay + 1f);
        }
        else
        {
            // Transición directa
            yield return new WaitForSeconds(transitionDelay);
        }
        
        // Cargar siguiente nivel
        if (Application.CanStreamedLevelBeLoaded(levelName))
        {
            SceneManager.LoadScene(levelName);
        }
        else
        {
            Debug.LogError($"Scene '{levelName}' not found! Make sure it's added to Build Settings.");
            
            // Mostrar mensaje de error
            if (levelCompleteText != null)
            {
                levelCompleteText.text = $"Error: Nivel '{levelName}' no encontrado!\nVerifica Build Settings.";
            }
        }
    }
    
    // Método alternativo para cambio directo sin UI
    public void LoadLevelDirectly(string levelName)
    {
        if (Application.CanStreamedLevelBeLoaded(levelName))
        {
            SceneManager.LoadScene(levelName);
        }
        else
        {
            Debug.LogError($"Scene '{levelName}' not found!");
        }
    }
    
    // Método para reiniciar nivel actual
    public void RestartCurrentLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    // Método para ir al siguiente nivel numérico (Level1 -> Level2, etc.)
    public void LoadNextNumericalLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        // Intentar extraer número del nombre actual
        if (currentSceneName.StartsWith("Nivel"))
        {
            string numberPart = currentSceneName.Substring(5); // Después de "Level"
            if (int.TryParse(numberPart, out int currentLevel))
            {
                string nextLevelName = "Nivel" + (currentLevel + 1);
                LoadNextLevel(nextLevelName);
                return;
            }
        }
        
        Debug.LogWarning("Could not determine next level from current scene name: " + currentSceneName);
    }
}