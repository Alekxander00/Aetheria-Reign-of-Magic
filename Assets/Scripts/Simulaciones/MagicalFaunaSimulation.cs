using UnityEngine;
using System.Collections.Generic;

public class MagicalFaunaSimulation : MonoBehaviour
{
    [Header("Magical Fauna Simulation")]
    public float updateTime = 2f;
    public bool autoSimulate = true;

    [Header("Fauna Settings")]
    public int maxFauna = 15;
    public float spawnChance = 0.3f;
    public float faunaMoveSpeed = 1.5f;

    [Header("Fauna Prefabs")]
    public GameObject lumisparkPrefab;
    public GameObject shadowlingPrefab;
    public GameObject neutralSpiritPrefab;

    [Header("2D Visual Effects")]
    public GameObject lumisparkTrailEffect;
    public GameObject shadowlingTrailEffect;
    public GameObject spiritTrailEffect;

    [Header("Audio System")]
    public AudioSource audioSource;
    public AudioClip lumisparkSpawnSound;
    public AudioClip shadowlingSpawnSound;
    public AudioClip spiritSpawnSound;
    public AudioClip faunaMoveSound;
    public AudioClip faunaEffectSound;

    [Header("Audio Settings")]
    [Range(0f, 1f)] public float spawnVolume = 0.6f;
    [Range(0f, 1f)] public float moveVolume = 0.3f;
    [Range(0f, 1f)] public float effectVolume = 0.4f;

    [Header("Ambient Fauna Sounds")]
    public AudioClip ambientFaunaSounds;
    public float ambientSoundInterval = 8f;

    private List<FaunaCreature> activeFauna = new List<FaunaCreature>();
    private float timer = 0f;
    private float ambientTimer = 0f;
    private bool isPaused = false;

    void Start()
    {
        // Configurar audio source si no está asignado
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        ambientTimer = ambientSoundInterval;
    }

    void Update()
    {
        if (isPaused || !autoSimulate) return;

        timer += Time.deltaTime;
        ambientTimer += Time.deltaTime;

        if (timer >= updateTime)
        {
            SimulateStep();
            timer = 0f;
        }

        // Reproducir sonidos ambientales ocasionales
        if (ambientTimer >= ambientSoundInterval)
        {
            PlayAmbientSound();
            ambientTimer = 0f;
        }
    }

    public void SimulateStep()
    {
        TrySpawnFauna();
        MoveFauna();
        ExecuteFaunaEffects();
    }

    void TrySpawnFauna()
    {
        if (activeFauna.Count >= maxFauna) return;

        if (Random.value < spawnChance)
        {
            SpawnRandomFauna();
        }
    }

    void SpawnRandomFauna()
    {
        Vector3 spawnPos = FindSuitableSpawnPosition();
        if (spawnPos == Vector3.zero) return;

        GameObject faunaPrefab = SelectFaunaType(spawnPos);
        if (faunaPrefab != null)
        {
            GameObject faunaObject = Instantiate(faunaPrefab, spawnPos, Quaternion.identity);

            // Agregar efectos visuales según el tipo
            AddVisualEffects(faunaObject, faunaPrefab);

            // Reproducir sonido de aparición
            PlaySpawnSound(faunaPrefab);

            FaunaCreature fauna = new FaunaCreature();
            fauna.gameObject = faunaObject;
            fauna.position = spawnPos;
            fauna.type = GetFaunaTypeFromPrefab(faunaPrefab);
            fauna.energy = 100f;

            activeFauna.Add(fauna);

            Debug.Log($"¡{fauna.type} apareció en ({spawnPos.x}, {spawnPos.y})!");
        }
    }

    void AddVisualEffects(GameObject faunaObject, GameObject prefab)
    {
        GameObject trailEffect = null;

        if (prefab == lumisparkPrefab)
        {
            trailEffect = lumisparkTrailEffect;
            SpriteRenderer sprite = faunaObject.GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                sprite.color = new Color(1f, 1f, 0.8f, 1f);
            }
        }
        else if (prefab == shadowlingPrefab)
        {
            trailEffect = shadowlingTrailEffect;
            SpriteRenderer sprite = faunaObject.GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                sprite.color = new Color(0.3f, 0f, 0.5f, 1f);
            }
        }
        else if (prefab == neutralSpiritPrefab)
        {
            trailEffect = spiritTrailEffect;
            SpriteRenderer sprite = faunaObject.GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                sprite.color = new Color(1f, 1f, 1f, 0.7f);
            }
        }

        if (trailEffect != null)
        {
            GameObject trail = Instantiate(trailEffect, faunaObject.transform);
            trail.transform.localPosition = Vector3.zero;
        }
    }

    void PlaySpawnSound(GameObject faunaPrefab)
    {
        if (audioSource == null) return;

        AudioClip clip = null;
        if (faunaPrefab == lumisparkPrefab)
            clip = lumisparkSpawnSound;
        else if (faunaPrefab == shadowlingPrefab)
            clip = shadowlingSpawnSound;
        else if (faunaPrefab == neutralSpiritPrefab)
            clip = spiritSpawnSound;

        if (clip != null)
        {
            audioSource.PlayOneShot(clip, spawnVolume);
        }
    }

    void PlayAmbientSound()
    {
        if (audioSource != null && ambientFaunaSounds != null && activeFauna.Count > 0)
        {
            // Solo reproducir si hay fauna activa
            if (Random.value < 0.7f) // 70% de probabilidad
            {
                audioSource.PlayOneShot(ambientFaunaSounds, moveVolume * 0.5f);
            }
        }
    }

    Vector3 FindSuitableSpawnPosition()
    {
        GridManager grid = GridManager.Instance;
        int attempts = 0;

        while (attempts < 20)
        {
            Vector3 candidate = new Vector3(
                Random.Range(2, grid.width - 2),
                Random.Range(2, grid.height - 2),
                0
            );

            if (IsPositionValidForFauna(candidate))
                return candidate;

            attempts++;
        }

        return Vector3.zero;
    }

    bool IsPositionValidForFauna(Vector3 position)
    {
        foreach (FaunaCreature fauna in activeFauna)
        {
            if (Vector3.Distance(fauna.position, position) < 3f)
                return false;
        }

        return true;
    }

    GameObject SelectFaunaType(Vector3 position)
    {
        GridManager grid = GridManager.Instance;
        int x = Mathf.RoundToInt(position.x);
        int y = Mathf.RoundToInt(position.y);

        float manaDensity = GetManaDensityAt(x, y);
        float corruptionLevel = grid.corruptionGrid[x, y];

        if (manaDensity > 0.7f && corruptionLevel < 0.2f)
            return lumisparkPrefab;

        if (corruptionLevel > 0.6f && manaDensity < 0.3f)
            return shadowlingPrefab;

        if (Mathf.Abs(manaDensity - corruptionLevel) < 0.3f)
            return neutralSpiritPrefab;

        return null;
    }

    void MoveFauna()
    {
        for (int i = activeFauna.Count - 1; i >= 0; i--)
        {
            FaunaCreature fauna = activeFauna[i];

            fauna.energy -= 5f;
            if (fauna.energy <= 0f)
            {
                RemoveFauna(fauna);
                continue;
            }

            Vector3 newPosition = FindOptimalPosition(fauna);
            fauna.position = Vector3.MoveTowards(fauna.position, newPosition, faunaMoveSpeed * Time.deltaTime * 10f);
            fauna.gameObject.transform.position = fauna.position;

            // Reproducir sonido de movimiento ocasionalmente
            if (Random.value < 0.1f && faunaMoveSound != null)
            {
                audioSource.PlayOneShot(faunaMoveSound, moveVolume * 0.3f);
            }
        }
    }

    Vector3 FindOptimalPosition(FaunaCreature fauna)
    {
        GridManager grid = GridManager.Instance;
        Vector3 currentPos = fauna.position;
        Vector3 bestPosition = currentPos;
        float bestScore = -999f;

        for (int dx = -3; dx <= 3; dx++)
        {
            for (int dy = -3; dy <= 3; dy++)
            {
                Vector3 candidate = currentPos + new Vector3(dx, dy, 0);
                int x = Mathf.RoundToInt(candidate.x);
                int y = Mathf.RoundToInt(candidate.y);

                if (grid.IsValidPosition(x, y))
                {
                    float score = CalculatePositionScore(fauna, x, y);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestPosition = candidate;
                    }
                }
            }
        }

        return bestPosition;
    }

    float CalculatePositionScore(FaunaCreature fauna, int x, int y)
    {
        GridManager grid = GridManager.Instance;
        float score = 0f;
        float manaDensity = GetManaDensityAt(x, y);
        float corruptionLevel = grid.corruptionGrid[x, y];

        switch (fauna.type)
        {
            case FaunaType.Lumispark:
                score += manaDensity * 0.8f;
                score -= corruptionLevel * 0.9f;
                break;

            case FaunaType.Shadowling:
                score += corruptionLevel * 0.8f;
                score -= manaDensity * 0.7f;
                break;

            case FaunaType.NeutralSpirit:
                float balance = 1f - Mathf.Abs(manaDensity - corruptionLevel);
                score += balance * 0.5f;
                break;
        }

        return score;
    }

    void ExecuteFaunaEffects()
    {
        GridManager grid = GridManager.Instance;

        foreach (FaunaCreature fauna in activeFauna)
        {
            int x = Mathf.RoundToInt(fauna.position.x);
            int y = Mathf.RoundToInt(fauna.position.y);

            if (!grid.IsValidPosition(x, y)) continue;

            // Efecto visual cuando la fauna afecta el entorno
            CreateFaunaEffect(fauna, x, y);

            // Reproducir sonido de efecto ocasionalmente
            if (Random.value < 0.15f && faunaEffectSound != null)
            {
                audioSource.PlayOneShot(faunaEffectSound, effectVolume * 0.2f);
            }

            switch (fauna.type)
            {
                case FaunaType.Lumispark:
                    if (grid.manaGrid[x, y] == CellState.TierraNormal && Random.value < 0.1f)
                    {
                        grid.manaGrid[x, y] = CellState.TierraMagica;
                    }
                    grid.corruptionGrid[x, y] = Mathf.Max(0, grid.corruptionGrid[x, y] - 0.1f);
                    break;

                case FaunaType.Shadowling:
                    grid.corruptionGrid[x, y] = Mathf.Min(1f, grid.corruptionGrid[x, y] + 0.1f);
                    if (grid.manaGrid[x, y] == CellState.TierraMagica && Random.value < 0.05f)
                    {
                        grid.manaGrid[x, y] = CellState.TierraNormal;
                    }
                    break;

                case FaunaType.NeutralSpirit:
                    if (grid.corruptionGrid[x, y] > 0.5f)
                    {
                        grid.corruptionGrid[x, y] -= 0.05f;
                    }
                    else if (grid.corruptionGrid[x, y] < 0.3f && grid.manaGrid[x, y] == CellState.TierraNormal)
                    {
                        if (Random.value < 0.08f)
                        {
                            grid.manaGrid[x, y] = CellState.TierraMagica;
                        }
                    }
                    break;
            }
        }

        grid.UpdateVisualization();
    }

    void CreateFaunaEffect(FaunaCreature fauna, int x, int y)
    {
        GameObject effect = null;

        switch (fauna.type)
        {
            case FaunaType.Lumispark:
                effect = lumisparkTrailEffect;
                break;
            case FaunaType.Shadowling:
                effect = shadowlingTrailEffect;
                break;
            case FaunaType.NeutralSpirit:
                effect = spiritTrailEffect;
                break;
        }

        if (effect != null && Random.value < 0.3f)
        {
            Vector3 effectPos = new Vector3(x, y, -0.5f);
            GameObject newEffect = Instantiate(effect, effectPos, Quaternion.identity);
            Destroy(newEffect, 1f);
        }
    }

    void RemoveFauna(FaunaCreature fauna)
    {
        // Efecto de desaparición
        if (fauna.gameObject != null)
        {
            CreateDeathEffect(fauna.position, fauna.type);
            Destroy(fauna.gameObject);
        }
        activeFauna.Remove(fauna);
    }

    void CreateDeathEffect(Vector3 position, FaunaType type)
    {
        GameObject effect = null;

        switch (type)
        {
            case FaunaType.Lumispark:
                effect = lumisparkTrailEffect;
                break;
            case FaunaType.Shadowling:
                effect = shadowlingTrailEffect;
                break;
            case FaunaType.NeutralSpirit:
                effect = spiritTrailEffect;
                break;
        }

        if (effect != null)
        {
            GameObject deathEffect = Instantiate(effect, position, Quaternion.identity);
            ParticleSystem particles = deathEffect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                var main = particles.main;
                main.loop = false;
                main.startLifetime = 0.5f;
            }
            Destroy(deathEffect, 2f);
        }
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

    FaunaType GetFaunaTypeFromPrefab(GameObject prefab)
    {
        if (prefab == lumisparkPrefab) return FaunaType.Lumispark;
        if (prefab == shadowlingPrefab) return FaunaType.Shadowling;
        if (prefab == neutralSpiritPrefab) return FaunaType.NeutralSpirit;
        return FaunaType.Lumispark;
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
    }

    public void ClearFauna()
    {
        foreach (FaunaCreature fauna in activeFauna)
        {
            Destroy(fauna.gameObject);
        }
        activeFauna.Clear();
    }

    public int GetFaunaCount()
    {
        return activeFauna.Count;
    }

    // Métodos para control de audio
    public void SetSpawnVolume(float volume)
    {
        spawnVolume = Mathf.Clamp01(volume);
    }

    public void SetMoveVolume(float volume)
    {
        moveVolume = Mathf.Clamp01(volume);
    }

    public void SetEffectVolume(float volume)
    {
        effectVolume = Mathf.Clamp01(volume);
    }
}

[System.Serializable]
public class FaunaCreature
{
    public GameObject gameObject;
    public Vector3 position;
    public FaunaType type;
    public float energy;
}

public enum FaunaType
{
    Lumispark,
    Shadowling,
    NeutralSpirit
}