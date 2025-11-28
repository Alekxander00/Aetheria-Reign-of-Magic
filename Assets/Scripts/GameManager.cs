using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum PlayerFaction { Mana, Corruption, Neutral }
    public PlayerFaction currentFaction = PlayerFaction.Neutral;

    [Header("Resources")]
    public int manaResource = 100;
    public int corruptionResource = 100;

    [Header("Buildings")]
    public int buildingLimit = 10;
    public int currentBuildings = 0;

    [Header("Resource Generation")]
    public float resourceGenerationInterval = 5f;
    public int baseManaGeneration = 10;
    public int baseCorruptionGeneration = 10;

    [Header("UI References")]
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI corruptionText;
    public TextMeshProUGUI factionText;

    [Header("Game State - PAUSA GLOBAL")]
    public bool isGamePaused = false;
    public System.Action<bool> OnPauseStateChanged;

    [Header("Audio")]
    public AudioSource ambientAudioSource;
    public AudioClip ambientSound;
    public float ambientVolume = 0.3f;

    private void Awake()
    {
        Debug.Log("GameManager: Awake iniciado");

        if (Instance == null)
        {
            Instance = this;
            // Quitamos DontDestroyOnLoad para evitar problemas
            Debug.Log("GameManager: Instancia creada");
        }
        else
        {
            Debug.Log("GameManager: Instancia duplicada destruida");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        Debug.Log("GameManager: Start iniciado");

        // Asegurarnos de que el juego empiece en tiempo normal
        Time.timeScale = 1f;
        isGamePaused = false;

        StartCoroutine(ResourceGenerationRoutine());
        UpdateUI();

        // Configurar audio ambiental
        if (ambientAudioSource != null && ambientSound != null)
        {
            ambientAudioSource.clip = ambientSound;
            ambientAudioSource.volume = ambientVolume;
            ambientAudioSource.loop = true;
            ambientAudioSource.Play();
            Debug.Log("GameManager: Audio ambiental configurado");
        }

        Debug.Log("GameManager: Start completado - Juego listo");
    }

    IEnumerator ResourceGenerationRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(resourceGenerationInterval);
            if (!isGamePaused)
            {
                GenerateResources();
                UpdateUI();
            }
        }
    }

    void GenerateResources()
    {
        if (currentFaction == PlayerFaction.Mana)
        {
            int manaFromBuildings = CountManaBuildings() * 8;
            int manaFromTerritory = CalculateManaTerritory();
            manaResource += baseManaGeneration + manaFromBuildings + manaFromTerritory;
        }
        else if (currentFaction == PlayerFaction.Corruption)
        {
            int corruptionFromBuildings = CountCorruptionBuildings() * 8;
            int corruptionFromTerritory = CalculateCorruptionTerritory();
            corruptionResource += baseCorruptionGeneration + corruptionFromBuildings + corruptionFromTerritory;
        }
        UpdateUI();
    }

    int CountManaBuildings()
    {
        Sanctuary[] sanctuaries = FindObjectsOfType<Sanctuary>();
        return sanctuaries.Length;
    }

    int CountCorruptionBuildings()
    {
        Corruptor[] corruptors = FindObjectsOfType<Corruptor>();
        return corruptors.Length;
    }

    int CalculateManaTerritory()
    {
        GridManager gridManager = GridManager.Instance;
        if (gridManager == null) return 0;

        int manaCells = 0;
        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                if (gridManager.manaGrid[x, y] != CellState.TierraNormal &&
                    gridManager.corruptionGrid[x, y] < 0.3f)
                {
                    manaCells++;
                }
            }
        }
        return manaCells / 10;
    }

    int CalculateCorruptionTerritory()
    {
        GridManager gridManager = GridManager.Instance;
        if (gridManager == null) return 0;

        int corruptionCells = 0;
        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                if (gridManager.corruptionGrid[x, y] > 0.3f)
                {
                    corruptionCells++;
                }
            }
        }
        return corruptionCells / 10;
    }

    public void ChooseFaction(PlayerFaction faction)
    {
        currentFaction = faction;
        Debug.Log($"Jugador eligió: {faction}");

        if (faction == PlayerFaction.Mana)
        {
            manaResource = 200;
            corruptionResource = 0;
        }
        else if (faction == PlayerFaction.Corruption)
        {
            manaResource = 0;
            corruptionResource = 200;
        }

        UpdateUI();
        SceneManager.LoadScene("GameScene");
    }

    public bool CanBuild(int cost, bool isUnit = false)
    {
        if (currentFaction == PlayerFaction.Mana)
            return manaResource >= cost && (isUnit || currentBuildings < buildingLimit);
        else if (currentFaction == PlayerFaction.Corruption)
            return corruptionResource >= cost && (isUnit || currentBuildings < buildingLimit);
        return false;
    }

    public void SpendResources(int cost)
    {
        if (currentFaction == PlayerFaction.Mana)
            manaResource -= cost;
        else if (currentFaction == PlayerFaction.Corruption)
            corruptionResource -= cost;

        if (cost > 50) currentBuildings++;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (manaText != null) manaText.text = $"Maná: {manaResource}";
        if (corruptionText != null) corruptionText.text = $"Corrupción: {corruptionResource}";
        if (factionText != null) factionText.text = $"Facción: {currentFaction}";
    }

    // ========== SISTEMA DE PAUSA GLOBAL ==========
    public void TogglePause()
    {
        Debug.Log($"GameManager: TogglePause llamado - Estado actual: {isGamePaused}");

        isGamePaused = !isGamePaused;
        Time.timeScale = isGamePaused ? 0f : 1f;
        OnPauseStateChanged?.Invoke(isGamePaused);
        Debug.Log($"GameManager: Pausa {(isGamePaused ? "ACTIVADA" : "DESACTIVADA")}");
    }

    public void SetPause(bool paused)
    {
        isGamePaused = paused;
        Time.timeScale = isGamePaused ? 0f : 1f;
        OnPauseStateChanged?.Invoke(isGamePaused);
    }
}