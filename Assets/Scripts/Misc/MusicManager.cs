using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public AudioClip LobbyMusic;
    public AudioClip GameplayMusic;

    private AudioSource audioSource;
    private string currentScene = "";
    
    [Range(0f, 1f)]
    public float volume = 1f;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        currentScene = SceneManager.GetActiveScene().name;
        PlayMusicForScene(currentScene);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != currentScene)
        {
            currentScene = scene.name;
            PlayMusicForScene(currentScene);
        }
    }

    void PlayMusicForScene(string sceneName)
    {
        AudioClip targetClip = sceneName == "Main" ? GameplayMusic : LobbyMusic;

        if (audioSource.clip != targetClip)
        {
            audioSource.clip = targetClip;
            audioSource.volume = volume;
            audioSource.Play();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}