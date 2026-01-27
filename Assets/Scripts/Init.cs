using UnityEngine;
using static Constants;

public class Init : MonoBehaviour
{
    [SerializeField] float PhysicsFramerate = 120.0f;
    [SerializeField] float VisualsFramerate = 80.0f;
    [SerializeField] float minimumGravity = 0.01f;
    [SerializeField] bool fakeDrag = false;
    [SerializeField] float dragDecay = 1.0f; // percent per second

    void Awake()
    {
        InitTimestep();
    }

    void InitTimestep()
    {
        physFreq = (PhysicsFramerate > 0) ? PhysicsFramerate : physFreq;
        visFreq = (VisualsFramerate > 0) ? VisualsFramerate : visFreq;
        minGravA = (minimumGravity >= 0) ? minimumGravity : minGravA;
        drag = fakeDrag ? dragDecay : 0.0f;
        physTimestep = 1.0f / physFreq;
        visTimestep = 1.0f / visFreq;
    }
}
