using UnityEngine;
using TMPro;
using UnityEngine.UI;

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

    void Start()
    {
        UpdateButtonVisibility();
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        UpdateResourceUI();
    }

    void UpdateResourceUI()
    {
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
        if (GameManager.Instance == null) return;

        // Mostrar/ocultar botones según la facción
        if (GameManager.Instance.currentFaction == GameManager.PlayerFaction.Mana)
        {
            // Mostrar botones de Maná, ocultar los de Corrupción
            if (buildSanctuaryButton != null) buildSanctuaryButton.gameObject.SetActive(true);
            if (buildCorruptorButton != null) buildCorruptorButton.gameObject.SetActive(false);
            if (spawnMageButton != null) spawnMageButton.gameObject.SetActive(true);
            if (spawnSlaveButton != null) spawnSlaveButton.gameObject.SetActive(false);
        }
        else if (GameManager.Instance.currentFaction == GameManager.PlayerFaction.Corruption)
        {
            // Mostrar botones de Corrupción, ocultar los de Maná
            if (buildSanctuaryButton != null) buildSanctuaryButton.gameObject.SetActive(false);
            if (buildCorruptorButton != null) buildCorruptorButton.gameObject.SetActive(true);
            if (spawnMageButton != null) spawnMageButton.gameObject.SetActive(false);
            if (spawnSlaveButton != null) spawnSlaveButton.gameObject.SetActive(true);
        }
    }
}