using UnityEngine;
using System.Collections;

public class UIFixer : MonoBehaviour
{
    void Start()
    {
        Debug.Log("UIFixer: Iniciando...");
        StartCoroutine(FixUI());
    }

    IEnumerator FixUI()
    {
        yield return new WaitForSeconds(0.5f);

        Debug.Log("UIFixer: Forzando actualización de toda la UI");

        // Forzar actualización de ResourceUI
        ResourceUI resourceUI = FindObjectOfType<ResourceUI>();
        if (resourceUI != null)
        {
            resourceUI.ForceUpdateUI();
            Debug.Log("UIFixer: ResourceUI actualizado forzadamente");
        }

        // Forzar actualización de BuildingSystem
        BuildingSystem buildingSystem = FindObjectOfType<BuildingSystem>();
        if (buildingSystem != null)
        {
            buildingSystem.UpdateBuildingButtons();
            Debug.Log("UIFixer: BuildingSystem actualizado forzadamente");
        }

        Debug.Log("UIFixer: Proceso completado");
    }
}