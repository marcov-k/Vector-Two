using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using TMPro;

public class InputManager : MonoBehaviour
{
    public static bool Paused { get; private set; } = false;
    Inputs inputs;
    [SerializeField] CinemachineCamera followCam;
    [SerializeField] Transform camPos;
    [SerializeField] GameObject help;
    Transform helpTextCont;
    [SerializeField] TextMeshProUGUI helpTextPrefab;
    [SerializeField] string[] keybinds;
    [SerializeField] float zoomSpeed = 1.0f;
    [SerializeField] float zoomMult = 4.0f;
    [SerializeField] float minZoom = 1.0f;
    [SerializeField] float moveSpeed = 1.0f;
    [SerializeField] float moveSpeedMult = 4.0f;

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
        InitInputs();
        InitHelp();
    }

    void InitInputs()
    {
        inputs.Player.Pause.performed += _ => OnPause();
        inputs.Player.Zoom.performed += OnZoom;
        inputs.Player.Quit.performed += _ => OnQuit();
        inputs.Player.Help.performed += _ => OnHelp();
    }

    void InitHelp()
    {
        helpTextCont = help.transform.GetChild(0).GetChild(0);

        foreach (Transform child in helpTextCont)
        {
            Destroy(child.gameObject);
        }

        foreach (var keybind in keybinds)
        {
            var textBox = Instantiate(helpTextPrefab, helpTextCont).GetComponent<TextMeshProUGUI>();
            textBox.text = $"- {keybind}";
        }
    }

    void Update()
    {
        Move();
    }

    void Move()
    {
        bool boost = Boost();
        if (Modifier() || boost)
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
        bool boost = Boost();
        if (Modifier() || boost)
        {
            float input = ctx.ReadValue<float>();
            float speed = boost ? zoomSpeed * zoomMult : zoomSpeed;
            followCam.Lens.OrthographicSize = Mathf.Max(minZoom, followCam.Lens.OrthographicSize - input * speed);
        }
    }

    public void OnQuit()
    {
        if (Control()) Application.Quit();
    }

    public void OnHelp()
    {
        if (Control()) help.SetActive(!help.activeSelf);
    }

    bool Modifier()
    {
        return inputs.Player.Modifier.IsPressed();
    }

    bool Boost()
    {
        return inputs.Player.Speed.IsPressed();
    }

    bool Control()
    {
        return inputs.Player.Control.IsPressed();
    }
}
