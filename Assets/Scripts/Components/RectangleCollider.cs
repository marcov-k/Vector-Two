using UnityEngine;

public class RectangleCollider : V2Collider
{
    public float w;
    public float h;

    protected override void InitValues()
    {
        base.InitValues();
        w = renderer.bounds.extents.x;
        h = renderer.bounds.extents.y;
    }

    public (Vector2[] vertices, Vector2[] normals) GetVerticesAndNormals()
    {
        var vertices = new Vector2[4]; // clockwise from top left
        var normalVectors = new Vector2[4]; // clockwise from top

        float theta = Properties.rot * Mathf.Deg2Rad;
        float theta2 = Mathf.Atan2(h, w);
        float d = Mathf.Sqrt(Mathf.Pow(w, 2.0f) + Mathf.Pow(h, 2.0f));

        float thetaB = theta + theta2;
        vertices[1] = new(d * Mathf.Cos(thetaB), d * Mathf.Sin(thetaB));

        vertices[3] = vertices[1] * -1.0f;

        float thetaC = theta - theta2;
        vertices[2] = new(d * Mathf.Cos(thetaC), d * Mathf.Sin(thetaC));

        vertices[0] = vertices[2] * -1.0f;

        Vector2 p1;
        Vector2 p2;
        for (int i = 0; i < vertices.Length; i++)
        {
            p1 = vertices[i];

            if (i < vertices.Length - 1) p2 = vertices[i + 1];
            else p2 = vertices[0];

            var v = p2 - p1;

            Vector2 n = new(-v.y, v.x);
            normalVectors[i] = n.normalized;
        }

        return (vertices, normalVectors);
    }
}
