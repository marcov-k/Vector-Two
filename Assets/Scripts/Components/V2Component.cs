using UnityEngine;
using System.Collections;
using static Constants;
using static InputManager;
using static Saver;

public class V2Component : MonoBehaviour
{
    public Properties Properties { get; protected set; }
    protected IEnumerator PhysUpdateCoroutine;
    protected IEnumerator VisUpdateCoroutine;

    protected void Awake()
    {
        InitObject();
    }

    protected virtual void InitObject()
    {
        Properties = GetComponent<Properties>() ? GetComponent<Properties>() : gameObject.AddComponent<Properties>();
    }

    protected void Start()
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
            if (!Paused && !Loading) PhysUpdate();
            yield return new WaitForSeconds(physTimestep);
        }
    }

    protected virtual void PhysUpdate() { }

    protected IEnumerator VisUpdateLoop()
    {
        while (true)
        {
            if (!Paused && !Loading) VisUpdate();
            yield return new WaitForSeconds(visTimestep);
        }
    }

    protected virtual void VisUpdate() { }
}
