using UnityEngine;
using System.Collections;
using static Constants;
using static V2Objects;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

public class CollisionManager : MonoBehaviour
{
    IEnumerator collisionCoroutine;
    [SerializeField] GameObject markerPrefab;

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
            CheckCollisions();
        }
    }

    void CheckCollisions()
    {
        for (int i = 0; i < colliders.Count; i++)
        {
            for (int j = i + 1; j < colliders.Count; j++)
            {
                if (ObjectCollision(colliders[i], colliders[j], out var collision))
                {
                    ImpulseResolveCollision(collision);
                }
            }
        }
    }

    void ImpulseResolveCollision(Collision collisionInfo)
    {
        var point = collisionInfo.point;
        var a = collisionInfo.a;
        var b = collisionInfo.b;

        float massA = 1.0f / a.Properties.m;
        float massB = 1.0f / b.Properties.m;
        float totalMass = massA + massB;

        var pointPos = point.localA + (Vector2)a.Properties.pos;

        a.Properties.pos -= (float2)((massA / totalMass) * point.penetration * point.normal);
        b.Properties.pos += (float2)((massB / totalMass) * point.penetration * point.normal);

        var localA = pointPos - (Vector2)a.Properties.pos;
        var localB = pointPos - (Vector2)b.Properties.pos;

        float avA = a.Properties.av;
        float avB = b.Properties.av;

        var angVelA = Perp(avA, localA);
        var angVelB = Perp(avB, localB);

        var fullVelA = (Vector2)a.Properties.v + angVelA;
        var fullVelB = (Vector2)b.Properties.v + angVelB;

        var contactVel = fullVelB - fullVelA;

        float impulseForce = Vector2.Dot(contactVel, point.normal);

        float invInertiaA = 1.0f / a.Properties.moi;
        float invInertiaB = 1.0f / b.Properties.moi;

        var inertiaA = Cross(invInertiaA * Cross(localA, point.normal), localA);
        var inertiaB = Cross(invInertiaB * Cross(localB, point.normal), localB);

        float angularEffect = Vector2.Dot(inertiaA + inertiaB, point.normal);

        float cRestitution = a.Properties.e * b.Properties.e;

        float j = (-(1.0f + cRestitution) * impulseForce) / (totalMass + angularEffect);

        var fullImpulse = point.normal * j;

        a.physObject.AddLinearImpulse(-fullImpulse);
        b.physObject.AddLinearImpulse(fullImpulse);

        a.physObject.AddAngularImpulse(Cross(localA, -fullImpulse));
        b.physObject.AddAngularImpulse(Cross(localB, fullImpulse));
    }

    bool ObjectCollision(V2Collider a, V2Collider b, out Collision collisionInfo)
    {
        collisionInfo = new() { a = a, b = b };

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

        Vector2 contact = (Vector2)b.Properties.pos - (worldNormal * b.radius);

        Vector2 localA = contact - (Vector2)a.Properties.pos;
        Vector2 localB = contact - (Vector2)b.Properties.pos;

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
            overlap = CalcOverlap(axis, vertsA, vertsB);

            if (overlap <= 0.0f) return false;
            else if (overlap < minOverlap)
            {
                normal = axis;
                minOverlap = overlap;
            }
        }

        foreach (var axis in normB)
        {
            overlap = CalcOverlap(axis, vertsA, vertsB);

            if (overlap <= 0.0f) return false;
            else if (overlap < minOverlap)
            {
                normal = axis;
                minOverlap = overlap;
            }
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

        Vector2 negNorm = normal * -1.0f;

        int incEdgeInt = 0;
        greatestDot = Mathf.NegativeInfinity;

        Vector2[] incNorms = normA;
        Vector2[] incPoints = vertsA;
        if (incShape == b)
        {
            incNorms = normB;
            incPoints = vertsB;
        }

        for (int i = 0; i < incNorms.Length; i++)
        {
            dot = Vector2.Dot(incNorms[i], normal);
            if (dot > greatestDot)
            {
                incEdgeInt = i;
                greatestDot = dot;
            }
        }

        var incVertices = new Vector2[2];
        incVertices[0] = incPoints[incEdgeInt];
        incVertices[1] = (incEdgeInt < incPoints.Length - 1) ? incPoints[incEdgeInt + 1] : incPoints[0];

        collisionInfo.a = refShape;
        collisionInfo.b = incShape;

        Vector2 refVector = (refVertices[1] - refVertices[0]).normalized;

        var posSideNormal = refVector;
        float posSideOffset = Vector2.Dot(posSideNormal, refVertices[1]);

        var negSideNormal = refVector * -1.0f;
        float negSideOffset = Vector2.Dot(negSideNormal, refVertices[0]);

        var clipped = Clip(incVertices.ToList(), normal, negSideOffset);
        clipped = Clip(clipped, normal, posSideOffset);

        var faceNormal = normal;
        float faceOffset = Vector2.Dot(faceNormal, refVertices[0]);

        List<Vector2> finalContacts = new();
        List<float> depths = new();
        float depth;
        foreach (var point in clipped)
        {
            depth = Vector2.Dot(faceNormal, point) - faceOffset;
            if (depth <= 0.0f)
            {
                finalContacts.Add(point);
                depths.Add(depth);
            }
        }

        var (contact, penetration) = GetSingleContact(finalContacts, depths);

        Vector2 localA = contact - (Vector2)collisionInfo.a.Properties.pos;
        Vector2 localB = contact - (Vector2)collisionInfo.b.Properties.pos;

        collisionInfo.AddContact(localA, localB, normal, penetration);

        return true;
    }

    List<Vector2> Clip(List<Vector2> points, Vector2 normal, float offset)
    {
        List<Vector2> clippedPoints = new();

        float d1 = Vector2.Dot(points[0], normal) - offset;
        float d2 = Vector2.Dot(points[1], normal) - offset;

        if (d1 <= 0.0f) clippedPoints.Add(points[0]);
        if (d2 <= 0.0f) clippedPoints.Add(points[1]);

        if (d1 * d2 < 0.0f)
        {
            float alpha = d1 / (d1 - d2);
            var intersect = points[0] + ((points[1] - points[0]) * alpha);
            clippedPoints.Add(intersect);
        }

        return clippedPoints;
    }

    (Vector2 point, float penetration) GetSingleContact(List<Vector2> contacts, List<float> depths)
    {
        Vector2 deepestContact = contacts[0];
        float maxDepth = depths[0];
        for (int i = 1; i < contacts.Count; i++)
        {
            if (depths[i] > maxDepth)
            {
                maxDepth = depths[i];
                deepestContact = contacts[i];
            }
        }

        return (deepestContact, maxDepth);
    }

    float CalcOverlap(Vector2 axis, Vector2[] vertsA, Vector2[] vertsB)
    {
        var intervalA = CalcInterval(axis, vertsA);
        var intervalB = CalcInterval(axis, vertsB);

        float overlap = Mathf.Min(intervalA.y, intervalB.y) - Mathf.Max(intervalA.x, intervalB.x);

        return overlap;
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

    Vector2 Perp(float omega, Vector2 v)
    {
        return new(-omega * v.y, omega * v.x);
    }

    float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    Vector2 Cross(float s, Vector2 v)
    {
        return new(-s * v.y, s * v.x);
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
