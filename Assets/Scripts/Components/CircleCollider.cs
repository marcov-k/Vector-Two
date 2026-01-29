using UnityEngine;

public class CircleCollider : V2Collider
{
    public float radius;

    protected override void InitValues()
    {
        radius = renderer.bounds.extents.y;
        base.InitValues();
    }

    protected override void CalcMOI()
    {
        Properties.moi = 0.5f * Properties.m * Mathf.Pow(radius, 2.0f);
    }

    protected override void CalcMaxDim()
    {
        maxDim = radius;
    }
}
