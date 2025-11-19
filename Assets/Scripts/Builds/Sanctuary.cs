using UnityEngine;

public class Sanctuary : MonoBehaviour
{
    [Header("Sanctuary Properties")]
    public int manaGeneration = 5;
    public float generationInterval = 5f;

    void Start()
    {
        // El santuario ya está contribuyendo a la generación a través del GameManager
        Debug.Log("Santuario construido - generando maná pasivamente");
    }
}