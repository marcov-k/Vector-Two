using UnityEngine;
using static Constants;
using static V2Objects;

public class Gravity : V2Component
{
    /// <summary>
    /// Minimum squared radius of the body's field of influence
    /// </summary>
    public float FieldRadius { get; protected set; }
    public float Gm { get; protected set; }
    protected PhysObject physObject;

    protected override void InitObject()
    {
        base.InitObject();
        physObject = GetComponent<PhysObject>();
        gravities.Add(this);
        physObject.gravIndex = gravities.IndexOf(this);
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
