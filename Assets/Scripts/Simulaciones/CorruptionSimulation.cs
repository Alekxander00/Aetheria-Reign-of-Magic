using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CorruptionSimulation : MonoBehaviour
{
    [Header("Corruption Simulation")]
    public float updateTime = 0.2f;
    public bool autoSimulate = true;

    [Header("Simulation Parameters")]
    public float tasaBase = 0.1f;
    public float factorMana = 0.3f;
    public int radioSantuario = 5;
    public float fuerzaSupresion = 0.2f;
    public float umbralExpansion = 0.15f;

    [Header("Initial Corruption")]
    [Range(0f, 1f)] public float initialCorruptionDensity = 0.05f;

    [Header("Colors")]
    public Color santuarioColor = Color.yellow;

    private GridManager gridManager;
    private List<Vector2Int> santuarios;
    private float timer;
    private bool isPaused = false;
    public bool isInitialized { get; private set; } = false;

    private Vector2Int[] direcciones = new Vector2Int[]
    {
        new Vector2Int(1, 0), new Vector2Int(-1, 0),
        new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(1, 1), new Vector2Int(-1, -1),
        new Vector2Int(1, -1), new Vector2Int(-1, 1)
    };

    void Start()
    {
        gridManager = GridManager.Instance;
        if (gridManager == null)
        {
            Debug.LogError("No se encontró GridManager");
            return;
        }

        StartCoroutine(InitializeCorruption());
    }

    IEnumerator InitializeCorruption()
    {
        // Esperar a que ManaFlow esté inicializado si es necesario
        yield return new WaitForSeconds(0.1f);
        InitializeSimulation();
    }

    public void InitializeSimulation()
    {
        InitializeGrid();

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPause += TogglePause;
            InputManager.Instance.OnRestart += RestartSimulation;
            InputManager.Instance.OnClear += ClearSimulation;
        }

        isInitialized = true;
        Debug.Log("CorruptionSimulation inicializado con grid compartido");
    }

    void InitializeGrid()
    {
        santuarios = new List<Vector2Int>();
        InitializeSantuarios();

        int corruptionCount = 0;
        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                if (Random.value < initialCorruptionDensity)
                {
                    gridManager.corruptionGrid[x, y] = 0.8f + Random.value * 0.2f;
                    corruptionCount++;
                }
                else
                {
                    gridManager.corruptionGrid[x, y] = 0f;
                }
            }
        }

        gridManager.UpdateVisualization();
        Debug.Log($"Corruption: {corruptionCount} celdas corruptas iniciales");
    }

    void InitializeSantuarios()
    {
        santuarios.Clear();
        santuarios.Add(new Vector2Int(10, 10));
        santuarios.Add(new Vector2Int(gridManager.width - 10, gridManager.height - 10));
        santuarios.Add(new Vector2Int(10, gridManager.height - 10));
        santuarios.Add(new Vector2Int(gridManager.width - 10, 10));
    }

    void Update()
    {
        if (!isInitialized || isPaused || !autoSimulate) return;

        timer += Time.deltaTime;
        if (timer >= updateTime)
        {
            SimulateStep();
            timer = 0f;
        }
    }

    public void SimulateStep()
    {
        CalculateNextGrid();
        gridManager.UpdateVisualization();
    }

    void CalculateNextGrid()
    {
        int width = gridManager.width;
        int height = gridManager.height;
        float[,] nextCorruptionGrid = new float[width, height];

        // Copiar estado actual
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                nextCorruptionGrid[x, y] = gridManager.corruptionGrid[x, y];
            }
        }

        // Calcular expansión
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gridManager.corruptionGrid[x, y] > 0.1f)
                {
                    ExpandFromCell(x, y, ref nextCorruptionGrid);
                }

                ApplySantuarioSuppression(x, y, ref nextCorruptionGrid);
            }
        }

        // Aplicar cambios al grid compartido
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                gridManager.corruptionGrid[x, y] = nextCorruptionGrid[x, y];
            }
        }
    }

    void ExpandFromCell(int x, int y, ref float[,] nextGrid)
    {
        foreach (var dir in direcciones)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;

            if (!gridManager.IsValidPosition(nx, ny)) continue;

            if (gridManager.corruptionGrid[nx, ny] < 0.1f)
            {
                // INTERACCIÓN: La corrupción se expande más rápido hacia áreas con maná
                float expansionRate = CalculateExpansionRate(x, y, nx, ny);

                if (expansionRate > umbralExpansion && Random.value < expansionRate)
                {
                    float baseCorruption = gridManager.corruptionGrid[x, y] * 0.7f;
                    float manaAttraction = GetManaDensityAt(nx, ny) * 0.3f;
                    nextGrid[nx, ny] = Mathf.Min(1f, baseCorruption + manaAttraction);
                }
            }
        }
    }

    float CalculateExpansionRate(int fromX, int fromY, int toX, int toY)
    {
        float rate = tasaBase;

        // INTERACCIÓN: Atracción por maná
        float manaTarget = GetManaDensityAt(toX, toY);
        rate += manaTarget * factorMana;

        return rate;
    }

    float GetManaDensityAt(int x, int y)
    {
        if (!gridManager.IsValidPosition(x, y)) return 0f;

        CellState state = gridManager.manaGrid[x, y];
        switch (state)
        {
            case CellState.TierraNormal: return 0f;
            case CellState.TierraMagica: return 0.5f;
            case CellState.CristalMagico: return 1f;
            case CellState.ArbolAncestral: return 0.8f;
            default: return 0f;
        }
    }

    void ApplySantuarioSuppression(int x, int y, ref float[,] nextGrid)
    {
        float suppression = GetSantuarioSuppression(x, y);
        nextGrid[x, y] = Mathf.Max(0f, nextGrid[x, y] - suppression);
    }

    float GetSantuarioSuppression(int x, int y)
    {
        float totalSuppression = 0f;

        foreach (var santuario in santuarios)
        {
            float distance = Vector2Int.Distance(new Vector2Int(x, y), santuario);
            if (distance <= radioSantuario)
            {
                float suppression = fuerzaSupresion * (1f - distance / radioSantuario);
                totalSuppression += suppression;
            }
        }

        return Mathf.Min(totalSuppression, 0.8f);
    }

    // Input handlers
    void TogglePause()
    {
        if (!isInitialized) return;
        isPaused = !isPaused;
        Debug.Log(isPaused ? "Corruption pausada" : "Corruption reanudada");
    }

    void RestartSimulation()
    {
        if (!isInitialized) return;
        Debug.Log("Reiniciando Corruption...");
        InitializeGrid();
        timer = 0f;
    }

    void ClearSimulation()
    {
        if (!isInitialized) return;
        Debug.Log("Limpiando Corruption...");
        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                gridManager.corruptionGrid[x, y] = 0f;
            }
        }
        gridManager.UpdateVisualization();
        timer = 0f;
    }

    public float GetCorruptionLevel(int x, int y)
    {
        if (!isInitialized || !gridManager.IsValidPosition(x, y))
            return 0f;
        return gridManager.corruptionGrid[x, y];
    }

    void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPause -= TogglePause;
            InputManager.Instance.OnRestart -= RestartSimulation;
            InputManager.Instance.OnClear -= ClearSimulation;
        }
    }
}