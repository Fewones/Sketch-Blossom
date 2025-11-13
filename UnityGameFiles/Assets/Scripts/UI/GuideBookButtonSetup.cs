using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Ensures the Guide Book button is always visible and properly styled
/// Auto-configures the button on scene start
/// </summary>
[RequireComponent(typeof(Button))]
public class GuideBookButtonSetup : MonoBehaviour
{
    [Header("Button Styling")]
    public Color buttonColor = new Color(0.2f, 0.6f, 0.9f, 1f); // Nice blue
    public Color hoverColor = new Color(0.3f, 0.7f, 1f, 1f);
    public Color pressColor = new Color(0.1f, 0.5f, 0.8f, 1f);

    [Header("Icon Settings")]
    public string buttonText = "📖 GUIDE";
    public int fontSize = 24;
    public bool boldText = true;

    private Button button;
    private Image buttonImage;
    private TextMeshProUGUI buttonText_TMP;

    private void Start()
    {
        SetupButton();
    }

    private void SetupButton()
    {
        // Get button component
        button = GetComponent<Button>();
        if (button == null) return;

        // Get or add Image component
        buttonImage = GetComponent<Image>();
        if (buttonImage == null)
        {
            buttonImage = gameObject.AddComponent<Image>();
        }

        // Set button colors
        ColorBlock colors = button.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = hoverColor;
        colors.pressedColor = pressColor;
        colors.selectedColor = hoverColor;
        button.colors = colors;

        // Ensure button has a background color
        buttonImage.color = buttonColor;

        // Setup text
        buttonText_TMP = GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText_TMP == null)
        {
            // Create text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(transform, false);
            buttonText_TMP = textObj.AddComponent<TextMeshProUGUI>();

            // Position text to fill button
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
        }

        // Style the text
        buttonText_TMP.text = buttonText;
        buttonText_TMP.fontSize = fontSize;
        buttonText_TMP.fontStyle = boldText ? FontStyles.Bold : FontStyles.Normal;
        buttonText_TMP.alignment = TextAlignmentOptions.Center;
        buttonText_TMP.color = Color.white;

        // Make sure button is active and interactable
        gameObject.SetActive(true);
        button.interactable = true;

        // Position button (top-right corner by default)
        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            // If not already positioned, set to top-right
            if (rect.anchorMin == Vector2.zero && rect.anchorMax == Vector2.zero)
            {
                rect.anchorMin = new Vector2(0.85f, 0.92f);
                rect.anchorMax = new Vector2(0.98f, 0.98f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
        }

        Debug.Log($"✅ Guide Book button configured: {gameObject.name}");
    }

    // Called from Inspector or other scripts
    public void EnsureVisible()
    {
        gameObject.SetActive(true);
        if (button != null) button.interactable = true;
    }
}

#if UNITY_EDITOR
// Add this component automatically when button is added to PlantGuideBook
[UnityEditor.InitializeOnLoad]
public class GuideBookButtonAutoAdd
{
    static GuideBookButtonAutoAdd()
    {
        UnityEditor.EditorApplication.update += CheckGuideBookButtons;
    }

    static void CheckGuideBookButtons()
    {
        if (Application.isPlaying) return;

        PlantGuideBook[] guides = Object.FindObjectsOfType<PlantGuideBook>();
        foreach (var guide in guides)
        {
            if (guide.openBookButton != null)
            {
                GuideBookButtonSetup setup = guide.openBookButton.GetComponent<GuideBookButtonSetup>();
                if (setup == null)
                {
                    guide.openBookButton.gameObject.AddComponent<GuideBookButtonSetup>();
                }
            }
        }
    }
}
#endif
