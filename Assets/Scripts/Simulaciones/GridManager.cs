using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    public int width = 50;
    public int height = 30;
    public GameObject cellPrefab;

    [Header("Visualization")]
    public bool showGrid = true;

    // Grid compartido - ahora público para que ambas simulaciones accedan
    public GameObject[,] cellObjects { get; private set; }
    public SpriteRenderer[,] cellRenderers { get; private set; }

    // Datos compartidos entre simulaciones
    public CellState[,] manaGrid { get; set; }
    public float[,] corruptionGrid { get; set; }

    private bool isGridInitialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeSharedGrid();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeSharedGrid()
    {
        cellObjects = new GameObject[width, height];
        cellRenderers = new SpriteRenderer[width, height];
        manaGrid = new CellState[width, height];
        corruptionGrid = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cell = Instantiate(cellPrefab, new Vector3(x, y, 0), Quaternion.identity);
                cell.transform.parent = transform;
                cellObjects[x, y] = cell;
                cellRenderers[x, y] = cell.GetComponent<SpriteRenderer>();

                // Inicializar estados
                manaGrid[x, y] = CellState.TierraNormal;
                corruptionGrid[x, y] = 0f;
            }
        }

        isGridInitialized = true;
        Debug.Log("Grid compartido inicializado");
    }

    public void UpdateVisualization()
    {
        if (!isGridInitialized || cellRenderers == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color color = CalculateCombinedColor(x, y);
                cellRenderers[x, y].color = color;
            }
        }
    }

    Color CalculateCombinedColor(int x, int y)
    {
        // Color base del maná
        Color manaColor = GetManaColor(manaGrid[x, y]);

        // Overlay de corrupción
        float corruption = corruptionGrid[x, y];
        Color corruptionColor = Color.Lerp(Color.clear, new Color(0.3f, 0f, 0f, 0.7f), corruption);

        // Combinar
        return Color.Lerp(manaColor, corruptionColor, corruption * 0.8f);
    }

    Color GetManaColor(CellState state)
    {
        switch (state)
        {
            case CellState.TierraNormal: return Color.white;
            case CellState.TierraMagica: return Color.blue;
            case CellState.CristalMagico: return Color.cyan;
            case CellState.ArbolAncestral: return Color.green;
            default: return Color.white;
        }
    }

    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public int CountNeighbors(int x, int y, int radius = 1)
    {
        int count = 0;
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int nx = x + dx;
                int ny = y + dy;

                if (IsValidPosition(nx, ny))
                {
                    count++;
                }
            }
        }
        return count;
    }
}