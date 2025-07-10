using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneName = ""; // Nombre de la escena a cargar
    [SerializeField] private bool useSceneIndex = false; // Si prefieres usar índice en vez de nombre
    [SerializeField] private int sceneIndex = 0; // Índice de la escena
    
    [Header("Optional Settings")]
    [SerializeField] private bool showLoadingLog = true;
    [SerializeField] private float delayBeforeLoad = 0f; // Delay opcional antes de cargar
    
    // Método principal para cargar escena (usar este en el botón)
    public void LoadScene()
    {
        if (delayBeforeLoad > 0)
        {
            StartCoroutine(LoadSceneWithDelay());
        }
        else
        {
            LoadSceneNow();
        }
    }
    
    // Cargar escena específica por nombre (llamar desde otro script)
    public void LoadScene(string sceneNameToLoad)
    {
        if (showLoadingLog)
        {
            Debug.Log($"Loading scene: {sceneNameToLoad}");
        }
        
        SceneManager.LoadScene(sceneNameToLoad);
    }
    
    // Cargar escena específica por índice (llamar desde otro script)
    public void LoadScene(int sceneIndexToLoad)
    {
        if (showLoadingLog)
        {
            Debug.Log($"Loading scene index: {sceneIndexToLoad}");
        }
        
        SceneManager.LoadScene(sceneIndexToLoad);
    }
    
    // Métodos útiles para navegación común
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    
    public void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }
    
    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentIndex + 1);
    }
    
    public void LoadPreviousScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentIndex - 1);
    }
    
    public void QuitGame()
    {
        if (showLoadingLog)
        {
            Debug.Log("Quitting game...");
        }
        
        Application.Quit();
        
        // Para el editor de Unity
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    // Método privado para cargar la escena configurada
    private void LoadSceneNow()
    {
        if (useSceneIndex)
        {
            LoadScene(sceneIndex);
        }
        else
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("Scene name is empty! Please set a scene name.");
                return;
            }
            LoadScene(sceneName);
        }
    }
    
    // Corrutina para delay opcional
    private System.Collections.IEnumerator LoadSceneWithDelay()
    {
        yield return new WaitForSeconds(delayBeforeLoad);
        LoadSceneNow();
    }
}