using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerController controls;

    // Eventos de cámara
    public event Action<Vector2> OnCameraMove;
    public event Action<float> OnCameraZoom;

    // Eventos de gameplay - USANDO LOS NOMBRES CORRECTOS DE TU PLAYERCONTROLLER
    public event Action OnPause;
    public event Action OnRestart;
    public event Action OnClear;
    public event Action OnToggleCell;
    public event Action OnToggleControls;
    public event Action OnToggleVisualization;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("InputManager: Inicializando controles...");
        controls = new PlayerController();
        SetupInputCallbacks();
    }

    void SetupInputCallbacks()
    {
        // Cámara
        controls.Camera.Move.performed += ctx => OnCameraMove?.Invoke(ctx.ReadValue<Vector2>());
        controls.Camera.Move.canceled += ctx => OnCameraMove?.Invoke(Vector2.zero);
        controls.Camera.Zoom.performed += ctx => OnCameraZoom?.Invoke(ctx.ReadValue<float>());
        controls.Camera.Zoom.canceled += ctx => OnCameraZoom?.Invoke(0);

        // Gameplay - USANDO LOS NOMBRES EXACTOS DE TU PLAYERCONTROLLER
        controls.Gameplay.Pause.performed += _ =>
        {
            Debug.Log("InputManager: Tecla Pausa presionada (ESC/Start)");
            OnPause?.Invoke();
        };

        controls.Gameplay.Restart.performed += _ =>
        {
            Debug.Log("InputManager: Tecla Reiniciar presionada (R/North)");
            OnRestart?.Invoke();
        };

        controls.Gameplay.Clear.performed += _ =>
        {
            Debug.Log("InputManager: Tecla Limpiar presionada");
            OnClear?.Invoke();
        };

        controls.Gameplay.ToggleCell.performed += _ =>
        {
            Debug.Log("InputManager: Tecla ToggleCell presionada");
            OnToggleCell?.Invoke();
        };

        controls.Gameplay.ToggleControls.performed += _ =>
        {
            Debug.Log("InputManager: Tecla ToggleControls presionada (TAB/Select)");
            OnToggleControls?.Invoke();
        };

        controls.Gameplay.ToggleVisualization.performed += _ =>
        {
            Debug.Log("InputManager: Tecla ToggleVisualization presionada");
            OnToggleVisualization?.Invoke();
        };

        Debug.Log("InputManager: Todos los callbacks configurados");
    }

    void OnEnable()
    {
        Debug.Log("InputManager: Activando controles...");
        controls.Camera.Enable();
        controls.Gameplay.Enable();
        Debug.Log("InputManager: Controles activados");
    }

    void OnDisable()
    {
        Debug.Log("InputManager: Desactivando controles...");
        controls.Camera.Disable();
        controls.Gameplay.Disable();
    }

    void Start()
    {
        Debug.Log("InputManager: Start completado");
    }

    // Método para probar input manualmente
    void Update()
    {
        // Debug manual de teclas
        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Debug.Log("InputManager (Update): ESC detectado directamente");
                OnPause?.Invoke();
            }

            if (Keyboard.current.tabKey.wasPressedThisFrame)
            {
                Debug.Log("InputManager (Update): TAB detectado directamente");
                OnToggleControls?.Invoke();
            }
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.startButton.wasPressedThisFrame)
            {
                Debug.Log("InputManager (Update): Start button detectado directamente");
                OnPause?.Invoke();
            }

            if (Gamepad.current.selectButton.wasPressedThisFrame)
            {
                Debug.Log("InputManager (Update): Select button detectado directamente");
                OnToggleControls?.Invoke();
            }
        }
    }
}