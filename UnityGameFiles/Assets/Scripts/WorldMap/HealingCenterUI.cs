using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using SketchBlossom.Progression;

/// <summary>
/// UI popup for the Healing Center on the world map.
/// Shows the player's plants, their current HP, and lets them heal one plant per charge.
/// Healing charges are earned by winning battles (1 charge per victory).
/// </summary>
public class HealingCenterUI : MonoBehaviour
{
    [Header("UI Panel")]
    [SerializeField] private GameObject popupPanel;

    [Header("Header")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI chargesText;

    [Header("Plant List")]
    [SerializeField] private RectTransform plantListContent;
    [SerializeField] private GameObject plantEntryPrefab;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;

    [Header("Colors")]
    [SerializeField] private Color healthyColor = new Color(0.3f, 0.9f, 0.3f);
    [SerializeField] private Color damagedColor = new Color(0.9f, 0.9f, 0.3f);
    [SerializeField] private Color criticalColor = new Color(0.9f, 0.3f, 0.3f);
    [SerializeField] private Color healedFlashColor = new Color(0.5f, 1f, 0.5f, 0.8f);

    private List<GameObject> spawnedEntries = new List<GameObject>();
    private bool isVisible = false;

    private void Awake()
    {
        // Ensure an EventSystem exists so UI buttons can receive clicks
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        if (popupPanel == null)
        {
            BuildUI();
        }

        // Always start hidden - force hide immediately
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
        isVisible = false;

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }
    }

    private void Start()
    {
        // Double-ensure hidden on start (handles AddComponent during another script's Start)
        if (popupPanel != null && !isVisible)
        {
            popupPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Allow Escape key to close the window
        if (isVisible && Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
        }
    }

    /// <summary>
    /// Opens the Healing Center UI and populates the plant list
    /// </summary>
    public void Show()
    {
        if (popupPanel == null)
        {
            BuildUI();
        }

        popupPanel.SetActive(true);
        isVisible = true;
        RefreshPlantList();

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.SetMovementEnabled(false);
        }
    }

    /// <summary>
    /// Closes the Healing Center UI
    /// </summary>
    public void Hide()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
        isVisible = false;

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.SetMovementEnabled(true);
        }
    }

    private void RefreshPlantList()
    {
        // Clear old entries
        foreach (var entry in spawnedEntries)
        {
            if (entry != null) Destroy(entry);
        }
        spawnedEntries.Clear();

        // Update charges display
        int charges = HealingCenter.HealCharges;
        if (chargesText != null)
        {
            chargesText.text = $"Healing Charges: {charges}";
            chargesText.color = charges > 0 ? healthyColor : criticalColor;
        }

        // Get all plants from inventory
        if (PlayerInventory.Instance == null || PlayerInventory.Instance.IsEmpty())
        {
            SpawnMessageEntry("No plants in your inventory.");
            return;
        }

        List<PlantInventoryEntry> plants = PlayerInventory.Instance.GetAllPlants();

        foreach (var plant in plants)
        {
            SpawnPlantEntry(plant, charges > 0);
        }
    }

    private void SpawnPlantEntry(PlantInventoryEntry plant, bool canHeal)
    {
        GameObject entry = new GameObject($"PlantEntry_{plant.plantName}");
        entry.transform.SetParent(plantListContent, false);
        spawnedEntries.Add(entry);

        // Layout
        RectTransform entryRect = entry.AddComponent<RectTransform>();
        entryRect.sizeDelta = new Vector2(0, 70);

        HorizontalLayoutGroup hlg = entry.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.padding = new RectOffset(10, 10, 5, 5);
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;

        // Background
        Image bg = entry.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        // Plant card image
        GameObject cardObj = new GameObject("PlantCard");
        cardObj.transform.SetParent(entry.transform, false);

        Image cardBg = cardObj.AddComponent<Image>();
        cardBg.color = GetElementColor(plant.elementType);

        LayoutElement cardLayout = cardObj.AddComponent<LayoutElement>();
        cardLayout.preferredWidth = 60;
        cardLayout.preferredHeight = 60;

        // Plant drawing texture on top of card background
        GameObject imgObj = new GameObject("PlantImage");
        imgObj.transform.SetParent(cardObj.transform, false);

        RawImage plantImg = imgObj.AddComponent<RawImage>();
        Texture2D texture = plant.GetDrawingTexture();
        if (texture != null)
        {
            plantImg.texture = texture;
            plantImg.color = Color.white;
        }
        else
        {
            plantImg.color = plant.plantColor;
        }

        RectTransform imgRect = plantImg.GetComponent<RectTransform>();
        imgRect.anchorMin = new Vector2(0.1f, 0.1f);
        imgRect.anchorMax = new Vector2(0.9f, 0.9f);
        imgRect.offsetMin = Vector2.zero;
        imgRect.offsetMax = Vector2.zero;

        // Plant info text
        GameObject infoObj = new GameObject("Info");
        infoObj.transform.SetParent(entry.transform, false);

        TextMeshProUGUI infoText = infoObj.AddComponent<TextMeshProUGUI>();
        infoText.fontSize = 16;
        infoText.color = Color.white;

        float hpPercent = plant.maxHealth > 0 ? (float)plant.currentHealth / plant.maxHealth : 0f;
        Color hpColor = hpPercent > 0.5f ? healthyColor : hpPercent > 0.25f ? damagedColor : criticalColor;
        string hpColorHex = ColorUtility.ToHtmlStringRGB(hpColor);

        bool isDamaged = plant.currentHealth < plant.maxHealth;

        infoText.text = $"<b>{plant.plantName}</b>  Lv.{plant.level}  [{plant.elementType}]\n" +
                        $"HP: <color=#{hpColorHex}>{plant.currentHealth}/{plant.maxHealth}</color>" +
                        (isDamaged ? "" : "  <color=#88FF88>(Full HP)</color>");

        LayoutElement infoLayout = infoObj.AddComponent<LayoutElement>();
        infoLayout.flexibleWidth = 1;
        infoLayout.preferredWidth = 300;

        // Heal button
        if (isDamaged)
        {
            GameObject btnObj = new GameObject("HealButton");
            btnObj.transform.SetParent(entry.transform, false);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = canHeal ? new Color(0.2f, 0.6f, 0.3f) : new Color(0.3f, 0.3f, 0.3f);

            Button healBtn = btnObj.AddComponent<Button>();
            healBtn.targetGraphic = btnImage;
            healBtn.interactable = canHeal;

            LayoutElement btnLayout = btnObj.AddComponent<LayoutElement>();
            btnLayout.preferredWidth = 80;
            btnLayout.preferredHeight = 40;

            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btnObj.transform, false);

            TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnText.text = canHeal ? "Heal" : "No Charges";
            btnText.fontSize = canHeal ? 16 : 11;
            btnText.color = canHeal ? Color.white : Color.gray;
            btnText.alignment = TextAlignmentOptions.Center;

            RectTransform btnTextRect = btnText.GetComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;

            // Capture plant ID for the button closure
            string plantId = plant.plantId;
            healBtn.onClick.AddListener(() => OnHealPlant(plantId));
        }
    }

    private void SpawnMessageEntry(string message)
    {
        GameObject entry = new GameObject("MessageEntry");
        entry.transform.SetParent(plantListContent, false);
        spawnedEntries.Add(entry);

        RectTransform entryRect = entry.AddComponent<RectTransform>();
        entryRect.sizeDelta = new Vector2(0, 50);

        TextMeshProUGUI text = entry.AddComponent<TextMeshProUGUI>();
        text.text = message;
        text.fontSize = 18;
        text.color = Color.gray;
        text.alignment = TextAlignmentOptions.Center;
    }

    private void OnHealPlant(string plantId)
    {
        if (!HealingCenter.ConsumeHealCharge())
        {
            Debug.Log("No healing charges available!");
            return;
        }

        PlayerInventory.Instance.HealPlant(plantId);

        PlantInventoryEntry plant = PlayerInventory.Instance.GetPlantById(plantId);
        if (plant != null)
        {
            Debug.Log($"Healed {plant.plantName} to full HP ({plant.currentHealth}/{plant.maxHealth})");
        }

        // Refresh the list to show updated HP and charges
        RefreshPlantList();
    }

    /// <summary>
    /// Procedurally builds the Healing Center UI if not already set up in the scene
    /// </summary>
    private void BuildUI()
    {
        // Root panel - start inactive so the Canvas never renders a visible frame
        popupPanel = new GameObject("HealingCenterPanel");
        popupPanel.SetActive(false);
        popupPanel.transform.SetParent(transform, false);

        Canvas canvas = popupPanel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        popupPanel.AddComponent<CanvasScaler>();
        popupPanel.AddComponent<GraphicRaycaster>();

        // Dimmed background overlay
        GameObject overlay = new GameObject("Overlay");
        overlay.transform.SetParent(popupPanel.transform, false);

        Image overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.6f);

        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        // Clicking the overlay (outside content panel) closes the UI
        Button overlayBtn = overlay.AddComponent<Button>();
        overlayBtn.onClick.AddListener(Hide);

        // Main content panel
        GameObject contentPanel = new GameObject("ContentPanel");
        contentPanel.transform.SetParent(popupPanel.transform, false);

        Image contentBg = contentPanel.AddComponent<Image>();
        contentBg.color = new Color(0.1f, 0.12f, 0.1f, 0.95f);

        RectTransform contentRect = contentPanel.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.15f, 0.1f);
        contentRect.anchorMax = new Vector2(0.85f, 0.9f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup vlg = contentPanel.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10;
        vlg.padding = new RectOffset(20, 20, 20, 20);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(contentPanel.transform, false);

        titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Healing Center";
        titleText.fontSize = 28;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(0.5f, 1f, 0.6f);
        titleText.alignment = TextAlignmentOptions.Center;

        LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 40;

        // Charges text
        GameObject chargesObj = new GameObject("Charges");
        chargesObj.transform.SetParent(contentPanel.transform, false);

        chargesText = chargesObj.AddComponent<TextMeshProUGUI>();
        chargesText.text = "Healing Charges: 0";
        chargesText.fontSize = 18;
        chargesText.color = Color.white;
        chargesText.alignment = TextAlignmentOptions.Center;

        LayoutElement chargesLayout = chargesObj.AddComponent<LayoutElement>();
        chargesLayout.preferredHeight = 30;

        // Description
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(contentPanel.transform, false);

        TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = "Select a plant to restore it to full health.\nYou earn 1 healing charge for each battle won.";
        descText.fontSize = 14;
        descText.color = new Color(0.7f, 0.7f, 0.7f);
        descText.alignment = TextAlignmentOptions.Center;

        LayoutElement descLayout = descObj.AddComponent<LayoutElement>();
        descLayout.preferredHeight = 40;

        // Scroll view for plant list
        GameObject scrollObj = new GameObject("ScrollView");
        scrollObj.transform.SetParent(contentPanel.transform, false);

        ScrollRect scrollRect = scrollObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        Image scrollBg = scrollObj.AddComponent<Image>();
        scrollBg.color = new Color(0.08f, 0.08f, 0.08f, 0.5f);

        LayoutElement scrollLayout = scrollObj.AddComponent<LayoutElement>();
        scrollLayout.flexibleHeight = 1;

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform, false);

        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;

        viewport.AddComponent<RectMask2D>();

        // Content container - add VLG first so RectTransform is auto-created
        GameObject contentContainer = new GameObject("Content");
        contentContainer.transform.SetParent(viewport.transform, false);

        VerticalLayoutGroup contentVlg = contentContainer.AddComponent<VerticalLayoutGroup>();

        plantListContent = contentContainer.GetComponent<RectTransform>();
        plantListContent.anchorMin = new Vector2(0, 1);
        plantListContent.anchorMax = new Vector2(1, 1);
        plantListContent.pivot = new Vector2(0.5f, 1);
        plantListContent.offsetMin = Vector2.zero;
        plantListContent.offsetMax = Vector2.zero;
        contentVlg.spacing = 5;
        contentVlg.padding = new RectOffset(5, 5, 5, 5);
        contentVlg.childForceExpandWidth = true;
        contentVlg.childForceExpandHeight = false;

        ContentSizeFitter csf = contentContainer.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRect;
        scrollRect.content = plantListContent;

        // Close button
        GameObject closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(contentPanel.transform, false);

        Image closeBtnImg = closeBtnObj.AddComponent<Image>();
        closeBtnImg.color = new Color(0.4f, 0.15f, 0.15f);

        closeButton = closeBtnObj.AddComponent<Button>();
        closeButton.targetGraphic = closeBtnImg;
        closeButton.onClick.AddListener(Hide);

        LayoutElement closeBtnLayout = closeBtnObj.AddComponent<LayoutElement>();
        closeBtnLayout.preferredHeight = 40;

        GameObject closeBtnTextObj = new GameObject("Text");
        closeBtnTextObj.transform.SetParent(closeBtnObj.transform, false);

        TextMeshProUGUI closeBtnText = closeBtnTextObj.AddComponent<TextMeshProUGUI>();
        closeBtnText.text = "Close";
        closeBtnText.fontSize = 20;
        closeBtnText.color = Color.white;
        closeBtnText.alignment = TextAlignmentOptions.Center;

        RectTransform closeBtnTextRect = closeBtnText.GetComponent<RectTransform>();
        closeBtnTextRect.anchorMin = Vector2.zero;
        closeBtnTextRect.anchorMax = Vector2.one;
        closeBtnTextRect.offsetMin = Vector2.zero;
        closeBtnTextRect.offsetMax = Vector2.zero;
    }

    private Color GetElementColor(PlantRecognitionSystem.ElementType element)
    {
        switch (element)
        {
            case PlantRecognitionSystem.ElementType.Fire:
                return new Color(0.6f, 0.2f, 0.15f, 0.9f);
            case PlantRecognitionSystem.ElementType.Water:
                return new Color(0.15f, 0.3f, 0.6f, 0.9f);
            case PlantRecognitionSystem.ElementType.Grass:
                return new Color(0.2f, 0.5f, 0.2f, 0.9f);
            default:
                return new Color(0.3f, 0.3f, 0.3f, 0.9f);
        }
    }
}
