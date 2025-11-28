using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    public Button manaFactionButton;
    public Button corruptionFactionButton;
    public TextMeshProUGUI descriptionText;
    private GameManager.PlayerFaction selectedFaction;

    public void Start()
    {
        // Configurar botones
        manaFactionButton.onClick.AddListener(() => ChooseFaction(GameManager.PlayerFaction.Mana));
        corruptionFactionButton.onClick.AddListener(() => ChooseFaction(GameManager.PlayerFaction.Corruption));

        // Actualizar descripción inicial
        UpdateDescription();
    }

    void ChooseFaction(GameManager.PlayerFaction faction)
    {
        // Guardar la facción seleccionada
        selectedFaction = faction;

        Debug.Log($"Facción seleccionada temporalmente: {faction}");

        // Efecto visual de feedback
        if (faction == GameManager.PlayerFaction.Mana)
        {
            manaFactionButton.image.color = Color.cyan;
            corruptionFactionButton.image.color = Color.white; // Resetear el otro
        }
        else
        {
            corruptionFactionButton.image.color = Color.magenta;
            manaFactionButton.image.color = Color.white; // Resetear el otro
        }

        // Elegir facción después de breve delay
        Invoke("ExecuteFactionChoice", 0.5f);
    }

    public void ChooseMana()
    {
        ChooseFaction(GameManager.PlayerFaction.Mana);
    }

    public void ChooseCorruption()
    {
        ChooseFaction(GameManager.PlayerFaction.Corruption);
    }


    void ExecuteFactionChoice()
    {
        Debug.Log($"=== EJECUTANDO ELECCIÓN DE FACCIÓN ===");
        Debug.Log($"Facción seleccionada: {selectedFaction}");
        Debug.Log($"GameManager.Instance: {GameManager.Instance}");

        // Llamar al GameManager para que procese la elección
        if (GameManager.Instance != null)
        {
            Debug.Log("Llamando a GameManager.Instance.ChooseFaction...");
            GameManager.Instance.ChooseFaction(selectedFaction);
        }
        else
        {
            Debug.LogError("❌ GameManager.Instance es null!");

            // Buscar cualquier GameManager en la escena
            GameManager manager = FindObjectOfType<GameManager>();
            if (manager != null)
            {
                Debug.Log("Se encontró un GameManager en la escena, asignando Instance...");
                GameManager.Instance = manager;
                manager.ChooseFaction(selectedFaction);
            }
        }
    }

    public void UpdateDescription()
    {
        string manaDesc = "ALIANZA DE LA MAGIA\n\nPreserva y expande el flujo de maná\nCrea santuarios de protección\nPurifica la corrupción";
        string corruptionDesc = "LEGIÓN DE LA CORRUPCIÓN\n\nDomina el mundo con corrupción\nExpande tu influencia oscura\nConsume la energía mágica";

        // Mostrar ambas descripciones o cambiar dinámicamente
        descriptionText.text = "Elige tu bando:\n\n" + manaDesc + "\n\n" + corruptionDesc;
    }

    // Métodos para hover (opcionales)
    public void OnManaButtonHover()
    {
        descriptionText.text = "ALIANZA DE LA MAGIA\n\nPreserva y expande el flujo de maná\nCrea santuarios de protección\nPurifica la corrupción\n\nRecursos: Maná";
    }

    public void OnCorruptionButtonHover()
    {
        descriptionText.text = "LEGIÓN DE LA CORRUPCIÓN\n\nDomina el mundo con corrupción\nExpande tu influencia oscura\nConsume la energía mágica\n\nRecursos: Corrupción";
    }

    public void OnButtonHoverExit()
    {
        UpdateDescription();
    }


}