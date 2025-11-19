using UnityEngine;

public class ManaFlowSimulation : MonoBehaviour
{
    [Header("Mana Flow Simulation")]
    public float updateTime = 0.1f;
    public bool autoSimulate = true;

    [Header("Simulation Parameters")]
    [Range(0, 8)] public int radioInfluencia = 1;
    [Range(0, 8)] public int umbralAislamiento = 2;
    [Range(0, 8)] public int umbralEstabilidad = 3;
    [Range(0, 8)] public int umbralCristalizacion = 3;
    [Range(0f, 1f)] public float probNacimiento = 0.3f;

    [Header("Initial Distribution")]
    [Range(0f, 1f)] public float initialManaDensity = 0.1f;
    [Range(0f, 1f)] public float initialTreeDensity = 0.01f;

    [Header("Interacción con Corrupción")]
    public bool corruptionAffectsMana = true;
    public float corruptionManaReduction = 0.3f;

    private GridManager gridManager;
    private int[,] ageGrid; // Mantenemos esto local
    private float timer;
    private bool isPaused = false;
    public bool IsInitialized { get; private set; } = false;

    void Start()
    {
        gridManager = GridManager.Instance;
        if (gridManager == null)
        {
            Debug.LogError("No se encontró GridManager");
            return;
        }

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

        IsInitialized = true;
        Debug.Log("ManaFlowSimulation inicializado con grid compartido");
    }

    public void InitializeGrid()
    {
        int width = gridManager.width;
        int height = gridManager.height;
        ageGrid = new int[width, height];

        int tierraMagicaCount = 0;
        int arbolCount = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float rand = Random.value;
                if (rand < initialTreeDensity)
                {
                    gridManager.manaGrid[x, y] = CellState.ArbolAncestral;
                    arbolCount++;
                }
                else if (rand < initialManaDensity + initialTreeDensity)
                {
                    gridManager.manaGrid[x, y] = CellState.TierraMagica;
                    tierraMagicaCount++;
                }
                else
                {
                    gridManager.manaGrid[x, y] = CellState.TierraNormal;
                }
                ageGrid[x, y] = 0;
            }
        }

        gridManager.UpdateVisualization();
        Debug.Log($"ManaFlow: {tierraMagicaCount} TierraMagica, {arbolCount} ArbolAncestral");
    }

    void Update()
    {
        if (!IsInitialized || isPaused || !autoSimulate) return;

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
        CellState[,] nextGrid = new CellState[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int vecinosMagicos = CountMagicalNeighbors(x, y);
                CellState currentState = gridManager.manaGrid[x, y];
                CellState nextState = currentState;

                // INTERACCIÓN CON CORRUPCIÓN
                if (corruptionAffectsMana && IsCellCorrupted(x, y))
                {
                    // La corrupción degrada el maná
                    if (currentState == CellState.TierraMagica && Random.value < corruptionManaReduction)
                    {
                        nextState = CellState.TierraNormal;
                    }
                    else if (currentState == CellState.CristalMagico && Random.value < corruptionManaReduction * 0.5f)
                    {
                        nextState = CellState.TierraMagica;
                    }
                }
                else
                {
                    // REGLAS NORMALES DEL AUTÓMATA
                    switch (currentState)
                    {
                        case CellState.TierraNormal:
                            if (vecinosMagicos == 3 && Random.value < probNacimiento)
                            {
                                nextState = CellState.TierraMagica;
                            }
                            break;

                        case CellState.TierraMagica:
                            if (vecinosMagicos < umbralAislamiento)
                            {
                                nextState = CellState.TierraNormal;
                            }
                            else if (vecinosMagicos > umbralCristalizacion)
                            {
                                nextState = CellState.CristalMagico;
                            }
                            break;
                    }
                }

                nextGrid[x, y] = nextState;

                // Actualizar edad
                if (nextState == currentState)
                {
                    ageGrid[x, y]++;
                }
                else
                {
                    ageGrid[x, y] = 0;
                }
            }
        }

        // Aplicar cambios al grid compartido
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                gridManager.manaGrid[x, y] = nextGrid[x, y];
            }
        }
    }

    int CountMagicalNeighbors(int x, int y)
    {
        int count = 0;

        for (int dx = -radioInfluencia; dx <= radioInfluencia; dx++)
        {
            for (int dy = -radioInfluencia; dy <= radioInfluencia; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int nx = x + dx;
                int ny = y + dy;

                if (gridManager.IsValidPosition(nx, ny) &&
                    gridManager.manaGrid[nx, ny] != CellState.TierraNormal)
                {
                    count++;
                }
            }
        }

        return count;
    }

    bool IsCellCorrupted(int x, int y)
    {
        return gridManager.corruptionGrid[x, y] > 0.3f;
    }

    // Input handlers
    void TogglePause()
    {
        isPaused = !isPaused;
        Debug.Log(isPaused ? "Mana Flow pausado" : "Mana Flow reanudado");
    }

    void RestartSimulation()
    {
        Debug.Log("Reiniciando Mana Flow...");
        InitializeGrid();
        timer = 0f;
    }

    void ClearSimulation()
    {
        Debug.Log("Limpiando Mana Flow...");
        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                gridManager.manaGrid[x, y] = CellState.TierraNormal;
            }
        }
        gridManager.UpdateVisualization();
        timer = 0f;
    }

    public CellState GetCellState(int x, int y)
    {
        if (!IsInitialized || !gridManager.IsValidPosition(x, y))
            return CellState.TierraNormal;
        return gridManager.manaGrid[x, y];
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