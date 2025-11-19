using UnityEngine;
using UnityEngine.UI;

public class BuildingSystem : MonoBehaviour
{
    [Header("Building Prefabs")]
    public GameObject sanctuaryPrefab;      // Para Maná
    public GameObject corruptorPrefab;      // Para Corrupción

    [Header("Building Costs")]
    public int sanctuaryCost = 100;
    public int corruptorCost = 100;

    [Header("UI Buttons")]
    public Button buildSanctuaryButton;
    public Button buildCorruptorButton;

    private bool isBuildingMode = false;
    private GameObject currentBuildingPrefab;
    private int currentBuildingCost;
    private GameObject buildingGhost;

    void Start()
    {
        UpdateBuildingButtons();
    }

    void Update()
    {
        if (!isBuildingMode) return;

        // Mover fantasma del edificio
        if (buildingGhost == null && currentBuildingPrefab != null)
        {
            buildingGhost = Instantiate(currentBuildingPrefab);
            // Hacerlo semi-transparente
            SpriteRenderer ghostRenderer = buildingGhost.GetComponent<SpriteRenderer>();
            if (ghostRenderer != null)
                ghostRenderer.color = new Color(1, 1, 1, 0.5f);
        }

        if (buildingGhost != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            buildingGhost.transform.position = mousePos;
        }

        // Cancelar construcción con ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelBuilding();
        }

        // Click izquierdo para colocar edificio
        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceBuilding();
        }
    }

    public void UpdateBuildingButtons()
    {
        if (GameManager.Instance == null) return;

        // Mostrar/ocultar botones según la facción
        if (GameManager.Instance.currentFaction == GameManager.PlayerFaction.Mana)
        {
            buildSanctuaryButton.gameObject.SetActive(true);
            buildCorruptorButton.gameObject.SetActive(false);
        }
        else if (GameManager.Instance.currentFaction == GameManager.PlayerFaction.Corruption)
        {
            buildSanctuaryButton.gameObject.SetActive(false);
            buildCorruptorButton.gameObject.SetActive(true);
        }
    }

    public void StartBuildingSanctuary()
    {
        // Verificar facción
        if (GameManager.Instance.currentFaction != GameManager.PlayerFaction.Mana)
        {
            Debug.Log("Solo la Alianza de la Magia puede construir Santuarios");
            return;
        }

        if (GameManager.Instance.CanBuild(sanctuaryCost))
        {
            isBuildingMode = true;
            currentBuildingPrefab = sanctuaryPrefab;
            currentBuildingCost = sanctuaryCost;
            Debug.Log("Modo construcción: Santuario. Click en terreno válido para construir. ESC para cancelar.");
        }
        else
        {
            Debug.Log("No hay recursos suficientes para Santuario");
        }
    }

    public void StartBuildingCorruptor()
    {
        // Verificar facción
        if (GameManager.Instance.currentFaction != GameManager.PlayerFaction.Corruption)
        {
            Debug.Log("Solo la Legión de la Corrupción puede construir Pozos Corruptores");
            return;
        }

        if (GameManager.Instance.CanBuild(corruptorCost))
        {
            isBuildingMode = true;
            currentBuildingPrefab = corruptorPrefab;
            currentBuildingCost = corruptorCost;
            Debug.Log("Modo construcción: Pozo Corruptor. Click en terreno válido para construir. ESC para cancelar.");
        }
        else
        {
            Debug.Log("No hay recursos suficientes para Pozo Corruptor");
        }
    }

    void TryPlaceBuilding()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;

        // Verificar posición válida (dentro del grid)
        GridManager gridManager = GridManager.Instance;
        int x = Mathf.RoundToInt(worldPos.x);
        int y = Mathf.RoundToInt(worldPos.y);

        if (!gridManager.IsValidPosition(x, y))
        {
            Debug.Log("Posición inválida para construir");
            return;
        }

        // Verificar terreno según facción
        bool canBuildHere = false;
        string terrainError = "";

        if (GameManager.Instance.currentFaction == GameManager.PlayerFaction.Mana)
        {
            // Maná solo puede construir en tierra mágica (no normal)
            canBuildHere = gridManager.manaGrid[x, y] != CellState.TierraNormal;
            terrainError = "Solo puedes construir en Tierra Mágica, Cristales o cerca de Árboles Ancestrales";
        }
        else if (GameManager.Instance.currentFaction == GameManager.PlayerFaction.Corruption)
        {
            // Corrupción solo puede construir en tierra corrupta
            canBuildHere = gridManager.corruptionGrid[x, y] > 0.3f;
            terrainError = "Solo puedes construir en áreas con alta corrupción";
        }

        if (canBuildHere)
        {
            // Construir
            Instantiate(currentBuildingPrefab, worldPos, Quaternion.identity);
            GameManager.Instance.SpendResources(currentBuildingCost);
            Debug.Log("¡Edificio construido!");

            // Aplicar efectos en la simulación
            ApplyBuildingEffects(x, y);

            CancelBuilding();

            // Actualizar botones después de construir (por si se acaban los recursos)
            UpdateBuildingButtons();
        }
        else
        {
            Debug.Log(terrainError);
        }
    }

    void ApplyBuildingEffects(int x, int y)
    {
        GridManager gridManager = GridManager.Instance;

        if (currentBuildingPrefab == sanctuaryPrefab)
        {
            // Santuario suprime corrupción en radio 5
            for (int dx = -5; dx <= 5; dx++)
            {
                for (int dy = -5; dy <= 5; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    if (gridManager.IsValidPosition(nx, ny))
                    {
                        float distance = Vector2.Distance(new Vector2(x, y), new Vector2(nx, ny));
                        if (distance <= 5)
                        {
                            gridManager.corruptionGrid[nx, ny] = Mathf.Max(0, gridManager.corruptionGrid[nx, ny] - 0.3f);
                        }
                    }
                }
            }
            gridManager.UpdateVisualization();

            // El componente Sanctuary se añade automáticamente al instanciar el prefab
        }
        else if (currentBuildingPrefab == corruptorPrefab)
        {
            // Pozo corruptor expande corrupción en radio 3
            for (int dx = -3; dx <= 3; dx++)
            {
                for (int dy = -3; dy <= 3; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    if (gridManager.IsValidPosition(nx, ny))
                    {
                        gridManager.corruptionGrid[nx, ny] = Mathf.Min(1f, gridManager.corruptionGrid[nx, ny] + 0.2f);
                    }
                }
            }
            gridManager.UpdateVisualization();

            // El componente Corruptor se añade automáticamente al instanciar el prefab
        }
    }

    void CancelBuilding()
    {
        if (buildingGhost != null)
        {
            Destroy(buildingGhost);
            buildingGhost = null;
        }
        isBuildingMode = false;
        currentBuildingPrefab = null;
        currentBuildingCost = 0;
        Debug.Log("Modo construcción cancelado");
    }
}