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
    [SerializeField] private TrackGenerator _trackGenerator;
    [SerializeField] private TrackPlayer _trackPlayer;
    [SerializeField] private LevelGenerator _levelGenerator;
    [SerializeField] private Transform _shipPrefab;
    private EventManager _eventManager;
    private TrackDefinition _trackDefinition;
    private GameplayState _currentState;
    private List<Level> _levels;
    private float _progress;
    private Level _currentLevel;
    private Transform _playerShip;

    enum GameplayState
    {
        InHangar,
        Transitioning,
        OnTrack,
        OnExitTrack,
        Dead
    }

    public void OnEnable()
    {
        _playerShip = Instantiate(_shipPrefab);

        _eventManager = SM.Instance<EventManager>();
        _eventManager.RegisterListener<TrackStarted>(evt => Debug.Log("Track started"));
        _eventManager.RegisterListener<GameStarted>(StartTrack);
        _eventManager.RegisterListener<GameOver>(evt => TrackFailed());
        _eventManager.RegisterListener<TrackPassed>(evt => TrackPassed());
        _eventManager.RegisterListener<GameOver>(evt => Debug.Log("GAME OVER LOSER"));
    }

    public void Start()
    {
        _levels = Enumerable.Range(0, 5).Select(_levelGenerator.Generate).ToList();
        _currentLevel = _levels.First();
        SM.Instance<EventManager>().DispatchEvent(new NewLevel(_currentLevel));
    }

    private void StartTrack(GameStarted _)
    {
        _trackPlayer.Play(_currentLevel.Track);
        _normalizedBeatTimes = _trackPlayer._currentTrack.GetNormalizedBeatTimes();
        _currentState = GameplayState.OnTrack;
    }

    private void Update()
    {
        if (_currentState is not (GameplayState.OnTrack or GameplayState.OnExitTrack))
        {
            return;
        }

        _progress += Time.deltaTime;
        _trackPlayer.Tick(_progress);
        _playerShip.position = OrbitHelpers.OrbitPointFromNormalisedPosition(_currentLevel.World.Orbit,
            _progress / _currentLevel.Track.Duration);

        _playerShip.rotation = OrbitHelpers.ForwardRotationFromNormalisePosition(_currentLevel.World.Orbit,
            _progress / _currentLevel.Track.Duration);

        if (_trackPlayer._currentTrack.State == PlayableTrack.States.Playing)
        {
            _progress = Mathf.Repeat(_progress, _trackPlayer._currentTrack.Duration);
        }
    }

    private void TrackPassed()
    {
        switch (_currentState)
        {
            case GameplayState.OnTrack:
                _currentState = GameplayState.OnExitTrack;
                _trackPlayer.Play(_currentLevel.ExitTrack);
                _trackPlayer.Tick(_progress);
                _eventManager.DispatchEvent(new LevelPassed(_currentLevel));
                break;
            case GameplayState.OnExitTrack:
                _currentState = GameplayState.Transitioning;
                break;
            default:
                return;
        }
    }

    private void TrackFailed()
    {
        _currentState = GameplayState.Dead;
    }
}