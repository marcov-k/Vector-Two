using UnityEngine;
using static Constants;

public class Gravity : V2Component
{
    /// <summary>
    /// Minimum squared radius of the body's field of influence
    /// </summary>
    public float FieldRadius { get; protected set; }
    public float Gm { get; protected set; }

    protected override void InitObject()
    {
        base.InitObject();
        V2Objects.gravities.Add(this);
    }

    protected override void InitValues()
    {
        base.InitValues();
        Gm = gravConst * Properties.m;
        CalcFieldRadius();
    }

    void CalcFieldRadius()
    {
        FieldRadius = gravConst * Properties.m / minGravA;
    }
}
