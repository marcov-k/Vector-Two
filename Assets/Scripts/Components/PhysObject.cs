using Unity.Mathematics;
using UnityEngine;
using static Constants;

public class PhysObject : V2Component
{
    public float Mass = 1.0f;
    [Range(0.0f, 1.0f)] public float RestitutionCoefficient = 0.5f;
    float InitialRotation;

    protected override void InitObject()
    {
        base.InitObject();
        InitialRotation = transform.eulerAngles.z;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
    }

    protected override void InitValues()
    {
        base.InitValues();
        Properties.m = Mass;
        Properties.e = RestitutionCoefficient;
        Properties.pos = new(transform.position.x, transform.position.y);
        Properties.rot = InitialRotation;
    }

    protected override void PhysUpdate()
    {
        CalcGravity();
        CalcAccel();
        CalcVel();
        CalcPos();
        CalcP();
        CalcKE();
    }

    void CalcGravity()
    {
        Properties gProps;
        float2 dPos;
        float r2;
        float r;
        float rRecip;
        float r2Recip;
        float mgMag;
        float g;
        float2 mg;
        foreach (var grav in V2Objects.gravities)
        {
            if (grav.gameObject != gameObject)
            {
                gProps = grav.GetProperties();
                dPos = gProps.pos - Properties.pos;
                r2 = Mathf.Pow(dPos.x, 2.0f) + Mathf.Pow(dPos.y, 2.0f);
                r = Mathf.Sqrt(r2);
                if (r <= grav.FieldRadius)
                {
                    rRecip = 1.0f / r;
                    r2Recip = 1.0f / r2;
                    g = grav.Gm * r2Recip;
                    mgMag = g * Properties.m;
                    mg = new(mgMag * dPos.x * rRecip, mgMag * dPos.y * rRecip);
                    Properties.f += mg;
                }
            }
        }
    }

    void CalcAccel()
    {
        Properties.a = Properties.f / Properties.m;
    }

    void CalcVel()
    {
        Properties.v += Properties.a * physTimestep;
    }

    void CalcPos()
    {
        Properties.pos += Properties.v * physTimestep;
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
        transform.SetPositionAndRotation(new(Properties.pos.x, Properties.pos.y), Quaternion.Euler(0.0f, 0.0f, Properties.rot));
    }
}
