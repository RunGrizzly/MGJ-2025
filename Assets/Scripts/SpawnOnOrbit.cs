using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class Beat
{
    [Range(0, 1)]
    public float Position = 0;
    public float Size;
}


public class SpawnOnOrbit : MonoBehaviour
{ 
    public OrbitManager OrbitManager = null;
   
   public List<Beat> Beats;
   
   private void OnDrawGizmos()
   {
       Gizmos.color = OrbitManager.MainOrbit.Color;
       foreach (var prompt in Beats)
       {
           Gizmos.DrawSphere(OrbitManager.OrbitPointFromNormalisedPosition(  OrbitManager.MainOrbit,prompt.Position),prompt.Size);
       }
   }
}
