using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SketchBlossom.Progression;

/// <summary>
/// Debug UI to display player inventory information
/// Attach this to a Canvas with TextMeshProUGUI to see inventory status in real-time
/// </summary>
public class InventoryDebugUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private bool showInUI = true;
    [SerializeField] private float updateInterval = 0.5f;

    private float lastUpdateTime;

    private void Update()
    {
        if (Time.time - lastUpdateTime < updateInterval)
            return;

        lastUpdateTime = Time.time;
        UpdateDebugInfo();
    }

    private void UpdateDebugInfo()
    {
        if (PlayerInventory.Instance == null)
        {
            if (showInUI && debugText != null)
            {
                debugText.text = "PlayerInventory: NULL (not created yet)";
            }
            Debug.LogWarning("[Inventory Debug] PlayerInventory.Instance is NULL!");
            return;
        }

        var plants = PlayerInventory.Instance.GetAllPlants();
        var selected = PlayerInventory.Instance.GetSelectedPlant();

        string info = $"=== INVENTORY DEBUG ===\n";
        info += $"Total Plants: {plants.Count}\n";
        info += $"Selected: {(selected != null ? selected.plantName : "None")}\n\n";

        if (plants.Count > 0)
        {
            info += "Plants in Inventory:\n";
            for (int i = 0; i < plants.Count; i++)
            {
                var plant = plants[i];
                info += $"{i + 1}. {plant.plantName} (Lvl {plant.level})\n";
                info += $"   HP: {plant.currentHealth}/{plant.maxHealth}\n";
                info += $"   ATK: {plant.attack} | DEF: {plant.defense}\n";
                info += $"   Battles: {plant.battlesWon}\n";
                info += $"   ID: {plant.plantId.Substring(0, 8)}...\n";
            }
        }
        else
        {
            info += "No plants in inventory yet!\n";
        }

        if (showInUI && debugText != null)
        {
            debugText.text = info;
        }

        // Also log to console periodically (less frequently)
        if (Mathf.FloorToInt(Time.time) % 5 == 0)
        {
            Debug.Log($"[Inventory Debug] {info}");
        }
    }

    [ContextMenu("Force Update")]
    public void ForceUpdate()
    {
        UpdateDebugInfo();
    }

    [ContextMenu("Clear Inventory (TEST ONLY)")]
    public void ClearInventoryForTesting()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.ClearInventory();
            Debug.LogWarning("[Inventory Debug] Inventory cleared!");
        }
    }
}
