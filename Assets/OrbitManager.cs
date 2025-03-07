using System;
using Unity.Cinemachine;
using UnityEngine;

[Serializable]
public class Orbit
{
    public Vector3 Centre;
    public float Radius;
    public float Axis;

    public Color Color = UnityEngine.Color.white;

    public Orbit(Vector3 centre, float radius, float axis)
    {
        Centre = centre;
        Radius = radius;
        Axis = axis;
    }
}

[ExecuteAlways]
public class OrbitManager : MonoBehaviour
{
    [SerializeField]
    private Transform m_TargetBody;
   
    //[SerializeField]
    //private float m_radiusFactor;

    //[SerializeField] 
    //private Vector3 m_axis;

    [SerializeField] 
    private float m_rotateSpeed;
    
    [SerializeField]
    [Range(0,1)]
    private float m_normalisedPosition;

    [SerializeField]
    private bool m_manualPosition = true;


    public Orbit MainOrbit = null;

    public Orbit SecondaryOrbit = null;

    public void OnDrawGizmos()
    {
        Gizmos.color = MainOrbit.Color;
        Gizmos.DrawWireSphere( transform.position+MainOrbit.Centre,MainOrbit.Radius);

        Gizmos.color = SecondaryOrbit.Color;
        Gizmos.DrawWireSphere( transform.position+SecondaryOrbit.Centre,SecondaryOrbit.Radius);
    }

    public Vector3 OrbitPointFromNormalisedPosition(Orbit orbit, float normalisedPosition)
    {
        var angle = -Mathf.Deg2Rad*( normalisedPosition*360);

        return transform.position + orbit.Centre + transform.localScale.x* orbit.Radius *(( Mathf.Cos(angle) * transform.right) + (Mathf.Sin(angle) * transform.forward));
    }

    public Quaternion ForwardRotationFromNormalisePosition(Orbit orbit,float normalisedPosition)
    {
        var angle = -Mathf.Deg2Rad*( normalisedPosition*360);

       return Quaternion.LookRotation((Mathf.Sin(angle) * transform.right) - (Mathf.Cos(angle) * transform.forward), transform.up);

        
    }
    
    private void Update()
    {
        if (m_TargetBody == null)
        {
            Debug.LogFormat($"m_targetBody is not set - aborting rotate.");
            return;
        }

        // Orbit orbit = new Orbit(transform.position,)
        
        
        m_normalisedPosition = m_manualPosition? m_normalisedPosition: Mathf.Repeat(Time.time * m_rotateSpeed, 1);
        
       m_TargetBody.transform.position = OrbitPointFromNormalisedPosition(MainOrbit,m_normalisedPosition);
       m_TargetBody.transform.rotation = ForwardRotationFromNormalisePosition(MainOrbit,m_normalisedPosition);
    }
}
