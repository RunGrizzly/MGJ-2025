using System;
using Events;
using SGS29.Utilities;
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

        public Level Generate(int number)
        {
            var world = Instantiate(_worldPrefab);
            var worldPosition = GenerateWorldPosition(number);
            var worldRadius = Random.Range(500f, 600f);
            world.Init(worldRadius, worldPosition);
            var trackDefinition = _trackGenerator.Generate(4, 1f / number);
            var track = PlayableTrack.FromTrackDefinition(trackDefinition, 1f, 1.2f);
            
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