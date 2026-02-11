using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMusicController : MonoBehaviour
{
    private static SceneMusicController instance;
    private AudioSource audioSource;

    void Awake()
    {
        // Singleton: prevent duplicates
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("No AudioSource found on this GameObject!");
        }

        // Subscribe to scene load events
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Initialize music state based on current scene
        UpdateMusic(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");
        UpdateMusic(scene.name);
    }

    private void UpdateMusic(string sceneName)
    {
        // Enable music only for "Main" and "lobby" (exact match)
        bool shouldPlay = sceneName == "Main" || sceneName == "lobby";

        if (audioSource != null)
        {
            if (shouldPlay && !audioSource.isPlaying)
            {
                audioSource.Play();
                Debug.Log($"Music started in scene: {sceneName}");
            }
            else if (!shouldPlay && audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log($"Music stopped in scene: {sceneName}");
            }
        }
    }
}
