using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Main UI Panels")]
    public GameObject simulationPanel;  // Panel de controles
    public GameObject pausePanel;       // Panel de pausa
    public TextMeshProUGUI pauseText;

    [Header("Mana Flow Controls")]
    public Slider manaSpeedSlider;
    public Slider manaDensitySlider;
    public Toggle manaAutoToggle;
    public TextMeshProUGUI manaSpeedText;
    public TextMeshProUGUI manaDensityText;

    [Header("Corruption Controls")]
    public Slider corruptionSpeedSlider;
    public Slider corruptionStrengthSlider;
    public Toggle corruptionAutoToggle;
    public TextMeshProUGUI corruptionSpeedText;
    public TextMeshProUGUI corruptionStrengthText;

    [Header("Events Controls")]
    public Slider eventsIntervalSlider;
    public Toggle eventsEnabledToggle;
    public TextMeshProUGUI eventsIntervalText;

    [Header("Fauna Controls")]
    public Slider faunaSpawnSlider;
    public Slider faunaMaxSlider;
    public Toggle faunaAutoToggle;
    public TextMeshProUGUI faunaSpawnText;
    public TextMeshProUGUI faunaMaxText;

    [Header("Units Controls")]
    public Slider unitSpeedSlider;
    public Slider unitVisionSlider;
    public TextMeshProUGUI unitSpeedText;
    public TextMeshProUGUI unitVisionText;

    [Header("Resource Display")]
    public TextMeshProUGUI manaResourceText;
    public TextMeshProUGUI corruptionResourceText;
    public TextMeshProUGUI factionText;
    public TextMeshProUGUI buildingCountText;

    [Header("Audio Controls")]
    public Slider masterVolumeSlider;
    public TextMeshProUGUI masterVolumeText;

    [Header("UI Navigation")]
    public Selectable firstSelectedOnPause;
    public Selectable firstSelectedOnControls;

    [Header("UI Buttons")]
    public Button resumeButton;
    public Button restartButton;
    public Button quitButton;
    public Button togglePanelButton;

    private EventSystem eventSystem;
    private bool controlsPanelVisible = false;  // Empezar con controles OCULTOS

    // Variables para control de input manual
    private bool escapePressed = false;
    private bool tabPressed = false;
    private bool startPressed = false;
    private bool selectPressed = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        eventSystem = EventSystem.current;

        // INICIALIZAR EN ORDEN CORRECTO
        InitializeUI();
        SetupEventListeners();
        SetupButtonListeners();
        SetupUINavigation();

        // Suscribirse a eventos del GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPauseStateChanged += OnPauseStateChanged;
        }

        Debug.Log("UIManager: Inicializado - Controles ocultos, Pausa desactivada");
    }

    void OnDestroy()
    {
        // Desuscribirse de eventos
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPauseStateChanged -= OnPauseStateChanged;
        }
    }

    void InitializeUI()
    {
        Debug.Log("UIManager: Inicializando UI...");

        // 1. PRIMERO asegurarnos de que el tiempo esté corriendo
        Time.timeScale = 1f;

        // 2. INICIALMENTE OCULTAR TODOS LOS PANELES
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            Debug.Log("UIManager: Panel de pausa oculto");
        }

        if (simulationPanel != null)
        {
            simulationPanel.SetActive(false);
            controlsPanelVisible = false;
            Debug.Log("UIManager: Panel de controles oculto");
        }

        // 3. Inicializar valores de sliders con los valores actuales de las simulaciones
        InitializeSliderValues();

        // 4. Configurar volumen
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = AudioListener.volume;
        }

        Debug.Log("UIManager: UI inicializada correctamente");
    }

    void InitializeSliderValues()
    {
        ManaFlowSimulation manaFlow = FindObjectOfType<ManaFlowSimulation>();
        if (manaFlow != null)
        {
            if (manaSpeedSlider != null) manaSpeedSlider.value = manaFlow.updateTime;
            if (manaDensitySlider != null) manaDensitySlider.value = manaFlow.initialManaDensity;
            if (manaAutoToggle != null) manaAutoToggle.isOn = manaFlow.autoSimulate;
        }

        CorruptionSimulation corruption = FindObjectOfType<CorruptionSimulation>();
        if (corruption != null)
        {
            if (corruptionSpeedSlider != null) corruptionSpeedSlider.value = corruption.updateTime;
            if (corruptionStrengthSlider != null) corruptionStrengthSlider.value = corruption.tasaBase;
            if (corruptionAutoToggle != null) corruptionAutoToggle.isOn = corruption.autoSimulate;
        }

        MagicEventsSimulation events = FindObjectOfType<MagicEventsSimulation>();
        if (events != null)
        {
            if (eventsIntervalSlider != null) eventsIntervalSlider.value = events.eventCheckInterval;
            if (eventsEnabledToggle != null) eventsEnabledToggle.isOn = events.eventsEnabled;
        }

        MagicalFaunaSimulation fauna = FindObjectOfType<MagicalFaunaSimulation>();
        if (fauna != null)
        {
            if (faunaSpawnSlider != null) faunaSpawnSlider.value = fauna.updateTime;
            if (faunaMaxSlider != null) faunaMaxSlider.value = fauna.maxFauna;
            if (faunaAutoToggle != null) faunaAutoToggle.isOn = fauna.autoSimulate;
        }
    }

    void SetupEventListeners()
    {
        // Mana Flow
        if (manaSpeedSlider != null) manaSpeedSlider.onValueChanged.AddListener(OnManaSpeedChanged);
        if (manaDensitySlider != null) manaDensitySlider.onValueChanged.AddListener(OnManaDensityChanged);
        if (manaAutoToggle != null) manaAutoToggle.onValueChanged.AddListener(OnManaAutoToggle);

        // Corruption
        if (corruptionSpeedSlider != null) corruptionSpeedSlider.onValueChanged.AddListener(OnCorruptionSpeedChanged);
        if (corruptionStrengthSlider != null) corruptionStrengthSlider.onValueChanged.AddListener(OnCorruptionStrengthChanged);
        if (corruptionAutoToggle != null) corruptionAutoToggle.onValueChanged.AddListener(OnCorruptionAutoToggle);

        // Events
        if (eventsIntervalSlider != null) eventsIntervalSlider.onValueChanged.AddListener(OnEventsIntervalChanged);
        if (eventsEnabledToggle != null) eventsEnabledToggle.onValueChanged.AddListener(OnEventsEnabledToggle);

        // Fauna
        if (faunaSpawnSlider != null) faunaSpawnSlider.onValueChanged.AddListener(OnFaunaSpawnChanged);
        if (faunaMaxSlider != null) faunaMaxSlider.onValueChanged.AddListener(OnFaunaMaxChanged);
        if (faunaAutoToggle != null) faunaAutoToggle.onValueChanged.AddListener(OnFaunaAutoToggle);

        // Units
        if (unitSpeedSlider != null) unitSpeedSlider.onValueChanged.AddListener(OnUnitSpeedChanged);
        if (unitVisionSlider != null) unitVisionSlider.onValueChanged.AddListener(OnUnitVisionChanged);

        // Audio
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
    }

    void SetupButtonListeners()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeButton);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButton);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitButton);

        if (togglePanelButton != null)
            togglePanelButton.onClick.AddListener(OnTogglePanelButton);
    }

    void SetupUINavigation()
    {
        // Configurar navegación para sliders
        if (manaSpeedSlider != null && manaDensitySlider != null && manaAutoToggle != null)
        {
            ConfigureSliderNavigation(manaSpeedSlider, manaDensitySlider, manaAutoToggle);
            ConfigureSliderNavigation(manaDensitySlider, manaAutoToggle, manaSpeedSlider);
        }

        if (corruptionSpeedSlider != null && corruptionStrengthSlider != null && corruptionAutoToggle != null)
        {
            ConfigureSliderNavigation(corruptionSpeedSlider, corruptionStrengthSlider, corruptionAutoToggle);
            ConfigureSliderNavigation(corruptionStrengthSlider, corruptionAutoToggle, corruptionSpeedSlider);
        }

        // Configurar navegación para botones de pausa
        if (resumeButton != null && restartButton != null && quitButton != null)
        {
            Navigation resumeNav = resumeButton.navigation;
            resumeNav.mode = Navigation.Mode.Explicit;
            resumeNav.selectOnDown = restartButton;
            resumeNav.selectOnUp = quitButton;
            resumeButton.navigation = resumeNav;

            Navigation restartNav = restartButton.navigation;
            restartNav.mode = Navigation.Mode.Explicit;
            restartNav.selectOnDown = quitButton;
            restartNav.selectOnUp = resumeButton;
            restartButton.navigation = restartNav;

            Navigation quitNav = quitButton.navigation;
            quitNav.mode = Navigation.Mode.Explicit;
            quitNav.selectOnDown = resumeButton;
            quitNav.selectOnUp = restartButton;
            quitNav.selectOnRight = resumeButton;
            quitNav.selectOnLeft = resumeButton;
            quitButton.navigation = quitNav;
        }
    }

    void ConfigureSliderNavigation(Slider current, Selectable downElement, Selectable upElement)
    {
        Navigation nav = current.navigation;
        nav.mode = Navigation.Mode.Explicit;
        nav.selectOnDown = downElement;
        nav.selectOnUp = upElement;
        // Añadir navegación horizontal para sliders
        nav.selectOnLeft = current;
        nav.selectOnRight = current;
        current.navigation = nav;
    }

    void Update()
    {
        // ========== DETECCIÓN MANUAL DE INPUT ==========
        HandleManualInput();

        // Si el juego está pausado, manejar navegación especial
        if (GameManager.Instance != null && GameManager.Instance.isGamePaused)
        {
            HandlePauseNavigation();
        }
        else
        {
            // Solo actualizar recursos y textos si no está pausado
            UpdateResourceDisplay();
            UpdateSliderTexts();
            HandleGamepadNavigation();
        }
    }

    void HandleManualInput()
    {
        // Detectar teclado
        if (Keyboard.current != null)
        {
            // ESC para pausa
            if (Keyboard.current.escapeKey.wasPressedThisFrame && !escapePressed)
            {
                escapePressed = true;
                Debug.Log("INPUT MANUAL: ESC detectado - Alternando pausa");
                TogglePause();
            }
            else if (Keyboard.current.escapeKey.wasReleasedThisFrame)
            {
                escapePressed = false;
            }

            // TAB para controles - SOLO si no estamos en pausa
            if (!GameManager.Instance.isGamePaused && Keyboard.current.tabKey.wasPressedThisFrame && !tabPressed)
            {
                tabPressed = true;
                Debug.Log("INPUT MANUAL: TAB detectado - Alternando panel de controles");
                ToggleSimulationPanel();
            }
            else if (Keyboard.current.tabKey.wasReleasedThisFrame)
            {
                tabPressed = false;
            }

            // R para reiniciar - SOLO si no estamos en pausa
            if (!GameManager.Instance.isGamePaused && Keyboard.current.rKey.wasPressedThisFrame)
            {
                Debug.Log("INPUT MANUAL: R detectado - Reiniciando simulaciones");
                RestartSimulations();
            }
        }

        // Detectar gamepad
        if (Gamepad.current != null)
        {
            // START para pausa
            if (Gamepad.current.startButton.wasPressedThisFrame && !startPressed)
            {
                startPressed = true;
                Debug.Log("INPUT MANUAL: Gamepad START detectado - Alternando pausa");
                TogglePause();
            }
            else if (Gamepad.current.startButton.wasReleasedThisFrame)
            {
                startPressed = false;
            }

            // SELECT para controles - SOLO si no estamos en pausa
            if (!GameManager.Instance.isGamePaused && Gamepad.current.selectButton.wasPressedThisFrame && !selectPressed)
            {
                selectPressed = true;
                Debug.Log("INPUT MANUAL: Gamepad SELECT detectado - Alternando panel de controles");
                ToggleSimulationPanel();
            }
            else if (Gamepad.current.selectButton.wasReleasedThisFrame)
            {
                selectPressed = false;
            }

            // Gamepad North button (Y en Xbox, Triangle en PlayStation) para reiniciar - SOLO si no estamos en pausa
            if (!GameManager.Instance.isGamePaused && Gamepad.current.buttonNorth.wasPressedThisFrame)
            {
                Debug.Log("INPUT MANUAL: Gamepad North detectado - Reiniciando simulaciones");
                RestartSimulations();
            }
        }
    }

    void HandleGamepadNavigation()
    {
        if (eventSystem == null) return;

        // Solo manejar navegación si hay un gamepad conectado
        if (Gamepad.current != null)
        {
            // Si no hay elemento seleccionado y el panel de controles está visible, seleccionar el primero
            if (eventSystem.currentSelectedGameObject == null)
            {
                if (controlsPanelVisible && firstSelectedOnControls != null)
                {
                    eventSystem.SetSelectedGameObject(firstSelectedOnControls.gameObject);
                    Debug.Log("UIManager: Primer control seleccionado automáticamente");
                }
            }
        }
    }

    void HandlePauseNavigation()
    {
        if (eventSystem == null) return;

        if (eventSystem.currentSelectedGameObject == null && firstSelectedOnPause != null)
        {
            eventSystem.SetSelectedGameObject(firstSelectedOnPause.gameObject);
            Debug.Log("UIManager: Botón de pausa seleccionado automáticamente");
        }
    }

    void UpdateResourceDisplay()
    {
        if (GameManager.Instance != null)
        {
            if (manaResourceText != null)
                manaResourceText.text = $"Maná: {GameManager.Instance.manaResource}";
            if (corruptionResourceText != null)
                corruptionResourceText.text = $"Corrupción: {GameManager.Instance.corruptionResource}";
            if (factionText != null)
                factionText.text = $"Facción: {GameManager.Instance.currentFaction}";
            if (buildingCountText != null)
                buildingCountText.text = $"Edificios: {GameManager.Instance.currentBuildings}/{GameManager.Instance.buildingLimit}";
        }
    }

    void UpdateSliderTexts()
    {
        if (manaSpeedText != null && manaSpeedSlider != null)
            manaSpeedText.text = $"Velocidad: {manaSpeedSlider.value:F1}s";
        if (manaDensityText != null && manaDensitySlider != null)
            manaDensityText.text = $"Densidad: {manaDensitySlider.value:F2}";
        if (corruptionSpeedText != null && corruptionSpeedSlider != null)
            corruptionSpeedText.text = $"Velocidad: {corruptionSpeedSlider.value:F1}s";
        if (corruptionStrengthText != null && corruptionStrengthSlider != null)
            corruptionStrengthText.text = $"Fuerza: {corruptionStrengthSlider.value:F2}";
        if (eventsIntervalText != null && eventsIntervalSlider != null)
            eventsIntervalText.text = $"Intervalo: {eventsIntervalSlider.value:F1}s";
        if (faunaSpawnText != null && faunaSpawnSlider != null)
            faunaSpawnText.text = $"Spawn: {faunaSpawnSlider.value:F1}s";
        if (faunaMaxText != null && faunaMaxSlider != null)
            faunaMaxText.text = $"Máx: {faunaMaxSlider.value}";
        if (unitSpeedText != null && unitSpeedSlider != null)
            unitSpeedText.text = $"Velocidad: {unitSpeedSlider.value:F1}";
        if (unitVisionText != null && unitVisionSlider != null)
            unitVisionText.text = $"Visión: {unitVisionSlider.value:F1}";
        if (masterVolumeText != null && masterVolumeSlider != null)
            masterVolumeText.text = $"Volumen: {Mathf.RoundToInt(masterVolumeSlider.value * 100)}%";
    }

    // ========== UI EVENT HANDLERS ==========
    void OnManaSpeedChanged(float value)
    {
        ManaFlowSimulation manaFlow = FindObjectOfType<ManaFlowSimulation>();
        if (manaFlow != null) manaFlow.updateTime = value;
    }

    void OnManaDensityChanged(float value)
    {
        ManaFlowSimulation manaFlow = FindObjectOfType<ManaFlowSimulation>();
        if (manaFlow != null) manaFlow.initialManaDensity = value;
    }

    void OnManaAutoToggle(bool value)
    {
        ManaFlowSimulation manaFlow = FindObjectOfType<ManaFlowSimulation>();
        if (manaFlow != null) manaFlow.autoSimulate = value;
    }

    void OnCorruptionSpeedChanged(float value)
    {
        CorruptionSimulation corruption = FindObjectOfType<CorruptionSimulation>();
        if (corruption != null) corruption.updateTime = value;
    }

    void OnCorruptionStrengthChanged(float value)
    {
        CorruptionSimulation corruption = FindObjectOfType<CorruptionSimulation>();
        if (corruption != null) corruption.tasaBase = value;
    }

    void OnCorruptionAutoToggle(bool value)
    {
        CorruptionSimulation corruption = FindObjectOfType<CorruptionSimulation>();
        if (corruption != null) corruption.autoSimulate = value;
    }

    void OnEventsIntervalChanged(float value)
    {
        MagicEventsSimulation events = FindObjectOfType<MagicEventsSimulation>();
        if (events != null) events.eventCheckInterval = value;
    }

    void OnEventsEnabledToggle(bool value)
    {
        MagicEventsSimulation events = FindObjectOfType<MagicEventsSimulation>();
        if (events != null) events.eventsEnabled = value;
    }

    void OnFaunaSpawnChanged(float value)
    {
        MagicalFaunaSimulation fauna = FindObjectOfType<MagicalFaunaSimulation>();
        if (fauna != null) fauna.updateTime = value;
    }

    void OnFaunaMaxChanged(float value)
    {
        MagicalFaunaSimulation fauna = FindObjectOfType<MagicalFaunaSimulation>();
        if (fauna != null) fauna.maxFauna = (int)value;
    }

    void OnFaunaAutoToggle(bool value)
    {
        MagicalFaunaSimulation fauna = FindObjectOfType<MagicalFaunaSimulation>();
        if (fauna != null) fauna.autoSimulate = value;
    }

    void OnUnitSpeedChanged(float value)
    {
        UnitBehavior[] units = FindObjectsOfType<UnitBehavior>();
        foreach (UnitBehavior unit in units)
        {
            unit.moveSpeed = value;
        }
    }

    void OnUnitVisionChanged(float value)
    {
        UnitBehavior[] units = FindObjectsOfType<UnitBehavior>();
        foreach (UnitBehavior unit in units)
        {
            unit.visionRange = value;
        }
    }

    void OnMasterVolumeChanged(float value)
    {
        AudioListener.volume = value;
    }

    // ========== BUTTON HANDLERS ==========
    void OnResumeButton()
    {
        TogglePause();
    }

    void OnRestartButton()
    {
        RestartSimulations();
    }

    void OnQuitButton()
    {
        QuitGame();
    }

    void OnTogglePanelButton()
    {
        ToggleSimulationPanel();
    }

    // ========== PUBLIC METHODS ==========
    public void TogglePause()
    {
        if (GameManager.Instance != null)
        {
            Debug.Log("UIManager: Alternando pausa");
            GameManager.Instance.TogglePause();
        }
        else
        {
            Debug.LogError("GameManager.Instance es null - No se puede alternar pausa");
        }
    }

    public void ToggleSimulationPanel()
    {
        if (simulationPanel != null)
        {
            controlsPanelVisible = !simulationPanel.activeSelf;
            simulationPanel.SetActive(controlsPanelVisible);

            // Manejar navegación con mando
            if (controlsPanelVisible)
            {
                if (eventSystem != null && firstSelectedOnControls != null)
                {
                    eventSystem.SetSelectedGameObject(firstSelectedOnControls.gameObject);
                    Debug.Log("UIManager: Primer control seleccionado");
                }
            }
            else
            {
                if (eventSystem != null)
                {
                    eventSystem.SetSelectedGameObject(null);
                }
            }

            Debug.Log($"Panel de controles: {(controlsPanelVisible ? "Visible" : "Oculto")}");
        }
    }

    public void RestartSimulations()
    {
        Debug.Log("Reiniciando todas las simulaciones...");

        // Reiniciar Mana Flow
        ManaFlowSimulation manaFlow = FindObjectOfType<ManaFlowSimulation>();
        if (manaFlow != null)
        {
            manaFlow.RestartSimulation();
            Debug.Log("Mana Flow reiniciado");
        }

        // Reiniciar Corruption
        CorruptionSimulation corruption = FindObjectOfType<CorruptionSimulation>();
        if (corruption != null)
        {
            corruption.RestartSimulation();
            Debug.Log("Corruption reiniciado");
        }

        // Limpiar Fauna
        MagicalFaunaSimulation fauna = FindObjectOfType<MagicalFaunaSimulation>();
        if (fauna != null)
        {
            fauna.ClearFauna();
            Debug.Log("Fauna limpiada");
        }

        // Limpiar unidades
        UnitBehavior[] units = FindObjectsOfType<UnitBehavior>();
        foreach (UnitBehavior unit in units)
        {
            Destroy(unit.gameObject);
        }
        Debug.Log($"Unidades eliminadas: {units.Length}");

        // Limpiar edificios
        Sanctuary[] sanctuaries = FindObjectsOfType<Sanctuary>();
        Corruptor[] corruptors = FindObjectsOfType<Corruptor>();
        foreach (Sanctuary sanctuary in sanctuaries) Destroy(sanctuary.gameObject);
        foreach (Corruptor corruptor in corruptors) Destroy(corruptor.gameObject);
        Debug.Log($"Edificios eliminados: {sanctuaries.Length + corruptors.Length}");

        // Resetear recursos si es necesario
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentBuildings = 0;
            GameManager.Instance.UpdateUI();
        }
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void OnPauseStateChanged(bool isPaused)
    {
        Debug.Log($"UIManager: Estado de pausa cambiado a {isPaused}");

        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);

            // Si se activa la pausa, ocultar el panel de controles
            if (isPaused && simulationPanel != null)
            {
                simulationPanel.SetActive(false);
                controlsPanelVisible = false;
            }

            // Manejar selección de UI para mando
            if (isPaused)
            {
                if (eventSystem != null && firstSelectedOnPause != null)
                {
                    eventSystem.SetSelectedGameObject(firstSelectedOnPause.gameObject);
                    Debug.Log("Elemento de pausa seleccionado: " + firstSelectedOnPause.name);
                }
            }
            else
            {
                // Al despausar, quitar selección de UI
                if (eventSystem != null)
                {
                    eventSystem.SetSelectedGameObject(null);
                }
            }
        }

        if (pauseText != null)
        {
            pauseText.text = isPaused ? "JUEGO PAUSADO" : "";
        }
    }
}