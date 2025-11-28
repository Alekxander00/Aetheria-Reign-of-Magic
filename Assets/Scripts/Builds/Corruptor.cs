using UnityEngine;

public class Corruptor : MonoBehaviour
{
    [Header("Corruptor Properties")]
    public int corruptionGeneration = 5; // Reducido
    public float generationInterval = 5f; // Más lento
    public float corruptionRadius = 4f; // Reducido

    private float timer;

    void Start()
    {
        timer = generationInterval;
        Debug.Log("Pozo corruptor construido - generando corrupción balanceada");
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            GenerateCorruption();
            timer = generationInterval;
        }
    }

    void GenerateCorruption()
    {
        GridManager gridManager = GridManager.Instance;
        if (gridManager == null) return;

        int x = Mathf.RoundToInt(transform.position.x);
        int y = Mathf.RoundToInt(transform.position.y);

        for (int dx = -(int)corruptionRadius; dx <= (int)corruptionRadius; dx++)
        {
            for (int dy = -(int)corruptionRadius; dy <= (int)corruptionRadius; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;

                if (gridManager.IsValidPosition(nx, ny))
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(nx, ny));
                    if (distance <= corruptionRadius)
                    {
                        float strength = 0.2f * (1f - distance / corruptionRadius); // Reducida
                        gridManager.corruptionGrid[nx, ny] = Mathf.Min(1f,
                            gridManager.corruptionGrid[nx, ny] + strength);
                    }
                }
            }
        }

        gridManager.UpdateVisualization();
    }
}