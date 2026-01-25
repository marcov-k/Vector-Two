using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class Properties : MonoBehaviour
{
    public float2 pos = float2.zero;
    public float Rot
    {
        get { return rot; }
        set { rot = ClampRot(value); }
    }
    float rot = 0.0f;
    public float2 v = float2.zero;
    public float av = 0.0f; // angular velocity
    public float2 a = float2.zero;
    public float aa = 0.0f; // angular acceleration
    public float2 f = float2.zero;
    public float t = 0.0f;
    public float2 p = float2.zero;
    public float ke = 0.0f;
    public float m = 1.0f;
    public float e = 0.5f;
    public float moi = 1.0f; // moment of inertia around the z-axis

    float ClampRot(float rot)
    {
        while (rot >= 360.0f)
        {
            rot -= 360.0f;
        }
        while (rot <= 0.0f)
        {
            rot += 360.0f;
        }
        return rot;
    }
}
