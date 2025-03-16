using Events;
using SGS29.Utilities;
using TrackEvents;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerShipFramer : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCameraBase PlanetCam = null;
    [SerializeField] private CinemachineVirtualCameraBase TransitionCam = null;
    
    private void OnEnable()
    {
        SM.Instance<EventManager>().RegisterListener<TransitionStarted>(OnTransitionStarted);
    
        SM.Instance<EventManager>().RegisterListener<TrackStarted>(OnTrackStarted);
    
        SM.Instance<EventManager>().RegisterListener<RunStaged>(OnRunStaged);
    }
    
    private void OnDisable()
    {
        SM.Instance<EventManager>().UnregisterListener<TransitionStarted>(OnTransitionStarted);
    
        SM.Instance<EventManager>().UnregisterListener<TrackStarted>(OnTrackStarted);
        
        SM.Instance<EventManager>().UnregisterListener<RunStaged>(OnRunStaged);
    }
    
    private void OnTransitionStarted(TransitionStarted context)
    {
        TransitionCam.Priority = 100;
    }
    
    
    private void OnTrackStarted(TrackStarted context)
    {
        TransitionCam.Priority = 0;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
    }
    
    private void OnRunStaged(RunStaged context)
    {
        TransitionCam.Priority = 0;
    }
}
