using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CorruptionSimulation : MonoBehaviour
{
    [Header("Corruption Simulation")]
    public float updateTime = 0.2f;
    public bool autoSimulate = true;

    [Header("Simulation Parameters")]
    public float tasaBase = 0.25f; // Aumentado
    public float factorMana = 0.4f; // Aumentado
    public int radioSantuario = 5;
    public float fuerzaSupresion = 0.15f; // Reducido
    public float umbralExpansion = 0.08f; // Reducido

    [Header("Initial Corruption")]
    [Range(0f, 1f)] public float initialCorruptionDensity = 0.05f;

    [Header("Corruption Strength")]
    public float corruptionDamageToMana = 0.35f; // Aumentado significativamente
    public float corruptionSpreadFromPits = 0.5f; // Aumentado

    private GridManager gridManager;
    private List<Vector2Int> santuarios;
    private List<Vector2Int> pozosCorruptores;
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
        yield return new WaitForSeconds(0.1f);
        InitializeSimulation();
    }

    public void InitializeSimulation()
    {
        InitializeGrid();
        isInitialized = true;
        Debug.Log("CorruptionSimulation inicializado");
    }

    void InitializeGrid()
    {
        santuarios = new List<Vector2Int>();
        pozosCorruptores = new List<Vector2Int>();
        FindSantuarios();
        FindPozosCorruptores();

        int corruptionCount = 0;
        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                if (Random.value < initialCorruptionDensity)
                {
                    gridManager.corruptionGrid[x, y] = 0.7f + Random.value * 0.2f;
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

    void FindSantuarios()
    {
        santuarios.Clear();
        Sanctuary[] santuariosEnEscena = FindObjectsOfType<Sanctuary>();
        foreach (Sanctuary santuario in santuariosEnEscena)
        {
            Vector3 pos = santuario.transform.position;
            santuarios.Add(new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y)));
        }
    }

    void FindPozosCorruptores()
    {
        pozosCorruptores.Clear();
        Corruptor[] pozosEnEscena = FindObjectsOfType<Corruptor>();
        foreach (Corruptor pozo in pozosEnEscena)
        {
            Vector3 pos = pozo.transform.position;
            pozosCorruptores.Add(new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y)));
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGamePaused) return;

        // Tu código original del Update aquí...
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

        // Actualizar listas de edificios
        FindSantuarios();
        FindPozosCorruptores();

        // FASE 1: Los pozos corruptores GENERAN corrupción activamente
        foreach (var pozo in pozosCorruptores)
        {
            GenerateCorruptionFromPit(pozo.x, pozo.y, ref nextCorruptionGrid);
        }

        // FASE 2: Expansión natural de la corrupción
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gridManager.corruptionGrid[x, y] > 0.2f)
                {
                    ExpandFromCell(x, y, ref nextCorruptionGrid);
                }
                ApplySantuarioSuppression(x, y, ref nextCorruptionGrid);

                // FASE 3: La corrupción DAÑA el maná (más frecuente)
                if (Random.value < 0.6f) // 60% de las veces
                {
                    DamageManaAtCell(x, y);
                }
            }
        }

        // Aplicar cambios
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                gridManager.corruptionGrid[x, y] = nextCorruptionGrid[x, y];
            }
        }
    }

    void GenerateCorruptionFromPit(int x, int y, ref float[,] nextGrid)
    {
        // Los pozos corruptores generan corrupción en radio 5
        for (int dx = -5; dx <= 5; dx++)
        {
            for (int dy = -5; dy <= 5; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;

                if (gridManager.IsValidPosition(nx, ny))
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(nx, ny));
                    if (distance <= 5f)
                    {
                        float strength = corruptionSpreadFromPits * (1f - distance / 5f);
                        nextGrid[nx, ny] = Mathf.Min(1f, nextGrid[nx, ny] + strength * 0.7f);
                    }
                }
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

            if (gridManager.corruptionGrid[nx, ny] < 0.8f)
            {
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

    void DamageManaAtCell(int x, int y)
    {
        // La corrupción media-alta daña el maná
        if (gridManager.corruptionGrid[x, y] > 0.4f)
        {
            CellState currentState = gridManager.manaGrid[x, y];

            switch (currentState)
            {
                case CellState.TierraMagica:
                    if (Random.value < corruptionDamageToMana * 0.4f)
                    {
                        gridManager.manaGrid[x, y] = CellState.TierraNormal;
                    }
                    break;

                case CellState.CristalMagico:
                    if (Random.value < corruptionDamageToMana * 0.2f)
                    {
                        gridManager.manaGrid[x, y] = CellState.TierraMagica;
                    }
                    break;

                case CellState.ArbolAncestral:
                    if (Random.value < corruptionDamageToMana * 0.1f)
                    {
                        gridManager.manaGrid[x, y] = CellState.TierraMagica;
                    }
                    break;
            }
        }
    }

    float CalculateExpansionRate(int fromX, int fromY, int toX, int toY)
    {
        float rate = tasaBase;

        // Atracción por maná - MÁS AGRESIVA
        float manaTarget = GetManaDensityAt(toX, toY);
        rate += manaTarget * factorMana;

        // Los santuarios suprimen menos
        float suppression = GetSantuarioSuppression(toX, toY);
        rate -= suppression;

        return Mathf.Max(0f, rate);
    }

    float GetManaDensityAt(int x, int y)
    {
        if (!gridManager.IsValidPosition(x, y)) return 0f;

        CellState state = gridManager.manaGrid[x, y];
        switch (state)
        {
            case CellState.TierraNormal: return 0f;
            case CellState.TierraMagica: return 0.6f;
            case CellState.CristalMagico: return 0.9f;
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

        return Mathf.Min(totalSuppression, 0.3f);
    }

    // Resto del código igual...
    public void TogglePause()
    {
        isPaused = !isPaused;
        Debug.Log(isPaused ? "Corruption pausada" : "Corruption reanudada");
    }

    public void RestartSimulation()
    {
        Debug.Log("Reiniciando Corruption...");
        InitializeGrid();
        timer = 0f;
    }

    public void ClearSimulation()
    {
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
}