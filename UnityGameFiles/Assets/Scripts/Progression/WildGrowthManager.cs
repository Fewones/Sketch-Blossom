using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class WildGrowthManager : MonoBehaviour
{
    [Header("References")]
    public DrawingCanvas drawingCanvas;
    public TextMeshProUGUI plantNameText;
    public TextMeshProUGUI currentStatsText;
    public TextMeshProUGUI previewStatsText;
    public TextMeshProUGUI qualityText;
    public Button confirmButton;
    public Button cancelButton;

    [Header("Scene")]
    public string worldMapSceneName = "WorldMapScene";

    private PlantData targetPlant;
    private float currentMultiplier = 1.3f; // minimum

    private void Start()
    {
        if (PlantInventory.Instance == null)
        {
            Debug.LogError("WildGrowthManager: No PlantInventory in scene!");
            return;
        }

        // Try to use selected plant, else fallback to first in list
        targetPlant = PlantInventory.Instance.selectedPlantForUpgrade;
        if (targetPlant == null && PlantInventory.Instance.plants.Count > 0)
        {
            Debug.LogWarning("WildGrowthManager: No selected plant, using first in inventory for now.");
            targetPlant = PlantInventory.Instance.plants[0];
        }

        if (targetPlant == null)
        {
            Debug.LogError("WildGrowthManager: No plant available to upgrade!");
            return;
        }

        SetupUI();

        if (confirmButton != null)
            confirmButton.interactable = false; // only after drawing

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmUpgrade);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancel);
    }

    private void OnDestroy()
    {
        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(OnConfirmUpgrade);

        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(OnCancel);
    }

    private void SetupUI()
    {
        if (plantNameText != null)
            plantNameText.text = targetPlant.name;

        if (currentStatsText != null)
            currentStatsText.text =
                $"HP:  {targetPlant.hp}\n" +
                $"ATK: {targetPlant.attack}\n" +
                $"DEF: {targetPlant.defense}";

        if (previewStatsText != null)
            previewStatsText.text = "";

        if (qualityText != null)
            qualityText.text = "Draw to power up!";
    }

    private void Update()
    {
        UpdateQualityAndPreview();
    }

    private void UpdateQualityAndPreview()
    {
        if (drawingCanvas == null || drawingCanvas.currentStrokeCount == 0)
        {
            if (confirmButton != null)
                confirmButton.interactable = false;
            return;
        }

        float qualityScore = AnalyzeQualityFromCanvas();
        currentMultiplier = MapQualityToMultiplier(qualityScore);

        if (qualityText != null)
            qualityText.text = $"Quality: {qualityScore:F1} → x{currentMultiplier:F2}";

        int newHp  = Mathf.RoundToInt(targetPlant.hp * currentMultiplier);
        int newAtk = Mathf.RoundToInt(targetPlant.attack * currentMultiplier);
        int newDef = Mathf.RoundToInt(targetPlant.defense * currentMultiplier);

        if (previewStatsText != null)
        {
            previewStatsText.text =
                $"HP:  {targetPlant.hp} → {newHp}\n" +
                $"ATK: {targetPlant.attack} → {newAtk}\n" +
                $"DEF: {targetPlant.defense} → {newDef}";
        }

        if (confirmButton != null)
            confirmButton.interactable = true;
    }

    private float AnalyzeQualityFromCanvas()
    {
        int strokes = drawingCanvas.currentStrokeCount;
        float totalLength = 0f;
        int totalPoints = 0;

        foreach (var stroke in drawingCanvas.allStrokes)
        {
            if (stroke == null) continue;

            Vector3[] positions = new Vector3[stroke.positionCount];
            stroke.GetPositions(positions);

            for (int i = 1; i < positions.Length; i++)
                totalLength += Vector3.Distance(positions[i - 1], positions[i]);

            totalPoints += positions.Length;
        }

        float q = 0f;
        q += strokes * 1.0f;
        q += totalLength * 0.5f;
        q += totalPoints * 0.1f;

        return q;
    }

    private float MapQualityToMultiplier(float qualityScore)
    {
        float minQ = 10f;
        float maxQ = 80f;
        float t = Mathf.InverseLerp(minQ, maxQ, qualityScore); // 0..1

        float minM = 1.3f;
        float maxM = 1.8f;

        return Mathf.Lerp(minM, maxM, t);
    }

    public void OnConfirmUpgrade()
    {
        int newHp  = Mathf.RoundToInt(targetPlant.hp * currentMultiplier);
        int newAtk = Mathf.RoundToInt(targetPlant.attack * currentMultiplier);
        int newDef = Mathf.RoundToInt(targetPlant.defense * currentMultiplier);

        targetPlant.hp      = newHp;
        targetPlant.attack  = newAtk;
        targetPlant.defense = newDef;

        Debug.Log($"Wild Growth applied: x{currentMultiplier:F2} to {targetPlant.name}");

        SceneManager.LoadScene(worldMapSceneName);
    }

    public void OnCancel()
    {
        SceneManager.LoadScene(worldMapSceneName);
    }
}

