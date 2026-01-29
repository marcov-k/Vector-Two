using UnityEngine;
using System;
using static V2Objects;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PhysObject))]
public class V2Collider : V2Component
{
    protected new SpriteRenderer renderer;
    public PhysObject physObject;
    public float maxDim;

    protected override void InitObject()
    {
        base.InitObject();
        colliders.Add(this);
        renderer = GetComponent<SpriteRenderer>();
        physObject = GetComponent<PhysObject>();
    }

    protected override void InitValues()
    {
        CalcMOI();
        CalcMaxDim();
    }

    protected virtual void CalcMOI() { throw new NotImplementedException(); }

    protected virtual void CalcMaxDim() { throw new NotImplementedException(); }
}
