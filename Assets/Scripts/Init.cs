using UnityEngine;
using static Constants;

public class Init : MonoBehaviour
{
    void Awake()
    {
        InitTimestep();
    }

    void InitTimestep()
    {
        physTimestep = 1.0f / physFreq;
        visTimestep = 1.0f / visFreq;
    }
}
