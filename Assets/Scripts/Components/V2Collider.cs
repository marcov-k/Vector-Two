using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PhysObject))]
public class V2Collider : V2Component
{
    protected new SpriteRenderer renderer;
    public PhysObject physObject;

    protected override void InitObject()
    {
        base.InitObject();
        V2Objects.colliders.Add(this);
        renderer = GetComponent<SpriteRenderer>();
        physObject = GetComponent<PhysObject>();
    }

    protected override void InitValues()
    {
        CalcMOI();
    }

    protected virtual void CalcMOI() { }
}
