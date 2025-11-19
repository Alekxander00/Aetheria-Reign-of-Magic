using UnityEngine;

public class UnitBehavior : MonoBehaviour
{
    [Header("Unit Stats")]
    public int health = 100;
    public int damage = 10;
    public float moveSpeed = 2f;
    public float actionRange = 3f;

    [Header("Unit Type")]
    public bool isManaUnit = true;

    private Vector3 targetPosition;
    private bool hasTarget = false;
    private float actionCooldown = 0f;
    private float actionInterval = 1f; // Acción cada 1 segundo

    void Start()
    {
        FindNewPatrolTarget();
    }

    void Update()
    {
        if (hasTarget)
        {
            // Moverse hacia el objetivo
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Si llegó al objetivo, buscar nuevo
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                FindNewPatrolTarget();
            }
        }

        // Realizar acción (purificar/corromper) cada cierto tiempo
        actionCooldown -= Time.deltaTime;
        if (actionCooldown <= 0f)
        {
            PerformAction();
            actionCooldown = actionInterval;
        }
    }

    void FindNewPatrolTarget()
    {
        GridManager gridManager = GridManager.Instance;
        targetPosition = new Vector3(
            Random.Range(0, gridManager.width),
            Random.Range(0, gridManager.height),
            0
        );
        hasTarget = true;
    }

    void PerformAction()
    {
        GridManager gridManager = GridManager.Instance;
        int x = Mathf.RoundToInt(transform.position.x);
        int y = Mathf.RoundToInt(transform.position.y);

        if (!gridManager.IsValidPosition(x, y)) return;

        if (isManaUnit)
        {
            // Unidad de maná purifica corrupción cercana
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    if (gridManager.IsValidPosition(nx, ny))
                    {
                        gridManager.corruptionGrid[nx, ny] = Mathf.Max(0, gridManager.corruptionGrid[nx, ny] - 0.1f);
                    }
                }
            }
        }
        else
        {
            // Unidad de corrupción expande corrupción
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    if (gridManager.IsValidPosition(nx, ny))
                    {
                        gridManager.corruptionGrid[nx, ny] = Mathf.Min(1f, gridManager.corruptionGrid[nx, ny] + 0.1f);
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