using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MagicEventsSimulation : MonoBehaviour
{
    [Header("Magic Events Simulation")]
    public float eventCheckInterval = 10f;
    public bool eventsEnabled = true;

    [Header("Event Probabilities")]
    [Range(0f, 1f)] public float manaSurgeProbability = 0.2f;
    [Range(0f, 1f)] public float corruptionBloomProbability = 0.2f;
    [Range(0f, 1f)] public float purificationWaveProbability = 0.15f;
    [Range(0f, 1f)] public float magicalEarthquakeProbability = 0.1f;

    [Header("Event Strengths")]
    public float manaSurgeStrength = 0.4f;
    public float corruptionBloomStrength = 0.4f;
    public float purificationWaveStrength = 0.5f;
    public float earthquakeDisruption = 0.3f;

    [Header("Event Duration")]
    public float eventDuration = 8f;

    [Header("2D Visual Effects")]
    public GameObject manaSurgeEffect;      // Prefab con SpriteRenderer y Animator
    public GameObject corruptionBloomEffect; // Prefab con SpriteRenderer y Animator  
    public GameObject purificationWaveEffect; // Prefab con SpriteRenderer y Animator
    public GameObject earthquakeEffect;     // Prefab con SpriteRenderer y Animator

    [Header("Screen Shake")]
    public float screenShakeIntensity = 0.5f;
    public float screenShakeDuration = 0.5f;

    [Header("Audio Effects")]
    public AudioClip manaSurgeSound;
    public AudioClip corruptionBloomSound;
    public AudioClip purificationWaveSound;
    public AudioClip earthquakeSound;
    public AudioSource audioSource;

    [Header("UI Effects - Notification System")]
    public TextMeshProUGUI eventNotificationText;
    public Image notificationBackground;
    public float notificationDuration = 3f;
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 1f;
    public float textPulseIntensity = 0.3f;
    public float textPulseSpeed = 2f;

    private float eventTimer = 0f;
    private float currentEventTimer = 0f;
    private MagicEventType currentEvent = MagicEventType.None;
    private bool eventActive = false;
    private Color originalBackgroundColor;
    private Vector3 originalCameraPosition;
    private Camera mainCamera;

    // Notification system variables
    private Coroutine notificationCoroutine;
    private Color originalTextColor;
    private Color originalBackgroundColorUI;
    private bool isShowingNotification = false;

    public enum MagicEventType
    {
        None,
        ManaSurge,
        CorruptionBloom,
        PurificationWave,
        MagicalEarthquake
    }

    void Start()
    {
        eventTimer = eventCheckInterval;
        mainCamera = Camera.main;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (mainCamera != null)
        {
            originalBackgroundColor = mainCamera.backgroundColor;
            originalCameraPosition = mainCamera.transform.position;
        }

        // Initialize notification system
        InitializeNotificationSystem();
    }

    void InitializeNotificationSystem()
    {
        if (eventNotificationText != null)
        {
            originalTextColor = eventNotificationText.color;
            // Start with notification hidden
            eventNotificationText.gameObject.SetActive(false);
        }

        if (notificationBackground != null)
        {
            originalBackgroundColorUI = notificationBackground.color;
            notificationBackground.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGamePaused) return;

        // Tu código original del Update aquí...
        if (!eventsEnabled) return;

        eventTimer -= Time.deltaTime;

        if (eventTimer <= 0f && !eventActive)
        {
            CheckForEvent();
            eventTimer = eventCheckInterval;
        }

        if (eventActive)
        {
            currentEventTimer -= Time.deltaTime;
            ExecuteCurrentEvent();
            UpdateEventEffects();

            if (currentEventTimer <= 0f)
            {
                EndCurrentEvent();
            }
        }

        // Update notification effects if active
        if (isShowingNotification)
        {
            UpdateNotificationEffects();
        }
    }

    void CheckForEvent()
    {
        float randomValue = Random.value;
        float cumulativeProbability = 0f;

        cumulativeProbability += manaSurgeProbability;
        if (randomValue <= cumulativeProbability)
        {
            StartEvent(MagicEventType.ManaSurge);
            return;
        }

        cumulativeProbability += corruptionBloomProbability;
        if (randomValue <= cumulativeProbability)
        {
            StartEvent(MagicEventType.CorruptionBloom);
            return;
        }

        cumulativeProbability += purificationWaveProbability;
        if (randomValue <= cumulativeProbability)
        {
            StartEvent(MagicEventType.PurificationWave);
            return;
        }

        cumulativeProbability += magicalEarthquakeProbability;
        if (randomValue <= cumulativeProbability)
        {
            StartEvent(MagicEventType.MagicalEarthquake);
            return;
        }
    }

    void StartEvent(MagicEventType eventType)
    {
        currentEvent = eventType;
        eventActive = true;
        currentEventTimer = eventDuration;

        string eventName = GetEventName(eventType);
        Debug.Log($"ˇEvento Mágico: {eventName}!");

        // Efectos de inicio
        PlayEventSound(eventType);
        StartVisualEffect(eventType);
        ShowEventNotification(eventName);

        if (eventType == MagicEventType.MagicalEarthquake)
        {
            StartCoroutine(ScreenShake());
        }
    }

    void ExecuteCurrentEvent()
    {
        GridManager grid = GridManager.Instance;
        if (grid == null) return;

        switch (currentEvent)
        {
            case MagicEventType.ManaSurge:
                ExecuteManaSurge();
                break;

            case MagicEventType.CorruptionBloom:
                ExecuteCorruptionBloom();
                break;

            case MagicEventType.PurificationWave:
                ExecutePurificationWave();
                break;

            case MagicEventType.MagicalEarthquake:
                ExecuteMagicalEarthquake();
                break;
        }

        grid.UpdateVisualization();
    }

    void UpdateEventEffects()
    {
        if (mainCamera == null) return;

        switch (currentEvent)
        {
            case MagicEventType.ManaSurge:
                // Tinte azul pulsante
                float manaPulse = Mathf.PingPong(Time.time * 2f, 1f);
                mainCamera.backgroundColor = Color.Lerp(originalBackgroundColor, new Color(0.2f, 0.4f, 0.8f, 0.3f), manaPulse * 0.3f);
                break;

            case MagicEventType.CorruptionBloom:
                // Tinte púrpura pulsante
                float corruptionPulse = Mathf.PingPong(Time.time * 3f, 1f);
                mainCamera.backgroundColor = Color.Lerp(originalBackgroundColor, new Color(0.4f, 0f, 0.4f, 0.3f), corruptionPulse * 0.3f);
                break;

            case MagicEventType.PurificationWave:
                // Brillos blancos suaves
                float purificationPulse = Mathf.Sin(Time.time * 4f) * 0.5f + 0.5f;
                mainCamera.backgroundColor = Color.Lerp(originalBackgroundColor, new Color(0.8f, 0.8f, 1f, 0.2f), purificationPulse * 0.2f);
                break;
        }
    }

    void ExecuteManaSurge()
    {
        GridManager grid = GridManager.Instance;

        for (int i = 0; i < 3; i++)
        {
            int centerX = Random.Range(5, grid.width - 5);
            int centerY = Random.Range(5, grid.height - 5);

            // Crear efecto visual en el centro
            CreateEventEffect(centerX, centerY, MagicEventType.ManaSurge);

            for (int x = centerX - 4; x <= centerX + 4; x++)
            {
                for (int y = centerY - 4; y <= centerY + 4; y++)
                {
                    if (grid.IsValidPosition(x, y))
                    {
                        float distance = Vector2.Distance(new Vector2(centerX, centerY), new Vector2(x, y));
                        if (distance <= 4f)
                        {
                            if (grid.manaGrid[x, y] == CellState.TierraNormal && Random.value < manaSurgeStrength * 0.3f)
                            {
                                grid.manaGrid[x, y] = CellState.TierraMagica;
                            }

                            if (grid.manaGrid[x, y] == CellState.TierraMagica && Random.value < manaSurgeStrength * 0.1f)
                            {
                                grid.manaGrid[x, y] = CellState.CristalMagico;
                            }

                            grid.corruptionGrid[x, y] = Mathf.Max(0, grid.corruptionGrid[x, y] - manaSurgeStrength * 0.2f);
                        }
                    }
                }
            }
        }
    }

    void ExecuteCorruptionBloom()
    {
        GridManager grid = GridManager.Instance;

        for (int i = 0; i < 2; i++)
        {
            int centerX = Random.Range(5, grid.width - 5);
            int centerY = Random.Range(5, grid.height - 5);

            // Crear efecto visual en el centro
            CreateEventEffect(centerX, centerY, MagicEventType.CorruptionBloom);

            for (int x = centerX - 5; x <= centerX + 5; x++)
            {
                for (int y = centerY - 5; y <= centerY + 5; y++)
                {
                    if (grid.IsValidPosition(x, y))
                    {
                        float distance = Vector2.Distance(new Vector2(centerX, centerY), new Vector2(x, y));
                        if (distance <= 5f)
                        {
                            float strength = corruptionBloomStrength * (1f - distance / 5f);
                            grid.corruptionGrid[x, y] = Mathf.Min(1f, grid.corruptionGrid[x, y] + strength * 0.5f);

                            if (grid.corruptionGrid[x, y] > 0.6f)
                            {
                                if (grid.manaGrid[x, y] == CellState.TierraMagica && Random.value < corruptionBloomStrength * 0.2f)
                                {
                                    grid.manaGrid[x, y] = CellState.TierraNormal;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void ExecutePurificationWave()
    {
        GridManager grid = GridManager.Instance;

        // Crear efecto de ola que barre el mapa
        for (int x = 0; x < grid.width; x += 3)
        {
            CreateEventEffect(x, grid.height / 2, MagicEventType.PurificationWave);
        }

        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                grid.corruptionGrid[x, y] = Mathf.Max(0, grid.corruptionGrid[x, y] - purificationWaveStrength * 0.3f);

                if (grid.manaGrid[x, y] == CellState.TierraMagica && Random.value < purificationWaveStrength * 0.15f)
                {
                    grid.manaGrid[x, y] = CellState.CristalMagico;
                }
            }
        }
    }

    void ExecuteMagicalEarthquake()
    {
        GridManager grid = GridManager.Instance;

        // Crear múltiples efectos de terremoto
        for (int i = 0; i < 5; i++)
        {
            int x = Random.Range(5, grid.width - 5);
            int y = Random.Range(5, grid.height - 5);
            CreateEventEffect(x, y, MagicEventType.MagicalEarthquake);
        }

        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                if (Random.value < earthquakeDisruption * 0.1f)
                {
                    if (Random.value < 0.3f)
                    {
                        if (grid.manaGrid[x, y] == CellState.TierraNormal)
                        {
                            grid.manaGrid[x, y] = CellState.TierraMagica;
                        }
                        else if (grid.manaGrid[x, y] == CellState.TierraMagica)
                        {
                            grid.manaGrid[x, y] = Random.value < 0.5f ? CellState.TierraNormal : CellState.CristalMagico;
                        }
                    }

                    if (Random.value < 0.4f)
                    {
                        grid.corruptionGrid[x, y] = Random.Range(0f, 1f);
                    }
                }
            }
        }
    }

    void CreateEventEffect(int x, int y, MagicEventType eventType)
    {
        GameObject effectPrefab = null;

        switch (eventType)
        {
            case MagicEventType.ManaSurge:
                effectPrefab = manaSurgeEffect;
                break;
            case MagicEventType.CorruptionBloom:
                effectPrefab = corruptionBloomEffect;
                break;
            case MagicEventType.PurificationWave:
                effectPrefab = purificationWaveEffect;
                break;
            case MagicEventType.MagicalEarthquake:
                effectPrefab = earthquakeEffect;
                break;
        }

        if (effectPrefab != null)
        {
            Vector3 position = new Vector3(x, y, -1f); // Z = -1 para que aparezca detrás del grid
            GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);

            // Destruir automáticamente después de un tiempo
            Destroy(effect, 3f);
        }
    }

    void EndCurrentEvent()
    {
        // Restaurar efectos visuales
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = originalBackgroundColor;
            mainCamera.transform.position = originalCameraPosition;
        }

        eventActive = false;
        currentEvent = MagicEventType.None;
        Debug.Log("Evento mágico ha terminado.");

        // Ocultar notificación si está activa
        HideNotification();
    }

    void PlayEventSound(MagicEventType eventType)
    {
        if (audioSource == null) return;

        AudioClip clip = null;
        switch (eventType)
        {
            case MagicEventType.ManaSurge:
                clip = manaSurgeSound;
                break;
            case MagicEventType.CorruptionBloom:
                clip = corruptionBloomSound;
                break;
            case MagicEventType.PurificationWave:
                clip = purificationWaveSound;
                break;
            case MagicEventType.MagicalEarthquake:
                clip = earthquakeSound;
                break;
        }

        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void StartVisualEffect(MagicEventType eventType)
    {
        // Efectos específicos por evento
        switch (eventType)
        {
            case MagicEventType.ManaSurge:
                // Ya se maneja en ExecuteManaSurge con CreateEventEffect
                break;
            case MagicEventType.CorruptionBloom:
                // Ya se maneja en ExecuteCorruptionBloom con CreateEventEffect
                break;
            case MagicEventType.PurificationWave:
                // Ya se maneja en ExecutePurificationWave con CreateEventEffect
                break;
            case MagicEventType.MagicalEarthquake:
                // El screen shake ya se maneja en StartEvent
                break;
        }
    }

    // ========== SISTEMA DE NOTIFICACIONES MEJORADO ==========

    void ShowEventNotification(string eventName)
    {
        if (eventNotificationText == null) return;

        // Cancelar notificación anterior si existe
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
        }

        // Configurar texto
        eventNotificationText.text = $"{eventName}!";

        // Configurar color según el evento
        Color textColor = GetNotificationColor(currentEvent);
        eventNotificationText.color = textColor;

        // Iniciar corrutina de notificación
        notificationCoroutine = StartCoroutine(NotificationSequence());
    }

    IEnumerator NotificationSequence()
    {
        isShowingNotification = true;

        // FASE 1: Fade In
        yield return StartCoroutine(FadeNotification(0f, 1f, fadeInDuration));

        // FASE 2: Mantener visible
        float visibleTimer = 0f;
        while (visibleTimer < notificationDuration)
        {
            visibleTimer += Time.deltaTime;
            yield return null;
        }

        // FASE 3: Fade Out
        yield return StartCoroutine(FadeNotification(1f, 0f, fadeOutDuration));

        // Ocultar completamente
        eventNotificationText.gameObject.SetActive(false);
        if (notificationBackground != null)
            notificationBackground.gameObject.SetActive(false);

        isShowingNotification = false;
        notificationCoroutine = null;
    }

    IEnumerator FadeNotification(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;

        // Activar objetos si estamos haciendo fade in
        if (toAlpha > 0f)
        {
            eventNotificationText.gameObject.SetActive(true);
            if (notificationBackground != null)
                notificationBackground.gameObject.SetActive(true);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float currentAlpha = Mathf.Lerp(fromAlpha, toAlpha, t);

            // Aplicar alpha al texto
            Color textColor = eventNotificationText.color;
            textColor.a = currentAlpha;
            eventNotificationText.color = textColor;

            // Aplicar alpha al fondo si existe
            if (notificationBackground != null)
            {
                Color bgColor = notificationBackground.color;
                bgColor.a = currentAlpha * 0.7f; // Fondo más transparente
                notificationBackground.color = bgColor;
            }

            yield return null;
        }

        // Desactivar objetos si estamos haciendo fade out
        if (toAlpha == 0f)
        {
            eventNotificationText.gameObject.SetActive(false);
            if (notificationBackground != null)
                notificationBackground.gameObject.SetActive(false);
        }
    }

    void UpdateNotificationEffects()
    {
        if (!isShowingNotification || eventNotificationText == null) return;

        // Efecto de pulso en el texto
        float pulse = (Mathf.Sin(Time.time * textPulseSpeed) + 1f) * 0.5f;
        Color pulseColor = eventNotificationText.color;
        pulseColor.a = 0.8f + pulse * textPulseIntensity;
        eventNotificationText.color = pulseColor;
    }

    void HideNotification()
    {
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
            notificationCoroutine = null;
        }

        if (eventNotificationText != null)
        {
            eventNotificationText.gameObject.SetActive(false);
        }

        if (notificationBackground != null)
        {
            notificationBackground.gameObject.SetActive(false);
        }

        isShowingNotification = false;
    }

    Color GetNotificationColor(MagicEventType eventType)
    {
        switch (eventType)
        {
            case MagicEventType.ManaSurge:
                return new Color(0.2f, 0.6f, 1f, 1f); // Azul mágico
            case MagicEventType.CorruptionBloom:
                return new Color(0.8f, 0.2f, 0.8f, 1f); // Púrpura corrupto
            case MagicEventType.PurificationWave:
                return new Color(1f, 1f, 1f, 1f); // Blanco puro
            case MagicEventType.MagicalEarthquake:
                return new Color(1f, 0.8f, 0.2f, 1f); // Amarillo terremoto
            default:
                return Color.white;
        }
    }

    IEnumerator ScreenShake()
    {
        float elapsed = 0f;
        Vector3 originalPos = mainCamera.transform.position;

        while (elapsed < screenShakeDuration)
        {
            float x = Random.Range(-1f, 1f) * screenShakeIntensity;
            float y = Random.Range(-1f, 1f) * screenShakeIntensity;

            mainCamera.transform.position = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = originalPos;
    }

    string GetEventName(MagicEventType eventType)
    {
        switch (eventType)
        {
            case MagicEventType.ManaSurge: return "Oleada de Maná";
            case MagicEventType.CorruptionBloom: return "Floración Corrupta";
            case MagicEventType.PurificationWave: return "Ola de Purificación";
            case MagicEventType.MagicalEarthquake: return "Terremoto Mágico";
            default: return "Ninguno";
        }
    }

    public MagicEventType GetCurrentEvent()
    {
        return currentEvent;
    }

    public bool IsEventActive()
    {
        return eventActive;
    }

    public float GetEventTimeRemaining()
    {
        return currentEventTimer;
    }
}