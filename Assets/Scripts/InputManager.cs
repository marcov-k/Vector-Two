using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class InputManager : MonoBehaviour
{
    public static bool Paused { get; private set; } = false;
    Inputs inputs;
    [SerializeField] CinemachineCamera followCam;
    [SerializeField] Transform camPos;
    [SerializeField] float zoomSpeed = 1.0f;
    [SerializeField] float zoomMult = 4.0f;
    [SerializeField] float minZoom = 1.0f;
    [SerializeField] float moveSpeed = 1.0f;
    [SerializeField] float moveSpeedMult = 4.0f;
    bool modifier = false;
    bool boost = false;

    void Awake()
    {
        inputs = new();
    }

    void OnEnable()
    {
        inputs.Enable();
    }

    void OnDisable()
    {
        inputs.Disable();
    }

    void Start()
    {
        inputs.Player.Pause.performed += _ => OnPause();
        inputs.Player.Zoom.performed += OnZoom;
    }

    void Update()
    {
        CheckMods();
        Move();
    }

    void CheckMods()
    {
        modifier = inputs.Player.Modifier.IsPressed();
        boost = inputs.Player.Speed.IsPressed();
    }

    void Move()
    {
        if (modifier || boost)
        {
            var move = inputs.Player.Move.ReadValue<Vector2>();
            float speed = boost ? moveSpeed * moveSpeedMult : moveSpeed;
            camPos.position += (Vector3)(speed * Time.unscaledDeltaTime * move);
        }
    }

    public void OnPause()
    {
        Paused = !Paused;
    }

    public void OnZoom(InputAction.CallbackContext ctx)
    {
        if (modifier || boost)
        {
            float input = ctx.ReadValue<float>();
            float speed = boost ? zoomSpeed * zoomMult : zoomSpeed;
            followCam.Lens.OrthographicSize = Mathf.Max(minZoom, followCam.Lens.OrthographicSize - input * speed);
        }
    }
}
