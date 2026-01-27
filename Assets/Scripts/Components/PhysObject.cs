using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using static Constants;
using static V2Objects;

public class PhysObject : V2Component
{
    public float Mass = 1.0f;
    [Range(0.0f, 1.0f)] public float RestitutionCoefficient = 0.5f;
    public float FrictionCoefficient = 1.0f;
    float InitialRotation;
    public int gravIndex = -1;

    protected override void InitObject()
    {
        base.InitObject();
        Properties.m = Mass;
        Properties.e = RestitutionCoefficient;
        Properties.cof = FrictionCoefficient;
        Properties.pos = new(transform.position.x, transform.position.y);
        InitialRotation = transform.eulerAngles.z;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        Properties.Rot = 0.0f;
    }

    protected override void InitValues()
    {
        Properties.Rot = InitialRotation;
        base.InitValues();
    }

    protected override void PhysUpdate()
    {
        CalcGravity();
        CalcAccel();
        CalcVel();
        CalcPos();
        CalcRot();
        CalcP();
        CalcKE();
        ResetForce();
        ResetAccel();
    }

    void ResetForce()
    {
        Properties.f = float2.zero;
        Properties.t = 0.0f;
    }

    void ResetAccel()
    {
        Properties.a = float2.zero;
        Properties.aa = 0.0f;
    }

    void CalcGravity()
    {
        Parallel.For(0, gravities.Count, i =>
        {
            if (i != gravIndex)
            {
                var grav = gravities[i];
                var gProps = grav.Properties;
                var dPos = gProps.pos - Properties.pos;
                float r2 = dPos.sqrMagnitude;
                if (r2 <= grav.FieldRadius)
                {
                    float r2Recip = 1.0f / r2;
                    float g = grav.Gm * r2Recip;
                    float mgMag = g * Properties.m;
                    var dir = dPos.normalized;
                    var mg = mgMag * dir;
                    Properties.f += mg;
                }
            }
        });
    }

    void CalcAccel()
    {
        Properties.a += Properties.f / Properties.m;
        Properties.aa += Properties.t / Properties.moi;
    }

    void CalcVel()
    {
        Properties.v += Properties.a * physTimestep;
        Properties.v -= Properties.v * drag * physTimestep;
        Properties.av += Properties.aa * physTimestep;
        Properties.av -= Properties.av * drag * physTimestep;
    }

    void CalcPos()
    {
        Properties.pos += Properties.v * physTimestep;
    }

    void CalcRot()
    {
        Properties.Rot += Properties.av * Mathf.Rad2Deg * physTimestep;
    }

    void CalcP()
    {
        Properties.p = Properties.v * Properties.m;
    }

    void CalcKE()
    {
        float2 v = Properties.v;
        float v2 = Mathf.Pow(v.x, 2.0f) + Mathf.Pow(v.y, 2.0f);
        Properties.ke = 0.5f * Properties.m * v2;
    }

    protected override void VisUpdate()
    {
        transform.SetPositionAndRotation(new(Properties.pos.x, Properties.pos.y), Quaternion.Euler(0.0f, 0.0f, Properties.Rot));
    }

    public void AddLinearImpulse(Vector2 impulse)
    {
        Properties.v += impulse / Properties.m;
    }

    public void AddAngularImpulse(float impulse)
    {
        Properties.av += impulse / Properties.moi;
    }
}
