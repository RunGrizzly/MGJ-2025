using System.Collections.Generic;
using Gameplay;
using UnityEngine;

// [ExecuteAlways]
public class KyleMess : MonoBehaviour
{
    [SerializeField] private OrbitManager _orbitManager;
    [SerializeField] private TrackGenerator _trackGenerator;
    [SerializeField] private TrackPlayer _trackPlayer;
    private Dictionary<Gameplay.Beat, float> _normalizedBeatTimes;

    public void OnEnable()
    {
        var trackDefinition = _trackGenerator.Generate(4, 0.5f);
        _trackPlayer.Play(trackDefinition);
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
}