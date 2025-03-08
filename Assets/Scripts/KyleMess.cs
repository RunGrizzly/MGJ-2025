using System.Collections.Generic;
using Events;
using Gameplay;
using Gameplay.TrackEvents;
using SGS29.Utilities;
using UnityEngine;

// [ExecuteAlways]
public class KyleMess : MonoBehaviour
{
    [SerializeField] private OrbitManager _orbitManager;
    [SerializeField] private TrackGenerator _trackGenerator;
    [SerializeField] private TrackPlayer _trackPlayer;
    private EventManager _eventManager;
    private Dictionary<Gameplay.Beat, float> _normalizedBeatTimes;

    public void OnEnable()
    {
        var trackDefinition = _trackGenerator.Generate(4, 0.5f);
        _trackPlayer.Play(trackDefinition);
        _normalizedBeatTimes = _trackPlayer._currentTrack.GetNormalizedBeatTimes();

        _eventManager = SM.Instance<EventManager>();
        _eventManager.RegisterListener<TrackStarted>(evt => Debug.Log("Track started"));
        _eventManager.RegisterListener<TrackFailed>(evt => Debug.Log("Track failed"));
        _eventManager.RegisterListener<TrackPassed>(evt => Debug.Log("Track passed"));
        _eventManager.RegisterListener<GameOver>(evt => Debug.Log("GAME OVER LOSER"));
    }

    public void OnDrawGizmos()
    {
        if (_normalizedBeatTimes == null)
        {
            return;
        }

        foreach (var (beat, normalizedTime) in _normalizedBeatTimes)
        {
            Gizmos.color = beat.Action == BeatAction.Empty ? Color.red : Color.green;
            Gizmos.DrawWireSphere(
                _orbitManager.OrbitPointFromNormalisedPosition(_orbitManager.MainOrbit, normalizedTime), 20f);
        }
    }
}