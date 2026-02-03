using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using TMPro;
using static Constants;
using static Saver;
using static FileManager;

public class InputManager : MonoBehaviour
{
    public static bool Paused { get; private set; } = true;
    Inputs inputs;
    [SerializeField] CinemachineCamera followCam;
    [SerializeField] Transform camPos;
    [SerializeField] TextMeshProUGUI simSpeedText;
    [SerializeField] GameObject help;
    Transform helpTextCont;
    [SerializeField] TextMeshProUGUI helpTextPrefab;
    FileManager fileManager;
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
        fileManager = FindFirstObjectByType<FileManager>();
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
        inputs.Player.Pause.performed += _ => { if (!FilesOpen) OnPause(); };
        inputs.Player.Scroll.performed += ctx => { if (!FilesOpen) OnScroll(ctx); };
        inputs.Player.Quit.performed += _ => { if (!FilesOpen) OnQuit(); };
        inputs.Player.Help.performed += _ => { if (!FilesOpen) OnHelp(); };
        inputs.Player.Reset.performed += _ => { if (!FilesOpen) OnReset(); };
        inputs.Player.FileView.performed += _ => OnFileView();
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
        if (!FilesOpen) Move();
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

    public void OnFileView()
    {
        if (Ctrl())
        {
            Paused = true;
            UpdateSimSpeedText();
            fileManager.ToggleViewer();
        }
    }

    public void OnReset()
    {
        if (Ctrl())
        {
            Paused = true;
            UpdateSimSpeedText();
            ResetState();
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
