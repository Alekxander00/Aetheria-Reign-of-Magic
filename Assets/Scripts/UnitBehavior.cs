using UnityEngine;
using System.Collections.Generic;

public class UnitBehavior : MonoBehaviour
{
    [Header("Unit Stats")]
    public int health = 100;
    public int damage = 10;
    public float moveSpeed = 2f;
    public float actionRange = 3f;
    public float visionRange = 6f;

    [Header("Unit Type")]
    public bool isManaUnit = true;

    [Header("Behavior Settings")]
    public float decisionInterval = 2f;

    [Header("Combat Settings")]
    public float attackCooldown = 2f; // Más lento
    public int attackDamage = 5;

    private Vector3 targetPosition;
    private bool hasTarget = false;
    private float actionCooldown = 0f;
    private float actionInterval = 1.5f; // Más lento
    private float decisionCooldown = 0f;
    private float attackTimer = 0f;

    void Start()
    {
        FindStrategicTarget();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGamePaused) return;

        // Tu código original del Update aquí...
        UpdateMovement();
        UpdateActions();
        UpdateCombat();
    }

    void UpdateMovement()
    {
        decisionCooldown -= Time.deltaTime;

        if (!hasTarget || Vector3.Distance(transform.position, targetPosition) < 0.5f || decisionCooldown <= 0f)
        {
            FindStrategicTarget();
            decisionCooldown = decisionInterval;
        }

        if (hasTarget)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition,
                                                   moveSpeed * Time.deltaTime);
        }
    }

    void UpdateActions()
    {
        actionCooldown -= Time.deltaTime;
        if (actionCooldown <= 0f)
        {
            PerformAction();
            actionCooldown = actionInterval;
        }
    }

    void UpdateCombat()
    {
        attackTimer -= Time.deltaTime;

        if (!isManaUnit && attackTimer <= 0f)
        {
            TryAttackStructures();
            attackTimer = attackCooldown;
        }
    }

    void FindStrategicTarget()
    {
        Vector3 newTarget = transform.position;
        float bestScore = -999f;

        GridManager grid = GridManager.Instance;
        int currentX = Mathf.RoundToInt(transform.position.x);
        int currentY = Mathf.RoundToInt(transform.position.y);

        for (int dx = -(int)visionRange; dx <= (int)visionRange; dx++)
        {
            for (int dy = -(int)visionRange; dy <= (int)visionRange; dy++)
            {
                int checkX = currentX + dx;
                int checkY = currentY + dy;

                if (grid.IsValidPosition(checkX, checkY))
                {
                    float score = CalculateStrategicScore(checkX, checkY);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        newTarget = new Vector3(checkX, checkY, 0);
                    }
                }
            }
        }

        if (bestScore > 0.1f)
        {
            targetPosition = newTarget;
            hasTarget = true;
        }
        else
        {
            FindRandomTarget();
        }
    }

    float CalculateStrategicScore(int x, int y)
    {
        GridManager grid = GridManager.Instance;
        float score = 0f;

        if (isManaUnit)
        {
            score += grid.corruptionGrid[x, y] * 0.7f;
            score += GetManaDensityAt(x, y) * 0.3f;

            if (grid.corruptionGrid[x, y] > 0.8f)
                score -= 0.3f;
        }
        else
        {
            score += GetManaDensityAt(x, y) * 0.5f;
            score += grid.corruptionGrid[x, y] * 0.3f;

            CellState state = grid.manaGrid[x, y];
            if (state == CellState.ArbolAncestral || state == CellState.CristalMagico)
            {
                score += 0.3f;
            }

            score -= GetSanctuaryInfluence(x, y) * 0.5f;
        }

        float distance = Vector3.Distance(transform.position, new Vector3(x, y, 0));
        score -= distance * 0.1f;

        return score;
    }

    void TryAttackStructures()
    {
        GridManager grid = GridManager.Instance;
        int x = Mathf.RoundToInt(transform.position.x);
        int y = Mathf.RoundToInt(transform.position.y);

        if (!grid.IsValidPosition(x, y)) return;

        CellState currentState = grid.manaGrid[x, y];

        if (currentState == CellState.ArbolAncestral || currentState == CellState.CristalMagico)
        {
            if (currentState == CellState.ArbolAncestral)
            {
                // 10% de chance de destruir el árbol (reducida)
                if (Random.value < 0.1f)
                {
                    grid.manaGrid[x, y] = CellState.TierraNormal;
                    Debug.Log("¡Árbol ancestral destruido por la corrupción!");
                    grid.UpdateVisualization();
                }
            }
            else if (currentState == CellState.CristalMagico)
            {
                // 15% de chance de degradar el cristal (reducida)
                if (Random.value < 0.15f)
                {
                    grid.manaGrid[x, y] = CellState.TierraMagica;
                    Debug.Log("¡Cristal mágico degradado por la corrupción!");
                    grid.UpdateVisualization();
                }
            }
        }
    }

    void FindRandomTarget()
    {
        GridManager gridManager = GridManager.Instance;
        targetPosition = new Vector3(
            Random.Range(2, gridManager.width - 2),
            Random.Range(2, gridManager.height - 2),
            0
        );
        hasTarget = true;
    }

    float GetManaDensityAt(int x, int y)
    {
        CellState state = GridManager.Instance.manaGrid[x, y];
        switch (state)
        {
            case CellState.TierraNormal: return 0f;
            case CellState.TierraMagica: return 0.5f;
            case CellState.CristalMagico: return 0.8f;
            case CellState.ArbolAncestral: return 0.7f;
            default: return 0f;
        }
    }

    float GetSanctuaryInfluence(int x, int y)
    {
        Sanctuary[] sanctuaries = FindObjectsOfType<Sanctuary>();
        float totalInfluence = 0f;

        foreach (Sanctuary sanctuary in sanctuaries)
        {
            float distance = Vector3.Distance(sanctuary.transform.position, new Vector3(x, y, 0));
            if (distance <= 7f)
            {
                totalInfluence += (1f - distance / 7f);
            }
        }

        return Mathf.Min(totalInfluence, 1f);
    }

    void PerformAction()
    {
        GridManager gridManager = GridManager.Instance;
        int x = Mathf.RoundToInt(transform.position.x);
        int y = Mathf.RoundToInt(transform.position.y);

        if (!gridManager.IsValidPosition(x, y)) return;

        if (isManaUnit)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    if (gridManager.IsValidPosition(nx, ny))
                    {
                        gridManager.corruptionGrid[nx, ny] = Mathf.Max(0, gridManager.corruptionGrid[nx, ny] - 0.15f); // Reducida
                    }
                }
            }
        }
        else
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    if (gridManager.IsValidPosition(nx, ny))
                    {
                        gridManager.corruptionGrid[nx, ny] = Mathf.Min(1f, gridManager.corruptionGrid[nx, ny] + 0.15f); // Reducida
                    }
                }
            }
        }

        gridManager.UpdateVisualization();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}