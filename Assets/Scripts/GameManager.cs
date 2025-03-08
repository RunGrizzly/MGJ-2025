using System.Collections.Generic;
using System.Linq;
using Events;
using Gameplay;
using Gameplay.TrackEvents;
using SGS29.Utilities;
using UnityEngine;

// [ExecuteAlways]
public class GameManager : MonoBehaviour
{
    [SerializeField] private OrbitManager _orbitManager;
    [SerializeField] private TrackGenerator _trackGenerator;
    [SerializeField] private TrackPlayer _trackPlayer;
    [SerializeField] private LevelGenerator _levelGenerator;
    private EventManager _eventManager;
    private Dictionary<Gameplay.Beat, float> _normalizedBeatTimes;
    private TrackDefinition _trackDefinition;
    private GameplayState _currentState;
    private List<Level> _levels;

    enum GameplayState
    {
        InHangar,
        Transitioning,
        OnTrack,
        Dead
    }

    public void OnEnable()
    {
        _trackDefinition = _trackGenerator.Generate(4, 0.5f);

        _eventManager = SM.Instance<EventManager>();
        _eventManager.RegisterListener<TrackStarted>(evt => Debug.Log("Track started"));
        _eventManager.RegisterListener<GameStarted>(StartTrack);
        _eventManager.RegisterListener<TrackFailed>(evt => TrackFailed());
        _eventManager.RegisterListener<TrackPassed>(evt => TrackPassed());
        _eventManager.RegisterListener<GameOver>(evt => Debug.Log("GAME OVER LOSER"));

        _levels = Enumerable.Range(0, 5).Select(_levelGenerator.Generate).ToList();
    }

    private void StartTrack(GameStarted _)
    {
        _trackPlayer.Play(_trackDefinition);
        _normalizedBeatTimes = _trackPlayer._currentTrack.GetNormalizedBeatTimes();
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

    private void TrackPassed()
    {
        _currentState = GameplayState.Transitioning;
    }

    private void TrackFailed()
    {
        _currentState = GameplayState.Dead;
    }
}