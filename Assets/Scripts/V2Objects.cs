using System.Collections.Generic;
using UnityEngine;

public static class V2Objects
{
    public static readonly List<PhysObject> physObjects = new();
    public static readonly List<Gravity> gravities = new();
    public static readonly List<V2Collider> colliders = new();

    /// <summary>
    /// Clear all existing physics objects.
    /// </summary>
    public static void ClearState()
    {
        foreach (var obj in physObjects)
        {
            GameObject.Destroy(obj.gameObject);
        }

        physObjects.Clear();
        gravities.Clear();
        colliders.Clear();
    }
}
