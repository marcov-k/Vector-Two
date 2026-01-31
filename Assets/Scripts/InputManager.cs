using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using TMPro;
using static Constants;

public class InputManager : MonoBehaviour
{
    public static bool Paused { get; private set; } = false;
    Inputs inputs;
    [SerializeField] CinemachineCamera followCam;
    [SerializeField] Transform camPos;
    [SerializeField] TextMeshProUGUI simSpeedText;
    [SerializeField] GameObject help;
    Transform helpTextCont;
    [SerializeField] TextMeshProUGUI helpTextPrefab;
    [SerializeField] string[] keybinds;
    [SerializeField] float zoomSpeed = 1.0f;
    [SerializeField] float zoomMult = 4.0f;
    [SerializeField] float minZoom = 1.0f;
    [SerializeField] float moveSpeed = 1.0f;
    [SerializeField] float moveSpeedMult = 4.0f;
    [SerializeField] float simSpeedStep = 0.1f;

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
        inputs.Player.Scroll.performed += OnScroll;
        inputs.Player.Quit.performed += _ => OnQuit();
        inputs.Player.Help.performed += _ => OnHelp();
        inputs.Player.Save.performed += _ => OnSave();
        inputs.Player.Load.performed += _ => OnLoad();
        inputs.Player.Reset.performed += _ => OnReset();
    }

    void InitHelp()
    {
        UpdateSimSpeedText();
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

    void UpdateSimSpeedText()
    {
        simSpeedText.text = $"{simSpeed:F2}x";
        if (Paused) simSpeedText.text += " (P)";
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
        UpdateSimSpeedText();
    }

    public void OnSave()
    {
        if (Ctrl())
        {
            // todo - add file namer and saver
        }
    }

    public void OnLoad()
    {
        if (Ctrl())
        {
            // todo - add file explorer and loader
        }
    }

    public void OnReset()
    {
        if (Ctrl())
        {
            // todo - add loading of last saved state
        }
    }

    public void OnZoom(InputAction.CallbackContext ctx)
    {
        float input = ctx.ReadValue<float>();
        float speed = Boost() ? zoomSpeed * zoomMult : zoomSpeed;
        followCam.Lens.OrthographicSize = Mathf.Max(minZoom, followCam.Lens.OrthographicSize - input * speed);
    }

    public void OnScroll(InputAction.CallbackContext ctx)
    {
        if (Modifier() || Boost()) OnZoom(ctx);
        else if (Ctrl()) OnSimSpeed(ctx);
    }

    void OnSimSpeed(InputAction.CallbackContext ctx)
    {
        float input = ctx.ReadValue<float>();
        simSpeed = Mathf.Max(simSpeed + simSpeedStep * input, 0.0f);
        UpdateSimSpeedText();
    }

    public void OnQuit()
    {
        if (Ctrl()) Application.Quit();
    }

    public void OnHelp()
    {
        help.SetActive(!help.activeSelf);
    }

    bool Modifier()
    {
        return inputs.Player.Modifier.IsPressed();
    }

    bool Boost()
    {
        return inputs.Player.Speed.IsPressed();
    }

    bool Ctrl()
    {
        return inputs.Player.Control.IsPressed();
    }

    bool Alt()
    {
        return inputs.Player.Alt.IsPressed();
    }
}
