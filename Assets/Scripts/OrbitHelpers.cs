using UnityEngine;

public class Orbit
{
    public Transform Centre;
    public Vector3 Offset;
    public float Radius;
    public Vector3 Axis;

    public Color Color = UnityEngine.Color.white;

    public Orbit(Transform centre ,Vector3 offset, float radius, Vector3 axis)
    {
        Centre = centre;
        Offset = offset;
        Radius = radius;
        Axis = axis;
    }
}


public static class OrbitHelpers
{
    public static Vector3 OrbitPointFromNormalisedPosition(Orbit orbit, float normalisedPosition)
    {
        var angle = -Mathf.Deg2Rad * (normalisedPosition * 360);

        return orbit.Centre.position + orbit.Offset + orbit.Centre.localScale.x * orbit.Radius * ((Mathf.Cos(angle) * orbit.Centre.transform.right) + (Mathf.Sin(angle) * orbit.Centre.transform.forward));
    }

    public static Quaternion ForwardRotationFromNormalisePosition(Orbit orbit, float normalisedPosition)
    {
        var angle = -Mathf.Deg2Rad * (normalisedPosition * 360);

        return Quaternion.LookRotation((Mathf.Sin(angle) * orbit.Centre.transform.right) - (Mathf.Cos(angle) * orbit.Centre.transform.forward), orbit.Centre.transform.up);
    }
}
