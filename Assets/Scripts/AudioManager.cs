using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip musicClip;
    }

    [Header("Configuración de Música")]
    public AudioSource audioSource;
    public List<SceneMusic> musicList;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    private void PlayMusicForScene(string sceneName)
    {
        
        AudioClip clip = musicList.Find(m => m.sceneName == sceneName)?.musicClip;

        if (clip != null)
        {
            
            if (audioSource.clip == clip && audioSource.isPlaying) return;

            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            Debug.Log($"No hay música asignada para la escena: {sceneName}");
            
            audioSource.Stop();
        }
    }
}