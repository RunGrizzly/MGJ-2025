using System;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteAlways]
public class RotateAroundBody : MonoBehaviour
{
    [SerializeField]
    private Transform m_sourceBody;
   
    [SerializeField]
    private float m_radiusFactor;

    [SerializeField] 
    private Vector3 m_axis;

    [SerializeField] 
    private float m_rotateSpeed;

    [SerializeField]
    private float m_angle;

    private float m_progress;
    
    private void Update()
    {
        if (m_sourceBody == null)
        {
            Debug.LogFormat($"m_sourceBody is not set - aborting rotate.");
            return;
        }

        var centre = m_sourceBody.position;
        var radius =  m_radiusFactor*m_sourceBody.transform.localScale.x ;

        var angle = Mathf.Repeat(Time.time * m_rotateSpeed, 360);
        angle = Mathf.Deg2Rad*( angle);

        var point = centre + radius *(( Mathf.Cos(angle) * m_sourceBody.transform.right) + (Mathf.Sin(angle) * m_sourceBody.transform.forward));

        transform.position = point;

        transform.rotation = Quaternion.LookRotation((-Mathf.Sin(angle) * m_sourceBody.transform.right) + (Mathf.Cos(angle) * m_sourceBody.transform.forward), m_sourceBody.up);
    }
}
