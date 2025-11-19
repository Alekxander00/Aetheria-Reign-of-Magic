using UnityEngine;

public class Corruptor : MonoBehaviour
{
    [Header("Corruptor Properties")]
    public int corruptionGeneration = 5;
    public float generationInterval = 5f;

    void Start()
    {
        // El pozo corruptor ya está contribuyendo a la generación a través del GameManager
        Debug.Log("Pozo corruptor construido - generando corrupción pasivamente");
    }
}