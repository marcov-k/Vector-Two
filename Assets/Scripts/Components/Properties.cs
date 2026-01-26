using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class Properties : MonoBehaviour
{
    public Vector2 pos = Vector2.zero;
    public float Rot
    {
        get { return rot; }
        set { rot = ClampRot(value); }
    }
    float rot = 0.0f;
    public Vector2 v = Vector2.zero;
    public float av = 0.0f; // angular velocity
    public Vector2 a = Vector2.zero;
    public float aa = 0.0f; // angular acceleration
    public Vector2 f = Vector2.zero;
    public float t = 0.0f;
    public Vector2 p = Vector2.zero;
    public float ke = 0.0f;
    public float m = 1.0f;
    public float e = 0.5f;
    public float cof = 1.0f; // coefficient of friction
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
