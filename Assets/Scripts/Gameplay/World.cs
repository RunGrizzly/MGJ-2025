using UnityEngine;

namespace Gameplay
{
    public class World : MonoBehaviour
    {
        public float Radius { get; private set; }
        public Vector3 Position { get; private set; }
        public Orbit Orbit { get; private set; }

        public void Init(float radius, Vector3 position)
        {
            Radius = radius;
            Position = position;

            transform.position = position;
            transform.localScale = Vector3.one * radius;

            Orbit = new Orbit(transform, Vector3.zero, 1.2f, transform.up);
        }
    }
}