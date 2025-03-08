using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private void Awake()
    {
        SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        // SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);
    }
}
