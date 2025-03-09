using System.Collections.Generic;
using System.Linq;
using Events;
using Gameplay;
using Gameplay.TrackEvents;
using SGS29.Utilities;
using UnityEngine;
using UnityEngine.Splines;

// [ExecuteAlways]
public class GameManager : MonoBehaviour
{
    [SerializeField] private TrackGenerator _trackGenerator;
    [SerializeField] private TrackPlayer _trackPlayer;
    [SerializeField] private LevelGenerator _levelGenerator;
    [SerializeField] private Transform _shipPrefab;
    public SplineContainer _container;
    private EventManager _eventManager;
    private TrackDefinition _trackDefinition;
    private GameplayState _currentState = GameplayState.InHangar;
    private List<Level> _levels;
    private float _progress;
    private Level _currentLevel;
    private Transform _playerShip;
    private SplineAnimate _splineAnimate;

    enum GameplayState
    {
        InHangar,
        Transitioning,
        OnTrack,
        OnExitTrack,
        Dead
    }


    //GAME MANAGER NOW GIZMOS OUT THE LEVEL NORMALISED POSITIONS
    private void OnDrawGizmos()
    {
        if (_currentLevel == null)
        {
            return;
        }

        if (_trackPlayer._currentTrack == null)
        {
            return;
        }

        Orbit newOrbit = _currentLevel.World.Orbit;

        foreach (var beat in _trackPlayer._currentTrack.GetNormalizedBeatTimes())
        {
            if (beat.Key.Action == BeatAction.Empty)
            {
                Gizmos.color = Color.gray;
            }

            else if (beat.Key.Action == BeatAction.Transfer)
            {
                Gizmos.color = Color.cyan;
            }

            else
            {
                Gizmos.color = Color.green;
            }

            Gizmos.DrawSphere(OrbitHelpers.OrbitPointFromNormalisedPosition(newOrbit, beat.Value), 40f);

            // Gizmos.DrawSphere(OrbitHelpers.OrbitPointFromNormalisedPosition(newOrbit, beat.Key.StartTime/_currentLevel.Track.Duration),20f);
            // Gizmos.DrawSphere(OrbitHelpers.OrbitPointFromNormalisedPosition(newOrbit,  beat.Key.EndTime/_currentLevel.Track.Duration),20f);


            Gizmos.color = Color.red;

            var currentBeat = _trackPlayer._currentTrack.GetCurrentBeat();
            if (currentBeat != null)
            {
                Gizmos.DrawWireSphere(
                    OrbitHelpers.OrbitPointFromNormalisedPosition(newOrbit,
                        ((currentBeat.StartTime + currentBeat.EndTime) / 2) / _trackPlayer._currentTrack.Duration),
                    60f);
            }

            Gizmos.color = Color.yellow;

            Gizmos.DrawSphere(
                OrbitHelpers.OrbitPointFromNormalisedPosition(newOrbit,
                    _trackPlayer._progress / _trackPlayer._currentTrack.Duration), 60f);
        }
    }


    public void OnEnable()
    {
        _playerShip = Instantiate(_shipPrefab);

        _eventManager = SM.Instance<EventManager>();
        _eventManager.RegisterListener<TrackStarted>(evt => Debug.Log("Track started"));
        _eventManager.RegisterListener<GameStarted>(OnGameStarted);
        _eventManager.RegisterListener<GameOver>(evt => TrackFailed());
        _eventManager.RegisterListener<TrackPassed>(evt => TrackPassed());
        _eventManager.RegisterListener<GameOver>(evt => Debug.Log("GAME OVER LOSER"));

        _container = FindAnyObjectByType<SplineContainer>();
        _splineAnimate = _playerShip.GetComponent<SplineAnimate>();
    }

    public void Start()
    {
        _levels = Enumerable.Range(0, 5).Select(_levelGenerator.Generate).ToList();
    }

    private void OnGameStarted(GameStarted evt)
    {
        StartNextLevel();
    }

    private void StartNextLevel()
    {
        _progress = 0f;
        _currentLevel = _currentLevel == null ? _levels.First() : _levels[_currentLevel.Number + 1];
        SM.Instance<EventManager>().DispatchEvent(new NewLevel(_currentLevel));
        _trackPlayer.Play(_currentLevel.Track);
        _currentState = GameplayState.OnTrack;
        _splineAnimate.Completed -= StartNextLevel;
        _splineAnimate?.Container?.KnotLinkCollection.Clear();
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
                var pos1 = OrbitHelpers.OrbitPointFromNormalisedPosition(_currentLevel.World.Orbit, 0.75f);
                var pos3 = OrbitHelpers.OrbitPointFromNormalisedPosition(_levels[_currentLevel.Number + 1].World.Orbit,
                    0.5f);
                var pos2 = new Vector3((pos1.x + pos3.x) / 2, 0, -2500f);

                var knot1 = new BezierKnot(pos1, 0f, 1000f);
                var knot3 = new BezierKnot(pos3, 0f, 750f);
                var spline = new Spline();
                spline.Add(knot1, TangentMode.Mirrored);
                spline.Add(pos2);
                spline.Add(knot3, TangentMode.Mirrored);


                _container.Spline = spline;
                _splineAnimate.Container = _container;

                _splineAnimate.Duration = 10f;
                _splineAnimate.Loop = SplineAnimate.LoopMode.Once;
                _splineAnimate.ElapsedTime = 0f;
                _splineAnimate.Play();
                _splineAnimate.Completed += StartNextLevel;

                _currentState = GameplayState.Transitioning;

                _levels.Add(_levelGenerator.Generate(_levels.Count));
                SM.Instance<EventManager>().DispatchEvent(new TransitionStarted());
                break;
            default:
                return;
        }
    }

    private void TrackFailed()
    {
        _currentState = GameplayState.Dead;
    }

    public class TransitionStarted : IEvent
    {
    }
}