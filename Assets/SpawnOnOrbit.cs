using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Prompt
{
    [Range(0, 1)]
    public float Position = 0;
    public float Size;
}


public class SpawnOnOrbit : MonoBehaviour
{
   [FormerlySerializedAs("RotateAroundBody")] public OrbitManager orbitManager = null;
   
   public List<Prompt> Prompts;
   
   private void OnDrawGizmos()
   {
       Gizmos.color = orbitManager.MainOrbit.Color;
       foreach (var prompt in Prompts)
       {
           Gizmos.DrawSphere(orbitManager.OrbitPointFromNormalisedPosition(  orbitManager.MainOrbit,prompt.Position),prompt.Size);
       }
   }
}
