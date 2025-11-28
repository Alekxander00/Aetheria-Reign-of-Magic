using UnityEngine;

public class UnitManager : MonoBehaviour
{
    [Header("Unit Prefabs")]
    public GameObject noviceMagePrefab;
    public GameObject corruptSlavePrefab;

    [Header("Unit Costs")]
    public int noviceMageCost = 50;
    public int corruptSlaveCost = 50;

    public void SpawnNoviceMage()
    {
        if (GameManager.Instance.CanBuild(noviceMageCost, true))
        {
            Vector3 spawnPos = FindSafeSpawnPosition();
            Instantiate(noviceMagePrefab, spawnPos, Quaternion.identity);
            GameManager.Instance.SpendResources(noviceMageCost);
            Debug.Log("Novice Mage creado!");
        }
        else
        {
            Debug.Log("No hay recursos suficientes para Novice Mage");
        }
    }

    public void SpawnCorruptSlave()
    {
        if (GameManager.Instance.CanBuild(corruptSlaveCost, true))
        {
            Vector3 spawnPos = FindSafeSpawnPosition();
            Instantiate(corruptSlavePrefab, spawnPos, Quaternion.identity);
            GameManager.Instance.SpendResources(corruptSlaveCost);
            Debug.Log("Corrupt Slave creado!");
        }
        else
        {
            Debug.Log("No hay recursos suficientes para Corrupt Slave");
        }
    }

    Vector3 FindSafeSpawnPosition()
    {
        GridManager gridManager = GridManager.Instance;
        int x = Random.Range(5, gridManager.width - 5);
        int y = Random.Range(5, gridManager.height - 5);
        return new Vector3(x, y, 0);
    }
}