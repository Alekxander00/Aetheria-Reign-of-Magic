// NUEVO: EcosystemManager.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EcosystemManager : MonoBehaviour
{
    [Header("Ecosystem Settings")]
    public float spawnInterval = 3f;
    public int maxCreatures = 20;
    public bool autoSpawn = true;

    [Header("Creature Prefabs")]
    public GameObject lumisparkPrefab;
    public GameObject crystalkinPrefab;
    public GameObject guardianPrefab;

    private List<GameObject> activeCreatures = new List<GameObject>();
    private float spawnTimer = 0f;

    void Update()
    {
        if (!autoSpawn) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval && activeCreatures.Count < maxCreatures)
        {
            TrySpawnCreature();
            spawnTimer = 0f;
        }
    }

    void TrySpawnCreature()
    {
        Vector3 spawnPos = FindSuitableSpawnPosition();
        if (spawnPos != Vector3.zero)
        {
            GameObject creaturePrefab = SelectCreatureType(spawnPos);
            if (creaturePrefab != null)
            {
                GameObject creature = Instantiate(creaturePrefab, spawnPos, Quaternion.identity);
                activeCreatures.Add(creature);

                // Configurar el comportamiento de la criatura
                MagicalCreature creatureBehavior = creature.GetComponent<MagicalCreature>();
                if (creatureBehavior != null)
                {
                    creatureBehavior.ecosystemManager = this;
                }
            }
        }
    }

    Vector3 FindSuitableSpawnPosition()
    {
        GridManager grid = GridManager.Instance;
        int attempts = 0;

        while (attempts < 30)
        {
            Vector3 candidate = new Vector3(
                Random.Range(2, grid.width - 2),
                Random.Range(2, grid.height - 2),
                0
            );

            int x = Mathf.RoundToInt(candidate.x);
            int y = Mathf.RoundToInt(candidate.y);

            if (IsPositionValidForSpawning(x, y))
                return candidate;

            attempts++;
        }

        return Vector3.zero;
    }

    bool IsPositionValidForSpawning(int x, int y)
    {
        GridManager grid = GridManager.Instance;

        // Verificar que la posición esté en el grid
        if (!grid.IsValidPosition(x, y)) return false;

        // Verificar que no esté demasiado cerca de otras criaturas
        foreach (GameObject creature in activeCreatures)
        {
            if (Vector3.Distance(creature.transform.position, new Vector3(x, y, 0)) < 3f)
                return false;
        }

        return true;
    }

    GameObject SelectCreatureType(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x);
        int y = Mathf.RoundToInt(position.y);

        float manaDensity = GetManaDensityAt(x, y);
        float corruptionLevel = GridManager.Instance.corruptionGrid[x, y];

        // Lumispark - Alta densidad de maná, baja corrupción
        if (manaDensity > 0.7f && corruptionLevel < 0.2f)
            return lumisparkPrefab;

        // Crystalkin - Alta corrupción, baja densidad de maná  
        else if (corruptionLevel > 0.6f && manaDensity < 0.3f)
            return crystalkinPrefab;

        // Guardian - Zonas de conflicto (maná y corrupción equilibrados)
        else if (manaDensity > 0.4f && corruptionLevel > 0.4f)
            return guardianPrefab;

        return null;
    }

    float GetManaDensityAt(int x, int y)
    {
        CellState state = GridManager.Instance.manaGrid[x, y];
        switch (state)
        {
            case CellState.TierraNormal: return 0f;
            case CellState.TierraMagica: return 0.5f;
            case CellState.CristalMagico: return 1f;
            case CellState.ArbolAncestral: return 0.8f;
            default: return 0f;
        }
    }

    public void RemoveCreature(GameObject creature)
    {
        activeCreatures.Remove(creature);
    }

    public int GetCreatureCount()
    {
        return activeCreatures.Count;
    }
}