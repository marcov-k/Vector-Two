using UnityEngine;
using System.Collections;
using static Constants;

public class V2Component : MonoBehaviour
{
    protected Properties Properties;
    protected IEnumerator PhysUpdateCoroutine;
    protected IEnumerator VisUpdateCoroutine;

    protected virtual void Awake()
    {
        InitObject();
    }

    protected virtual void InitObject()
    {
        Properties = GetComponent<Properties>() ? GetComponent<Properties>() : gameObject.AddComponent<Properties>();
    }

    protected virtual void Start()
    {
        InitValues();
        InitUpdateLoops();
    }

    protected virtual void InitValues() { }

    protected void InitUpdateLoops()
    {
        PhysUpdateCoroutine = PhysUpdateLoop();
        VisUpdateCoroutine = VisUpdateLoop();
        StartCoroutine(PhysUpdateCoroutine);
        StartCoroutine(VisUpdateCoroutine);
    }

    protected IEnumerator PhysUpdateLoop()
    {
        while (true)
        {
            PhysUpdate();
            yield return new WaitForSeconds(physTimestep);
        }
    }

    protected virtual void PhysUpdate() { }

    protected IEnumerator VisUpdateLoop()
    {
        while (true)
        {
            VisUpdate();
            yield return new WaitForSeconds(visTimestep);
        }
    }

    protected virtual void VisUpdate() { }

    public Properties GetProperties()
    {
        return Properties;
    }
}
