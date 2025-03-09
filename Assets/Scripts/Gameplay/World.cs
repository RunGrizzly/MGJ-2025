using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class World : MonoBehaviour
    {
        public float Radius { get; private set; }
        public Vector3 Position { get; private set; }
        public Orbit Orbit { get; private set; }
        
        public Vector3 rotateAxis = Vector3.up;
        public float rotateSpeed = 1f;

        //public Material PlanetMaterial = null;

        public MeshRenderer PlanetRenderer = null;
        
        public List<Texture2D> SurfaceMasks = new List<Texture2D>();
        
        public void Init(float radius, Vector3 position)
        {
            Radius = radius;
            Position = position;

            transform.position = position;
            transform.localScale = Vector3.one * radius;

            Orbit = new Orbit(transform, Vector3.zero, 1.2f, transform.up);

            rotateAxis = new Vector3(Random.Range(0.2f, 1), Random.Range(0.2f, 1f), Random.Range(0.2f, 1f)).normalized;
            rotateSpeed = Random.Range(0.01f, 0.095f);
            
            SetRandomSurface();
        }
        
        private void Update()
        {
            PlanetRenderer.transform.Rotate(rotateAxis,rotateSpeed);
        }

        public void SetRandomSurface()
        {
          PlanetRenderer.material.SetTexture("_Mask", SurfaceMasks[Random.Range(0,SurfaceMasks.Count)]);
        }
    }
}