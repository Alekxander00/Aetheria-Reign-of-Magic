using UnityEngine;
using UnityEngine.UI;

public class BuildingSystem : MonoBehaviour
{
    [Header("Building Prefabs")]
    public GameObject sanctuaryPrefab;
    public GameObject corruptorPrefab;

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
        StartCoroutine(InitializeButtons());
    }

    private System.Collections.IEnumerator InitializeButtons()
    {
        yield return new WaitForEndOfFrame();
        UpdateBuildingButtons();
        Debug.Log("BuildingSystem inicializado completamente");
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGamePaused) return;

        if (!isBuildingMode) return;

        if (buildingGhost == null && currentBuildingPrefab != null)
        {
            buildingGhost = Instantiate(currentBuildingPrefab);
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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelBuilding();
        }

        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceBuilding();
        }
    }

    public void UpdateBuildingButtons()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance es null en BuildingSystem");
            return;
        }

        Debug.Log($"BuildingSystem: Actualizando botones para {GameManager.Instance.currentFaction}");

        // Primero ocultar todos los botones
        if (buildSanctuaryButton != null)
        {
            buildSanctuaryButton.gameObject.SetActive(false);
            Debug.Log("BuildingSystem: Santuario ocultado");
        }
        if (buildCorruptorButton != null)
        {
            buildCorruptorButton.gameObject.SetActive(false);
            Debug.Log("BuildingSystem: Corruptor ocultado");
        }

        // Luego mostrar solo los de la facción actual
        if (GameManager.Instance.currentFaction == GameManager.PlayerFaction.Mana)
        {
            if (buildSanctuaryButton != null)
            {
                buildSanctuaryButton.gameObject.SetActive(true);
                Debug.Log("✅ BuildingSystem: Botón Santuario ACTIVADO");
            }
        }
        else if (GameManager.Instance.currentFaction == GameManager.PlayerFaction.Corruption)
        {
            if (buildCorruptorButton != null)
            {
                buildCorruptorButton.gameObject.SetActive(true);
                Debug.Log("✅ BuildingSystem: Botón Corruptor ACTIVADO");
            }
        }
        else
        {
            Debug.LogWarning("BuildingSystem: Facción neutral o no definida");
        }
    }

    public void StartBuildingSanctuary()
    {
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

        GridManager gridManager = GridManager.Instance;
        int x = Mathf.RoundToInt(worldPos.x);
        int y = Mathf.RoundToInt(worldPos.y);

        if (!gridManager.IsValidPosition(x, y))
        {
            Debug.Log("Posición inválida para construir");
            return;
        }

        bool canBuildHere = false;
        string terrainError = "";

        if (GameManager.Instance.currentFaction == GameManager.PlayerFaction.Mana)
        {
            canBuildHere = gridManager.manaGrid[x, y] != CellState.TierraNormal;
            terrainError = "Solo puedes construir en Tierra Mágica, Cristales o cerca de Árboles Ancestrales";
        }
        else if (GameManager.Instance.currentFaction == GameManager.PlayerFaction.Corruption)
        {
            canBuildHere = gridManager.corruptionGrid[x, y] > 0.3f;
            terrainError = "Solo puedes construir en áreas con alta corrupción";
        }

        if (canBuildHere)
        {
            Instantiate(currentBuildingPrefab, worldPos, Quaternion.identity);
            GameManager.Instance.SpendResources(currentBuildingCost);
            Debug.Log("¡Edificio construido!");

            ApplyBuildingEffects(x, y);
            CancelBuilding();
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
            // Santuario suprime corrupción en radio 7
            for (int dx = -7; dx <= 7; dx++)
            {
                for (int dy = -7; dy <= 7; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    if (gridManager.IsValidPosition(nx, ny))
                    {
                        float distance = Vector2.Distance(new Vector2(x, y), new Vector2(nx, ny));
                        if (distance <= 7)
                        {
                            float reduction = 0.6f * (1f - distance / 7f);
                            gridManager.corruptionGrid[nx, ny] = Mathf.Max(0,
                                gridManager.corruptionGrid[nx, ny] - reduction);
                        }
                    }
                }
            }
            gridManager.UpdateVisualization();
        }
        else if (currentBuildingPrefab == corruptorPrefab)
        {
            // Pozo corruptor expande corrupción en radio 5
            for (int dx = -5; dx <= 5; dx++)
            {
                for (int dy = -5; dy <= 5; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    if (gridManager.IsValidPosition(nx, ny))
                    {
                        gridManager.corruptionGrid[nx, ny] = Mathf.Min(1f,
                            gridManager.corruptionGrid[nx, ny] + 0.3f);
                    }
                }
            }
            gridManager.UpdateVisualization();
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