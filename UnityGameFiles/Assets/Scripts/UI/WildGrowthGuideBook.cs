using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Guide book for the Wild Growth scene.
/// Shows 3 pages explaining basics, scoring and colors.
/// Completely independent from PlantGuideBook.
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
    /// </summary>
    private void PopulateDefaultPages()
    {
        pages = new List<GuidePage>
        {
            new GuidePage(
                "Wild Growth – Basics",
                "Wild Growth\n" +
                "You are evolving your existing plant.\n" +
                "Draw new leaves, thorns, petals or armor on top of it.\n" +
                "A better drawing → a stronger upgrade to HP, ATK and DEF."
            ),

            new GuidePage(
                "What influences the score",
                "Scoring\n" +
                "• More strokes (up to a limit)\n" +
                "• Longer lines (not just dots)\n" +
                "• Drawing that covers more of the box\n" +
                "All of these increase the Wild Growth multiplier."
            ),

            new GuidePage(
                "Colors",
                "Colors\n" +
                "• Red = fiery / aggressive look\n" +
                "• Green = natural / defensive look\n" +
                "• Blue = watery / flexible look\n" +
                "Right now, colors are cosmetic – the shape & size of your drawing control the score."
            )
        };
    }
}
