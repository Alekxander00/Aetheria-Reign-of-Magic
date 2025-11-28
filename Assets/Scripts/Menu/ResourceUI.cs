using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ResourceUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI resourceText;
    public TextMeshProUGUI factionText;
    public TextMeshProUGUI buildingsText;

    [Header("Building Buttons")]
    public Button buildSanctuaryButton;
    public Button buildCorruptorButton;

    [Header("Unit Buttons")]
    public Button spawnMageButton;
    public Button spawnSlaveButton;

    private bool isInitialized = false;

    void Start()
    {
        Debug.Log("ResourceUI: Iniciando...");
        StartCoroutine(InitializeUI());
    }

    IEnumerator InitializeUI()
    {
        // Esperar a que GameManager esté listo
        yield return new WaitUntil(() => GameManager.Instance != null);
        yield return new WaitForSeconds(0.1f); // Pequeño delay adicional

        Debug.Log($"ResourceUI: GameManager listo. Facción: {GameManager.Instance.currentFaction}");

        UpdateButtonVisibility();
        UpdateResourceUI();
        isInitialized = true;

        Debug.Log("ResourceUI: Inicialización completada");
    }

    void Update()
    {
        if (!isInitialized) return;

        UpdateResourceUI();
    }

    void UpdateResourceUI()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance es null en UpdateResourceUI");
            return;
        }

        // Actualizar textos según la facción
        if (GameManager.Instance.currentFaction == GameManager.PlayerFaction.Mana)
        {
            resourceText.text = $"Maná: {GameManager.Instance.manaResource}";
            factionText.text = "Facción: Alianza de la Magia";
            factionText.color = Color.cyan;
        }
        else if (GameManager.Instance.currentFaction == GameManager.PlayerFaction.Corruption)
        {
            resourceText.text = $"Corrupción: {GameManager.Instance.corruptionResource}";
            factionText.text = "Facción: Legión de la Corrupción";
            factionText.color = Color.red;
        }

        // Actualizar contador de edificios
        buildingsText.text = $"Edificios: {GameManager.Instance.currentBuildings}/{GameManager.Instance.buildingLimit}";

        // Cambiar color si se acerca al límite
        if (GameManager.Instance.currentBuildings >= GameManager.Instance.buildingLimit)
        {
            buildingsText.color = Color.red;
        }
        else if (GameManager.Instance.currentBuildings >= GameManager.Instance.buildingLimit - 1)
        {
            buildingsText.color = Color.yellow;
        }
        else
        {
            buildingsText.color = Color.white;
        }

        // Actualizar visibilidad de botones
        UpdateButtonVisibility();
    }

    void UpdateButtonVisibility()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance es null en UpdateButtonVisibility");
            return;
        }

        Debug.Log($"ResourceUI: Actualizando botones para {GameManager.Instance.currentFaction}");

        // Mostrar/ocultar botones según la facción
        if (GameManager.Instance.currentFaction == GameManager.PlayerFaction.Mana)
        {
            // Mostrar botones de Maná, ocultar los de Corrupción
            if (buildSanctuaryButton != null)
            {
                buildSanctuaryButton.gameObject.SetActive(true);
                Debug.Log("✅ Botón Santuario ACTIVADO");
            }
            if (buildCorruptorButton != null)
            {
                buildCorruptorButton.gameObject.SetActive(false);
                Debug.Log("❌ Botón Corruptor OCULTADO");
            }
            if (spawnMageButton != null)
            {
                spawnMageButton.gameObject.SetActive(true);
                Debug.Log("✅ Botón Mago ACTIVADO");
            }
            if (spawnSlaveButton != null)
            {
                spawnSlaveButton.gameObject.SetActive(false);
                Debug.Log("❌ Botón Esclavo OCULTADO");
            }
        }
        else if (GameManager.Instance.currentFaction == GameManager.PlayerFaction.Corruption)
        {
            // Mostrar botones de Corrupción, ocultar los de Maná
            if (buildSanctuaryButton != null)
            {
                buildSanctuaryButton.gameObject.SetActive(false);
                Debug.Log("❌ Botón Santuario OCULTADO");
            }
            if (buildCorruptorButton != null)
            {
                buildCorruptorButton.gameObject.SetActive(true);
                Debug.Log("✅ Botón Corruptor ACTIVADO");
            }
            if (spawnMageButton != null)
            {
                spawnMageButton.gameObject.SetActive(false);
                Debug.Log("❌ Botón Mago OCULTADO");
            }
            if (spawnSlaveButton != null)
            {
                spawnSlaveButton.gameObject.SetActive(true);
                Debug.Log("✅ Botón Esclavo ACTIVADO");
            }
        }
        else
        {
            Debug.LogWarning("ResourceUI: Facción no reconocida");
        }
    }

    // Método público para forzar actualización desde otros scripts
    public void ForceUpdateUI()
    {
        UpdateButtonVisibility();
        UpdateResourceUI();
    }
}