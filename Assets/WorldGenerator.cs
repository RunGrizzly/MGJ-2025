using System;
using Gameplay;
using UnityEngine;

public class WorldParams
{
    public float Radius;
    public Vector3 Position;
    public float RadiusFactor;

    public WorldParams(float radius, Vector3 position, float radiusFactor)
    {
        Radius = radius;
        Position = position;
        RadiusFactor = radiusFactor;
    }
}

public class WorldGenerator : MonoBehaviour
{
   [SerializeField] private World WorldTemplate = null;

  [SerializeField] private float _worldDistance = 1;
    

    public static WorldGenerator ins = null;
    
    private void Awake()
    {
        ins = this;
    }
    
    public World GenerateWorld(WorldParams parameters, int index)
    {
        World newWorld = Instantiate(WorldTemplate);
        
        newWorld.Init(parameters.Radius,GenerateWorldPosition(index), parameters.RadiusFactor);

        return newWorld;
    }
    
    private Vector3 GenerateWorldPosition(int number)
    {
        return Vector3.right * _worldDistance * number;
    }
    
    
}
