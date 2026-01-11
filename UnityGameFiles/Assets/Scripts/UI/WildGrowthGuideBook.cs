using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Guide book for the Wild Growth scene.
/// Shows 3 pages explaining basics, scoring and colors.
/// </summary>
public class WildGrowthGuideBook : MonoBehaviour
{
    [Header("UI References")]
    public GameObject bookPanel;
    public Button closeBookButton;
    public Button nextPageButton;
    public Button previousPageButton;

    [Header("Page Content UI")]
    public TextMeshProUGUI pageTitle;
    public TextMeshProUGUI pageDescription;
    public TextMeshProUGUI pageNumberText;
    public Image guideImage;      // optional, can stay null

    [System.Serializable]
    public class GuidePage
    {
        public string title;
        [TextArea(3, 10)]
        public string description;
        public Sprite image;

        public GuidePage(string title, string description, Sprite image = null)
        {
            this.title = title;
            this.description = description;
            this.image = image;
        }
    }

    [SerializeField]
    private List<GuidePage> pages = new List<GuidePage>();

    private int currentPageIndex = 0;

    private void Awake()
    {
        // Set up button callbacks (only uses THIS script, nothing global)
        if (closeBookButton != null)
            closeBookButton.onClick.AddListener(CloseBook);

        if (nextPageButton != null)
            nextPageButton.onClick.AddListener(NextPage);

        if (previousPageButton != null)
            previousPageButton.onClick.AddListener(PreviousPage);

        // If pages not set in Inspector, fill with our defaults
        if (pages == null || pages.Count == 0)
        {
            PopulateDefaultPages();
        }

        // Start with the panel closed
        if (bookPanel != null)
            bookPanel.SetActive(false);

        currentPageIndex = 0;
        UpdatePage();
    }

    /// <summary>
    /// Called by the GUIDE button.
    /// </summary>
    public void OpenBook()
    {
        if (bookPanel != null)
            bookPanel.SetActive(true);

        currentPageIndex = Mathf.Clamp(0, 0, pages.Count - 1);
        UpdatePage();
    }

    private void CloseBook()
    {
        if (bookPanel != null)
            bookPanel.SetActive(false);
    }

    private void NextPage()
    {
        if (pages == null || pages.Count == 0) return;

        if (currentPageIndex < pages.Count - 1)
        {
            currentPageIndex++;
            UpdatePage();
        }
    }

    private void PreviousPage()
    {
        if (pages == null || pages.Count == 0) return;

        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePage();
        }
    }

    /// <summary>
    /// Updates the UI with the current page data.
    /// </summary>
    private void UpdatePage()
    {
        if (pages == null || pages.Count == 0)
            return;

        currentPageIndex = Mathf.Clamp(currentPageIndex, 0, pages.Count - 1);
        GuidePage page = pages[currentPageIndex];

        if (pageTitle != null)
            pageTitle.text = page.title;

        if (pageDescription != null)
            pageDescription.text = page.description;

        if (guideImage != null)
        {
            guideImage.sprite = page.image;
            guideImage.gameObject.SetActive(page.image != null);
        }

        if (pageNumberText != null)
            pageNumberText.text = $"{currentPageIndex + 1} / {pages.Count}";

        if (previousPageButton != null)
            previousPageButton.interactable = currentPageIndex > 0;

        if (nextPageButton != null)
            nextPageButton.interactable = currentPageIndex < pages.Count - 1;
    }

    /// <summary>
/// Fills the guide with the Wild Growth specific pages.
/// Text is kept in sync with WildGrowthSceneManager:
/// - One stroke only
/// - Geometric quality (length + coverage) controls base multiplier
/// - Color biases which stat grows more (HP / ATK / DEF)
/// </summary>
private void PopulateDefaultPages()
    {
        pages = new List<GuidePage>
        {
            new GuidePage(
                "Wild Growth – Basics",
                "What is Wild Growth?\n\n" +
                "• You evolve the plant that just fought in battle.\n" +
                "• You see its current art, then draw exactly ONE new stroke on top.\n" +
                "• That stroke is merged into the plant and saved as the new card art.\n" +
                "• The drawing also increases the plant's HP, ATK and DEF permanently.\n\n" +
                "The better the stroke (size and reach), the stronger the upgrade."
            ),

            new GuidePage(
                "How your stroke is scored",
                "Geometric scoring (no AI, no CLIP yet):\n\n" +
                "<b>1. Length</b>\n" +
                "A longer stroke is worth more than just a tiny dot.\n" +
                "• Very short → almost no bonus\n" +
                "• Medium/long → good quality\n" +
                "• Extremely long → capped at a maximum value\n\n" +
                "<b>2. Coverage</b>\n" +
                "We look at how much of the Wild Growth box your stroke spans.\n" +
                "• Tiny scribble in one corner → low score\n" +
                "• Stroke that covers a visible chunk of the box → higher score\n" +
                "• Very huge strokes are capped at a maximum benefit\n\n" +
                "These two (length + coverage) are combined into one base multiplier\n" +
                "between 1.3x (minimum) and 1.8x (maximum)."
            ),

            new GuidePage(
                "Colors and stat focus",
                "Color changes which stat grows the most:\n\n" +
                "<b>Red stroke</b>\n" +
                "• Focuses on <b>Attack</b>\n" +
                "• ATK gets a bit more growth than HP and DEF\n\n" +
                "<b>Green stroke</b>\n" +
                "• Focuses on <b>HP</b>\n" +
                "• HP gets a bit more growth than ATK and DEF\n\n" +
                "<b>Blue stroke</b>\n" +
                "• Focuses on <b>Defense</b>\n" +
                "• DEF gets a bit more growth than HP and ATK\n\n" +
                "The base power of the upgrade still comes from the stroke's length\n" +
                "and coverage. Color only decides which stat gets the biggest share."
            ),

            new GuidePage(
                "Tips for good Wild Growth",
                "Some practical tips:\n\n" +
                "• Draw across the plant, not just in an empty corner.\n" +
                "• Try to use a single clear gesture instead of many tiny wiggles.\n" +
                "• If you want a tanky plant, draw in <b>green</b>.\n" +
                "• If you want a glass cannon, draw in <b>red</b>.\n" +
                "• If you want a sturdy defender, draw in <b>blue</b>.\n\n" +
                "Remember: You only get ONE stroke per Wild Growth, so make it count."
            )
        };
    }
}
