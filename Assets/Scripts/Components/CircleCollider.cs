using UnityEngine;

public class CircleCollider : V2Collider
{
    public float radius;

    protected override void InitValues()
    {
        base.InitValues();
        radius = renderer.bounds.extents.y;
    }
}
