using Events;
using Gameplay;
using Gameplay.TrackEvents;
using SGS29.Utilities;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerShipFramer : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCameraBase PlanetCam = null;
    [SerializeField] private CinemachineVirtualCameraBase TransitionCam = null;
    
    private void OnEnable()
    {
        //SM.Instance<EventManager>().RegisterListener<NewLevel>(OnNewLevel);
        //SM.Instance<EventManager>().RegisterListener<BeatAttemptEvent>(OnBeatAttempt);
    
        SM.Instance<EventManager>().RegisterListener<GameManager.TransitionStarted>(OnTransitionStarted);
    
        SM.Instance<EventManager>().RegisterListener<TrackStarted>(OnTrackStarted);
    
        //SM.Instance<EventManager>().RegisterListener<GameOver>(OnGameOver);
    
        SM.Instance<EventManager>().RegisterListener<MainMenu>(OnMainMenu);
    }
    
    private void OnDisable()
    {
        //SM.Instance<EventManager>().UnregisterListener<NewLevel>(OnNewLevel);
        
        //SM.Instance<EventManager>().UnregisterListener<BeatAttemptEvent>(OnBeatAttempt);
    
        SM.Instance<EventManager>().UnregisterListener<GameManager.TransitionStarted>(OnTransitionStarted);
    
        SM.Instance<EventManager>().UnregisterListener<TrackStarted>(OnTrackStarted);
    
        //SM.Instance<EventManager>().UnregisterListener<GameOver>(OnGameOver);
    
        SM.Instance<EventManager>().UnregisterListener<MainMenu>(OnMainMenu);
    }
    
    private void OnTransitionStarted(GameManager.TransitionStarted context)
    {
        TransitionCam.Priority = 100;
    }
    
    
    private void OnTrackStarted(TrackStarted context)
    {
        TransitionCam.Priority = 0;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
    }
    
    private void OnMainMenu(MainMenu context)
    {
        TransitionCam.Priority = 0;
    }

}
