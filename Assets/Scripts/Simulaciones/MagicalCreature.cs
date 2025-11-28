// NUEVO: MagicalCreature.cs
using UnityEngine;

public class MagicalCreature : MonoBehaviour
{
    [Header("Creature Type")]
    public CreatureType creatureType;

    [Header("Stats")]
    public int health = 100;
    public int energy = 100;
    public float moveSpeed = 1.5f;
    public float visionRange = 4f;

    [Header("Behavior")]
    public float decisionInterval = 3f;

    public EcosystemManager ecosystemManager;

    private Vector3 targetPosition;
    private float decisionTimer = 0f;
    private float energyTimer = 0f;

    public enum CreatureType { Lumispark, Crystalkin, Guardian }

    void Update()
    {
        decisionTimer += Time.deltaTime;
        energyTimer += Time.deltaTime;

        if (decisionTimer >= decisionInterval)
        {
            MakeDecision();
            decisionTimer = 0f;
        }

        if (energyTimer >= 1f)
        {
            energy -= 2; // Las criaturas consumen energía más rápido
            energyTimer = 0f;

            if (energy <= 0)
                Die();
        }

        MoveToTarget();
    }

    void MakeDecision()
    {
        targetPosition = FindOptimalTarget();
    }

    Vector3 FindOptimalTarget()
    {
        GridManager grid = GridManager.Instance;
        int currentX = Mathf.RoundToInt(transform.position.x);
        int currentY = Mathf.RoundToInt(transform.position.y);

        Vector3 bestTarget = transform.position;
        float bestScore = -999f;

        for (int dx = -(int)visionRange; dx <= visionRange; dx++)
        {
            for (int dy = -(int)visionRange; dy <= visionRange; dy++)
            {
                int checkX = currentX + dx;
                int checkY = currentY + dy;

                if (grid.IsValidPosition(checkX, checkY))
                {
                    float score = CalculateTargetScore(checkX, checkY);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestTarget = new Vector3(checkX, checkY, 0);
                    }
                }
            }
        }

        return bestTarget;
    }

    float CalculateTargetScore(int x, int y)
    {
        GridManager grid = GridManager.Instance;
        float score = 0f;
        float manaDensity = GetManaDensityAt(x, y);
        float corruption = grid.corruptionGrid[x, y];

        switch (creatureType)
        {
            case CreatureType.Lumispark:
                // Atraído por maná, repele corrupción
                score += manaDensity * 1.0f;
                score -= corruption * 0.8f;
                break;

            case CreatureType.Crystalkin:
                // Atraído por corrupción, repele maná
                score += corruption * 1.0f;
                score -= manaDensity * 0.6f;
                break;

            case CreatureType.Guardian:
                // Se mantiene en zonas de conflicto
                score += (manaDensity + corruption) * 0.5f;
                break;
        }

        return score;
    }

    void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
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

    void Die()
    {
        if (ecosystemManager != null)
            ecosystemManager.RemoveCreature(gameObject);

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Interacción con otras criaturas o unidades
        MagicalCreature otherCreature = other.GetComponent<MagicalCreature>();
        if (otherCreature != null)
        {
            // Lógica de interacción entre criaturas
            HandleCreatureInteraction(otherCreature);
        }
    }

    void HandleCreatureInteraction(MagicalCreature other)
    {
        // Ejemplo: Guardian ataca Crystalkin, etc.
        if (creatureType == CreatureType.Guardian && other.creatureType == CreatureType.Crystalkin)
        {
            other.TakeDamage(10);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
            Die();
    }
}