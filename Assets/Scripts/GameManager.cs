using System.Collections;
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
    public int buildingLimit = 5;
    public int currentBuildings = 0;

    [Header("Resource Generation")]
    public float resourceGenerationInterval = 5f; // Cada 5 segundos
    public int baseManaGeneration = 10;
    public int baseCorruptionGeneration = 10;

    private float generationTimer = 0f;

    void Start()
    {
        // Iniciar generación de recursos
        StartCoroutine(ResourceGenerationRoutine());
    }

    IEnumerator ResourceGenerationRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(resourceGenerationInterval);
            GenerateResources();
        }
    }

    void GenerateResources()
    {
        if (currentFaction == PlayerFaction.Mana)
        {
            // Generar maná basado en edificios y territorio
            int manaFromBuildings = CountManaBuildings() * 5;
            int manaFromTerritory = CalculateManaTerritory() * 2;

            manaResource += baseManaGeneration + manaFromBuildings + manaFromTerritory;
            Debug.Log($"Generado +{baseManaGeneration + manaFromBuildings + manaFromTerritory} maná");
        }
        else if (currentFaction == PlayerFaction.Corruption)
        {
            // Generar corrupción basado en edificios y territorio
            int corruptionFromBuildings = CountCorruptionBuildings() * 5;
            int corruptionFromTerritory = CalculateCorruptionTerritory() * 2;

            corruptionResource += baseCorruptionGeneration + corruptionFromBuildings + corruptionFromTerritory;
            Debug.Log($"Generado +{baseCorruptionGeneration + corruptionFromBuildings + corruptionFromTerritory} corrupción");
        }
    }

    int CountManaBuildings()
    {
        // Contar santuarios en la escena
        Sanctuary[] sanctuaries = FindObjectsOfType<Sanctuary>();
        return sanctuaries.Length;
    }

    int CountCorruptionBuildings()
    {
        // Contar pozos corruptores en la escena
        Corruptor[] corruptors = FindObjectsOfType<Corruptor>();
        return corruptors.Length;
    }

    int CalculateManaTerritory()
    {
        // Calcular porcentaje del mapa controlado por maná
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

        int totalCells = gridManager.width * gridManager.height;
        return (manaCells * 100) / totalCells; // Porcentaje
    }

    int CalculateCorruptionTerritory()
    {
        // Calcular porcentaje del mapa controlado por corrupción
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

        int totalCells = gridManager.width * gridManager.height;
        return (corruptionCells * 100) / totalCells; // Porcentaje
    }

    public void Awake()
    {
        // Sistema Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persiste entre escenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChooseFaction(PlayerFaction faction)
    {
        currentFaction = faction;
        Debug.Log($"Jugador eligió: {faction}");

        // Inicializar recursos según facción
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

        // Cargar escena del juego
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

        currentBuildings++;
    }
}