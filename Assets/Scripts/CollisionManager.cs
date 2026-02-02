using UnityEngine;
using System.Collections;
using static Constants;
using static V2Objects;
using static VectorUtils;
using static InputManager;
using static Saver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

public class CollisionManager : MonoBehaviour
{
    const float EPS = 1E-5f;
    const int solverIterations = 5;
    IEnumerator collisionCoroutine;

    void Awake()
    {
        collisionCoroutine = CollisionLoop();
        StartCoroutine(collisionCoroutine);
    }

    IEnumerator CollisionLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(physTimestep);
            if (!Paused && !Loading) CheckCollisions();
        }
    }

    void CheckCollisions()
    {
        ConcurrentBag<Collision> collisions = new();

        Parallel.For(0, colliders.Count, (i, loopState) =>
        {
            if (Loading)
            {
                loopState.Stop();
                return;
            }
            Parallel.For(i + 1, colliders.Count, (j, loopState) =>
            {
                if (Loading)
                {
                    loopState.Stop();
                    return;
                }
                if (ObjectCollision(colliders[i], colliders[j], out var collision))
                {
                    collisions.Add(collision);
                }
            });
        });

        for (int i = 0; i < solverIterations; i++)
        {
            if (Loading) break;
            foreach (var collision in collisions)
            {
                if (Loading) break;
                ImpulseResolveCollision(collision);
            }
        }
    }

    void ImpulseResolveCollision(Collision collisionInfo)
    {
        var a = collisionInfo.a;
        var b = collisionInfo.b;

        var point = collisionInfo.point;
        var n = point.normal;
        Vector2 t = new(-n.y, n.x);

        float massA = 1.0f / a.Properties.m;
        float massB = 1.0f / b.Properties.m;
        float totalMass = massA + massB;
        float iA = 1.0f / a.Properties.moi;
        float iB = 1.0f / b.Properties.moi;

        // calculate impulse of collision

        var localA = point.localA;
        var localB = point.localB;

        float wA = a.Properties.av;
        float wB = b.Properties.av;

        var vContA = a.Properties.v + Cross(wA, localA);
        var vContB = b.Properties.v + Cross(wB, localB);
        var vRel = vContB - vContA;

        float vRelNorm = Vector2.Dot(vRel, n);

        float vRelTang = Vector2.Dot(vRel, t);

        float e = a.Properties.e * b.Properties.e;
        float cof = (a.Properties.cof + b.Properties.cof) / 2.0f;

        float jn = (-(1 + e) * vRelNorm) / (totalMass + (Mathf.Pow(Cross(localA, n), 2.0f) * iA) + (Mathf.Pow(Cross(localB, n), 2.0f) * iB));
        float jtMax = (-vRelTang) / (totalMass + (Mathf.Pow(Cross(localA, t), 2.0f) * iA) + (Mathf.Pow(Cross(localB, t), 2.0f) * iB));
        float fricLimit = Mathf.Abs(cof * jn);
        float jt = Mathf.Clamp(jtMax, -fricLimit, fricLimit);

        var totalJ = jn * n + jt * t;

        // apply calculated impulses

        a.physObject.AddLinearImpulse(-totalJ);
        a.physObject.AddAngularImpulse(-Cross(localA, totalJ));

        b.physObject.AddLinearImpulse(totalJ);
        b.physObject.AddAngularImpulse(Cross(localB, totalJ));

        // correct positions to resolve overlap

        float stab = 0.5f; // stabilize movement via gradual application of correction
        float slop = 0.1f; // allow small overlap to prevent jitter
        var correction = Mathf.Max(point.penetration - slop, 0.0f) * stab * n;
        a.Properties.pos -= (massA / totalMass) * correction;
        b.Properties.pos += (massB / totalMass) * correction;
    }

    bool ObjectCollision(V2Collider a, V2Collider b, out Collision collisionInfo)
    {
        collisionInfo = new() { a = a, b = b };

        float checkDist = 1.5f * (a.maxDim + b.maxDim);
        float dist = (b.Properties.pos - a.Properties.pos).magnitude;

        if (dist > checkDist) return false;

        if (a is CircleCollider && b is CircleCollider)
        {
            return CircCirc(a as CircleCollider, b as CircleCollider, out collisionInfo);
        }
        if (a is RectangleCollider && b is CircleCollider)
        {
            return RectCirc(a as RectangleCollider, b as CircleCollider, out collisionInfo);
        }
        if (a is CircleCollider && b is RectangleCollider)
        {
            collisionInfo.a = b;
            collisionInfo.b = a;
            return RectCirc(b as RectangleCollider, a as CircleCollider, out collisionInfo);
        }
        if (a is RectangleCollider && b is RectangleCollider)
        {
            return RectRect(a as RectangleCollider, b as RectangleCollider, out collisionInfo);
        }

        return false;
    }

    bool CircCirc(CircleCollider a, CircleCollider b, out Collision collisionInfo)
    {
        collisionInfo = new() { a = a, b = b };

        float radii = a.radius + b.radius;
        Vector2 delta = b.Properties.pos - a.Properties.pos;

        float deltaLength = delta.magnitude;

        if (deltaLength < radii)
        {
            float penetration = radii - deltaLength;
            Vector2 normal = delta.normalized;
            Vector2 localA = normal * a.radius;
            Vector2 localB = -normal * b.radius;

            collisionInfo.AddContact(localA, localB, normal, penetration);
            return true;
        }

        return false;
    }

    bool RectCirc(RectangleCollider a, CircleCollider b, out Collision collisionInfo)
    {
        collisionInfo = new() { a = a, b = b };

        Vector2 relPos = b.Properties.pos - a.Properties.pos;

        float rectRot = a.Properties.Rot * Mathf.Deg2Rad;

        float cos = Mathf.Cos(-rectRot);
        float sin = Mathf.Sin(-rectRot);

        Vector2 localCircle = new(relPos.x * cos - relPos.y * sin, relPos.x * sin + relPos.y * cos);

        float w = a.w;
        float h = a.h;

        Vector2 localClosest = new(Mathf.Clamp(localCircle.x, -w, w), Mathf.Clamp(localCircle.y, -h, h));

        Vector2 distVec = new(localCircle.x - localClosest.x, localCircle.y - localClosest.y);
        float dist = distVec.magnitude;

        if (dist > b.radius) return false;

        Vector2 localNormal = new(distVec.x / dist, distVec.y / dist);

        cos = Mathf.Cos(rectRot);
        sin = Mathf.Sin(rectRot);

        Vector2 worldNormal = new(localNormal.x * cos - localNormal.y * sin, localNormal.x * sin + localNormal.y * cos);

        Vector2 contact = b.Properties.pos - (worldNormal * b.radius);

        Vector2 localA = contact - a.Properties.pos;
        Vector2 localB = contact - b.Properties.pos;

        float penetration = b.radius - dist;
        collisionInfo.AddContact(localA, localB, worldNormal, penetration);

        return true;
    }

    bool RectRect(RectangleCollider a, RectangleCollider b, out Collision collisionInfo)
    {
        collisionInfo = new() { a = a, b = b };

        var (vertsA, normA) = a.GetVerticesAndNormals();
        var (vertsB, normB) = b.GetVerticesAndNormals();

        float overlap;
        float minOverlap = Mathf.Infinity;
        Vector2 normal = Vector2.zero;
        foreach (var axis in normA)
        {
            if (CheckOverlap(axis, vertsA, vertsB, out overlap))
            {
                if (overlap < minOverlap)
                {
                    normal = axis;
                    minOverlap = overlap;
                }
            }
            else return false;
        }

        foreach (var axis in normB)
        {
            if (CheckOverlap(axis, vertsA, vertsB, out overlap))
            {
                if (overlap < minOverlap)
                {
                    normal = axis;
                    minOverlap = overlap;
                }
            }
            else return false;
        }

        RectangleCollider refShape = a;
        RectangleCollider incShape = b;
        int refEdgeInt = 0; // clockwise from 0 = top
        float greatestDot = Mathf.NegativeInfinity;
        float dot;
        for (int i = 0; i < normA.Length; i++)
        {
            dot = Vector2.Dot(normA[i], normal);
            if (dot > greatestDot)
            {
                refEdgeInt = i;
                greatestDot = dot;
            }
        }

        for (int i = 0; i < normB.Length; i++)
        {
            dot = Vector2.Dot(normB[i], normal);
            if (dot > greatestDot)
            {
                refEdgeInt = i;
                refShape = b;
                incShape = a;
                greatestDot = dot;
            }
        }

        Vector2[] refPoints = vertsA;
        if (refShape == b) refPoints = vertsB;

        var refVertices = new Vector2[2];
        refVertices[0] = refPoints[refEdgeInt];
        refVertices[1] = (refEdgeInt < refPoints.Length - 1) ? refPoints[refEdgeInt + 1] : refPoints[0];

        int incEdgeInt = 0;
        greatestDot = Mathf.NegativeInfinity;

        Vector2[] incNorms = normA;
        Vector2[] incPoints = vertsA;
        if (incShape == b)
        {
            incNorms = normB;
            incPoints = vertsB;
        }

        var negNorm = -normal;
        for (int i = 0; i < incNorms.Length; i++)
        {
            dot = Vector2.Dot(incNorms[i], negNorm);
            if (dot > greatestDot)
            {
                incEdgeInt = i;
                greatestDot = dot;
            }
        }

        var incVertices = new Vector2[2];
        incVertices[0] = incPoints[incEdgeInt];
        incVertices[1] = (incEdgeInt < incPoints.Length - 1) ? incPoints[incEdgeInt + 1] : incPoints[0];

        collisionInfo.a = incShape;
        collisionInfo.b = refShape;

        var worldNormal = (collisionInfo.b.Properties.pos - collisionInfo.a.Properties.pos).normalized;
        if (Vector2.Dot(normal, worldNormal) < 0.0f) normal *= -1.0f;

        var contact = GetContactPoint(refVertices, incVertices, normal);
        float penetration = Mathf.Abs(minOverlap);

        Vector2 localA = contact - collisionInfo.a.Properties.pos;
        Vector2 localB = contact - collisionInfo.b.Properties.pos;

        collisionInfo.AddContact(localA, localB, normal, penetration);

        return true;
    }

    Vector2 GetContactPoint(Vector2[] refVerts, Vector2[] incVerts, Vector2 normal)
    {
        var clipped = incVerts.ToList();
        clipped = ClipPoints(clipped, normal, Vector2.Dot(normal, refVerts[0]));

        var refDir = (refVerts[1] - refVerts[0]).normalized;

        Vector2 sideNormal1 = new(-refDir.y, refDir.x);
        float sideOffset1 = Vector2.Dot(sideNormal1, refVerts[0]) - EPS;
        clipped = ClipPoints(clipped, sideNormal1, sideOffset1);

        var sideNormal2 = -sideNormal1;
        float sideOffset2 = Vector2.Dot(sideNormal2, refVerts[1]) - EPS;
        clipped = ClipPoints(clipped, sideNormal2, sideOffset2);

        if (clipped.Count == 0)
        {
            return (incVerts[0] + incVerts[1]) * 0.5f;
        }

        var contact = Vector2.zero;
        foreach (var point in clipped)
        {
            contact += point;
        }
        contact /= clipped.Count;
        return contact;
    }

    List<Vector2> ClipPoints(List<Vector2> points, Vector2 normal, float o)
    {
        List<Vector2> output = new();

        for (int i = 0; i < points.Count; i++)
        {
            var v = points[i];
            float d = Vector2.Dot(v, normal) - o;
            if (d >= -EPS) output.Add(v);
            if (i == 0) continue;

            float d0 = Vector2.Dot(points[i - 1], normal) - o;
            if ((d < -EPS && d0 > EPS) || (d > EPS && d0 < -EPS))
            {
                var e = v - points[i - 1];
                float u = -d0 / (d - d0);
                output.Add(e * u + points[i - 1]);
            }
        }
        return output;
    }

    bool CheckOverlap(Vector2 axis, Vector2[] vertsA, Vector2[] vertsB, out float overlap)
    {
        var intervalA = CalcInterval(axis, vertsA);
        var intervalB = CalcInterval(axis, vertsB);

        overlap = Mathf.Min(intervalA.y, intervalB.y) - Mathf.Max(intervalA.x, intervalB.x);

        return intervalA.y >= intervalB.x && intervalB.y >= intervalA.x;
    }

    Vector2 CalcInterval(Vector2 axis, Vector2[] verts)
    {
        Vector2 interval = new(Mathf.Infinity, Mathf.NegativeInfinity);

        float scalar;
        foreach (var vert in verts)
        {
            scalar = Vector2.Dot(vert, axis);
            interval = new(Mathf.Min(scalar, interval.x), Mathf.Max(scalar, interval.y));
        }

        return interval;
    }
}

public struct Collision
{
    public V2Collider a;
    public V2Collider b;
    public ContactPoint point;

    public void AddContact(Vector2 localA, Vector2 localB, Vector2 normal, float penetration)
    {
        point = new() { localA = localA, localB = localB, normal = normal, penetration = penetration };
    }
}

public struct ContactPoint
{
    public Vector2 localA;
    public Vector2 localB;
    public Vector2 normal;
    public float penetration;
}

public static class VectorUtils
{
    public static float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    public static Vector2 Cross(float s, Vector2 v)
    {
        return new(-s * v.y, s * v.x);
    }
}
