using System;
using Events;
using SGS29.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    
        SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
    }

    private void OnEnable()
    {
        //Listen for the reset game event
        SM.Instance<EventManager>().RegisterListener<ResetGame>(OnResetGame);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex != 1) return;
        
        SceneManager.SetActiveScene(scene);
    }
    
    private void OnResetGame(ResetGame resetGame)
    {
        SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
    }
}
