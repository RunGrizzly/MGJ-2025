using System;
using Settings;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField] private World _worldPrefab;
        [SerializeField] private float _worldDistance;
        [SerializeField] private float _spawnAngle;
        [SerializeField] private TrackGenerator _trackGenerator;
        [SerializeField] private DifficultySettings _difficultySettings;

        public Level Generate(int number)
        {
            var world = Instantiate(_worldPrefab);
            var worldPosition = GenerateWorldPosition(number);
            var worldRadius = Random.Range(500f, 600f);
            world.Init(worldRadius, worldPosition);
            var trackDefinition = _trackGenerator.Generate(4, 0f);
            var track = PlayableTrack.FromTrackDefinition(
                trackDefinition,
                _difficultySettings.BeatRate,
                _difficultySettings.BeatTimingWindow);

            return new Level(world, track, number);
        }

        private Vector3 GenerateWorldPosition(int number)
        {
            return Vector3.right * _worldDistance * number;
        }
    }

    [Serializable]
    public struct Range
    {
        public float Min;
        public float Max;

        public Range(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}