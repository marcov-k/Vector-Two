using UnityEngine;
using static Constants;

public class Init : MonoBehaviour
{
    [SerializeField] float PhysicsFramerate = 120.0f;
    [SerializeField] float VisualsFramerate = 80.0f;

    void Awake()
    {
        InitTimestep();
    }

    void InitTimestep()
    {
        physFreq = (PhysicsFramerate > 0) ? PhysicsFramerate : physFreq;
        visFreq = (VisualsFramerate > 0) ? VisualsFramerate : visFreq;
        physTimestep = 1.0f / physFreq;
        visTimestep = 1.0f / visFreq;
    }
}
