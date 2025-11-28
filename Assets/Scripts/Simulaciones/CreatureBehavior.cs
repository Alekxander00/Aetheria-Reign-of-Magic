// Nuevo script: CreatureBehavior.cs
using UnityEngine;
using System.Collections.Generic;

public class CreatureBehavior : MonoBehaviour
{
    [Header("Creature Stats")]
    public CreatureType creatureType;
    public int health = 100;
    public int energy = 100;
    public float moveSpeed = 1.2f;
    public float visionRange = 3f;

    [Header("Behavior Weights")]
    public float hungerWeight = 0.6f;
    public float safetyWeight = 0.3f;
    public float reproductionWeight = 0.1f;

    private Vector3 targetPosition;
    private CreatureState currentState = CreatureState.Exploring;
    private float decisionCooldown = 0f;
    private float energyTimer = 0f;

    public enum CreatureType { Lumispark, Crystalkin, Guardian, Spirit }
    public enum CreatureState { Exploring, Feeding, Fleeing, Reproducing, Combat }

    void Update()
    {
        UpdateEnergy();
        MakeDecision();
        Move();
    }

    void MakeDecision()
    {
        decisionCooldown -= Time.deltaTime;
        if (decisionCooldown <= 0f)
        {
            Vector3 bestTarget = FindOptimalTarget();
            targetPosition = bestTarget;
            decisionCooldown = Random.Range(2f, 5f);
        }
    }

    Vector3 FindOptimalTarget()
    {
        List<Vector3> potentialTargets = new List<Vector3>();
        List<float> targetScores = new List<float>();

        // Buscar en radio de visión
        for (int dx = -(int)visionRange; dx <= visionRange; dx++)
        {
            for (int dy = -(int)visionRange; dy <= visionRange; dy++)
            {
                int checkX = Mathf.RoundToInt(transform.position.x) + dx;
                int checkY = Mathf.RoundToInt(transform.position.y) + dy;

                if (GridManager.Instance.IsValidPosition(checkX, checkY))
                {
                    float score = CalculateTargetScore(checkX, checkY);
                    potentialTargets.Add(new Vector3(checkX, checkY, 0));
                    targetScores.Add(score);
                }
            }
        }

        // Elegir el mejor objetivo
        if (targetScores.Count > 0)
        {
            int bestIndex = 0;
            for (int i = 1; i < targetScores.Count; i++)
            {
                if (targetScores[i] > targetScores[bestIndex])
                    bestIndex = i;
            }
            return potentialTargets[bestIndex];
        }

        return transform.position; // Mantener posición actual
    }

    float CalculateTargetScore(int x, int y)
    {
        float score = 0f;
        GridManager grid = GridManager.Instance;

        switch (creatureType)
        {
            case CreatureType.Lumispark:
                // Atraído por maná, repele corrupción
                score += GetManaDensityAt(x, y) * hungerWeight;
                score -= grid.corruptionGrid[x, y] * safetyWeight;
                break;

            case CreatureType.Crystalkin:
                // Atraído por corrupción y áreas oscuras
                score += grid.corruptionGrid[x, y] * hungerWeight;
                score -= GetManaDensityAt(x, y) * safetyWeight * 0.5f;
                break;
        }

        return score;
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

    void UpdateEnergy()
    {
        energyTimer += Time.deltaTime;
        if (energyTimer >= 1f)
        {
            energy -= 1;
            energyTimer = 0f;

            if (energy <= 0)
                Die();
        }
    }

    void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition,
                                               moveSpeed * Time.deltaTime);
    }

    void Die()
    {
        Destroy(gameObject);
    }
}