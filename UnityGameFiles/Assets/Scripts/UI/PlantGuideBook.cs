using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Interactive guide book that shows players hints on how to draw each plant type
/// Can be opened/closed during the drawing phase
/// </summary>
public class PlantGuideBook : MonoBehaviour
{
    [Header("UI References")]
    public GameObject bookPanel;
    public Button openBookButton;
    public Button closeBookButton;
    public Button nextPageButton;
    public Button previousPageButton;

    [Header("Page Content")]
    public TextMeshProUGUI pageTitle;
    public TextMeshProUGUI pageDescription;
    public Image guideImage;
    public TextMeshProUGUI pageNumberText;

    [Header("Animation")]
    public float transitionSpeed = 5f;
    public bool useSlideAnimation = true;

    private int currentPage = 0;
    private GuidePageData[] pages;
    private bool isBookOpen = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;

    [System.Serializable]
    public class GuidePageData
    {
        public string title;
        [TextArea(3, 6)]
        public string description;
        public Sprite guideSprite;
        public Color pageColor = Color.white;
    }

    private void Start()
    {
        Debug.Log("PlantGuideBook: Starting initialization...");

        InitializePages();
        SetupButtons();

        if (bookPanel != null)
        {
            // Store positions for animation
            RectTransform rectTransform = bookPanel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Open position is on screen (right side)
                openPosition = rectTransform.anchoredPosition;

                // Closed position is off-screen to the right
                closedPosition = new Vector3(openPosition.x + 1000f, openPosition.y, openPosition.z);

                Debug.Log($"Guide Book positions - Open: {openPosition}, Closed: {closedPosition}");
            }

            // Start with book closed
            CloseBook(instant: true);
        }
        else
        {
            Debug.LogError("PlantGuideBook: bookPanel is NULL! Make sure to assign it in Inspector.");
        }

        // Ensure open button is visible
        if (openBookButton != null)
        {
            openBookButton.gameObject.SetActive(true);
            Debug.Log("PlantGuideBook: Open button is active");
        }
        else
        {
            Debug.LogError("PlantGuideBook: openBookButton is NULL! Make sure to assign it in Inspector.");
        }
    }

    private void InitializePages()
    {
        // Create guide pages for each plant type
        pages = new GuidePageData[]
        {
            // Page 1: Welcome & Color Selection
            new GuidePageData
            {
                title = "Welcome to the Plant Guide!",
                description = "Discover 9 unique plants across 3 elements!\n\n" +
                             "<b>🎨 Choose Your Color First:</b>\n" +
                             "• <color=red>RED</color> → Fire Plants 🔥\n" +
                             "• <color=green>GREEN</color> → Grass Plants 🌿\n" +
                             "• <color=blue>BLUE</color> → Water Plants 💧\n\n" +
                             "<b>Then Draw Your Shape!</b>\n" +
                             "Each element has 3 plant types.\n" +
                             "Your drawing shape determines which plant.\n\n" +
                             "→ Use arrows to explore all plants!",
                pageColor = new Color(1f, 0.95f, 0.8f)
            },

            // Pages 2-4: FIRE PLANTS (Red)
            new GuidePageData
            {
                title = "🔥 Fire: Sunflower",
                description = "<b><color=red>Draw with RED color</color></b>\n\n" +
                             "<b>Drawing Hint:</b>\n" +
                             "Think of petals spreading from a center...\n" +
                             "Radial patterns work well.\n" +
                             "Many lines from one point.\n\n" +
                             "<b>Stats:</b> HP: 30 | ATK: 18 | DEF: 8\n" +
                             "High damage, quick attacks\n\n" +
                             "<b>Best For:</b> Aggressive playstyle",
                pageColor = new Color(1f, 0.85f, 0.7f)
            },
            new GuidePageData
            {
                title = "🔥 Fire: Fire Rose",
                description = "<b><color=red>Draw with RED color</color></b>\n\n" +
                             "<b>Drawing Hint:</b>\n" +
                             "Layers upon layers...\n" +
                             "Compact and dense.\n" +
                             "Many overlapping strokes.\n\n" +
                             "<b>Stats:</b> HP: 35 | ATK: 16 | DEF: 10\n" +
                             "Balanced fire type\n\n" +
                             "<b>Best For:</b> All-around fire power",
                pageColor = new Color(1f, 0.8f, 0.8f)
            },
            new GuidePageData
            {
                title = "🔥 Fire: Flame Tulip",
                description = "<b><color=red>Draw with RED color</color></b>\n\n" +
                             "<b>Drawing Hint:</b>\n" +
                             "Reach for the sky...\n" +
                             "Tall and elegant.\n" +
                             "Few simple strokes, vertical.\n\n" +
                             "<b>Stats:</b> HP: 28 | ATK: 20 | DEF: 6\n" +
                             "Highest attack, low defense\n\n" +
                             "<b>Best For:</b> Glass cannon strategy",
                pageColor = new Color(1f, 0.75f, 0.6f)
            },

            // Pages 5-7: GRASS PLANTS (Green)
            new GuidePageData
            {
                title = "🌿 Grass: Cactus",
                description = "<b><color=green>Draw with GREEN color</color></b>\n\n" +
                             "<b>Drawing Hint:</b>\n" +
                             "Stand tall and proud...\n" +
                             "Vertical lines dominate.\n" +
                             "Straight and upright.\n\n" +
                             "<b>Stats:</b> HP: 32 | ATK: 12 | DEF: 14\n" +
                             "Balanced defense\n\n" +
                             "<b>Best For:</b> Defensive tactics",
                pageColor = new Color(0.7f, 1f, 0.7f)
            },
            new GuidePageData
            {
                title = "🌿 Grass: Vine Flower",
                description = "<b><color=green>Draw with GREEN color</color></b>\n\n" +
                             "<b>Drawing Hint:</b>\n" +
                             "Flow like water, but green...\n" +
                             "Curves and waves.\n" +
                             "Organic, flowing movements.\n\n" +
                             "<b>Stats:</b> HP: 35 | ATK: 14 | DEF: 12\n" +
                             "Well-rounded grass type\n\n" +
                             "<b>Best For:</b> Adaptable strategy",
                pageColor = new Color(0.6f, 1f, 0.8f)
            },
            new GuidePageData
            {
                title = "🌿 Grass: Grass Sprout",
                description = "<b><color=green>Draw with GREEN color</color></b>\n\n" +
                             "<b>Drawing Hint:</b>\n" +
                             "Small but mighty...\n" +
                             "Many short strokes.\n" +
                             "Bushy and compact.\n\n" +
                             "<b>Stats:</b> HP: 30 | ATK: 10 | DEF: 16\n" +
                             "Highest grass defense\n\n" +
                             "<b>Best For:</b> Tanking damage",
                pageColor = new Color(0.8f, 1f, 0.7f)
            },

            // Pages 8-10: WATER PLANTS (Blue)
            new GuidePageData
            {
                title = "💧 Water: Water Lily",
                description = "<b><color=blue>Draw with BLUE color</color></b>\n\n" +
                             "<b>Drawing Hint:</b>\n" +
                             "Float peacefully...\n" +
                             "Spread wide, not tall.\n" +
                             "Horizontal strokes.\n\n" +
                             "<b>Stats:</b> HP: 40 | ATK: 10 | DEF: 14\n" +
                             "Highest HP! Has healing!\n\n" +
                             "<b>Best For:</b> Survival strategy",
                pageColor = new Color(0.7f, 0.9f, 1f)
            },
            new GuidePageData
            {
                title = "💧 Water: Coral Bloom",
                description = "<b><color=blue>Draw with BLUE color</color></b>\n\n" +
                             "<b>Drawing Hint:</b>\n" +
                             "Branch like underwater trees...\n" +
                             "Multiple smaller lines.\n" +
                             "Branching patterns.\n\n" +
                             "<b>Stats:</b> HP: 38 | ATK: 12 | DEF: 12\n" +
                             "Balanced water type\n\n" +
                             "<b>Best For:</b> Versatile play",
                pageColor = new Color(0.6f, 0.85f, 1f)
            },
            new GuidePageData
            {
                title = "💧 Water: Bubble Flower",
                description = "<b><color=blue>Draw with BLUE color</color></b>\n\n" +
                             "<b>Drawing Hint:</b>\n" +
                             "Round and bubbly...\n" +
                             "Clusters of circles.\n" +
                             "Compact grouping.\n\n" +
                             "<b>Stats:</b> HP: 36 | ATK: 8 | DEF: 16\n" +
                             "Highest defense! Healing moves!\n\n" +
                             "<b>Best For:</b> Ultimate defense",
                pageColor = new Color(0.8f, 0.9f, 1f)
            },

            // Page 11: Tips
            new GuidePageData
            {
                title = "Master Tips",
                description = "<b>🎨 Color Matters!</b>\n" +
                             "Always select color BEFORE drawing.\n" +
                             "Color determines element type.\n\n" +
                             "<b>✏️ Shape Matters!</b>\n" +
                             "• Radial/circular → Sunflower\n" +
                             "• Dense layers → Fire Rose\n" +
                             "• Tall/simple → Flame Tulip\n" +
                             "• Vertical/straight → Cactus\n" +
                             "• Flowing curves → Vine Flower\n" +
                             "• Short/bushy → Grass Sprout\n" +
                             "• Horizontal/wide → Water Lily\n" +
                             "• Branching → Coral Bloom\n" +
                             "• Clustered circles → Bubble Flower\n\n" +
                             "Experiment and discover! 🌱",
                pageColor = new Color(0.95f, 0.95f, 1f)
            }
        };

        UpdatePageDisplay();
    }

    private void SetupButtons()
    {
        if (openBookButton != null)
        {
            openBookButton.onClick.RemoveAllListeners(); // Clear any existing listeners
            openBookButton.onClick.AddListener(() => OpenBook());
            Debug.Log("PlantGuideBook: Open button listener added");
        }

        if (closeBookButton != null)
        {
            closeBookButton.onClick.RemoveAllListeners();
            closeBookButton.onClick.AddListener(() => CloseBook());
            Debug.Log("PlantGuideBook: Close button listener added");
        }

        if (nextPageButton != null)
        {
            nextPageButton.onClick.RemoveAllListeners();
            nextPageButton.onClick.AddListener(NextPage);
        }

        if (previousPageButton != null)
        {
            previousPageButton.onClick.RemoveAllListeners();
            previousPageButton.onClick.AddListener(PreviousPage);
        }
    }

    public void OpenBook()
    {
        Debug.Log("PlantGuideBook: OpenBook() called");

        if (isBookOpen)
        {
            Debug.Log("PlantGuideBook: Book already open, ignoring");
            return;
        }

        isBookOpen = true;

        if (bookPanel != null)
        {
            bookPanel.SetActive(true);
            Debug.Log("PlantGuideBook: Book panel activated");

            // Bring the guide panel to front (above drawing window)
            bookPanel.transform.SetAsLastSibling();
            Debug.Log("PlantGuideBook: Panel brought to front (z-order)");

            if (useSlideAnimation)
            {
                StartCoroutine(AnimateBookPosition(openPosition));
            }
            else
            {
                RectTransform rect = bookPanel.GetComponent<RectTransform>();
                if (rect != null) rect.anchoredPosition = openPosition;
            }
        }
        else
        {
            Debug.LogError("PlantGuideBook: Cannot open book - bookPanel is NULL!");
        }

        if (openBookButton != null)
        {
            openBookButton.gameObject.SetActive(false);
            Debug.Log("PlantGuideBook: Open button hidden");
        }

        Debug.Log("Plant Guide Book opened successfully");
    }

    public void CloseBook(bool instant = false)
    {
        if (!isBookOpen && !instant) return;

        isBookOpen = false;

        if (bookPanel != null)
        {
            if (useSlideAnimation && !instant)
            {
                StartCoroutine(AnimateBookPosition(closedPosition, () =>
                {
                    bookPanel.SetActive(false);
                }));
            }
            else
            {
                RectTransform rectTransform = bookPanel.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = closedPosition;
                }
                bookPanel.SetActive(false);
            }
        }

        if (openBookButton != null)
        {
            openBookButton.gameObject.SetActive(true);
        }

        Debug.Log("Plant Guide Book closed");
    }

    public void NextPage()
    {
        if (currentPage < pages.Length - 1)
        {
            currentPage++;
            UpdatePageDisplay();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdatePageDisplay();
        }
    }

    private void UpdatePageDisplay()
    {
        if (pages == null || pages.Length == 0 || currentPage >= pages.Length)
            return;

        GuidePageData page = pages[currentPage];

        // Update title
        if (pageTitle != null)
        {
            pageTitle.text = page.title;
        }

        // Update description
        if (pageDescription != null)
        {
            pageDescription.text = page.description;
        }

        // Update page number
        if (pageNumberText != null)
        {
            pageNumberText.text = $"Page {currentPage + 1} / {pages.Length}";
        }

        // Update image (if you add sprites later)
        if (guideImage != null && page.guideSprite != null)
        {
            guideImage.sprite = page.guideSprite;
            guideImage.color = Color.white;
        }
        else if (guideImage != null)
        {
            guideImage.color = new Color(1, 1, 1, 0); // Transparent if no sprite
        }

        // Update background color
        if (bookPanel != null)
        {
            Image panelImage = bookPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = page.pageColor;
            }
        }

        // Update button states
        if (previousPageButton != null)
        {
            previousPageButton.interactable = currentPage > 0;
        }

        if (nextPageButton != null)
        {
            nextPageButton.interactable = currentPage < pages.Length - 1;
        }
    }

    private System.Collections.IEnumerator AnimateBookPosition(Vector3 targetPosition, System.Action onComplete = null)
    {
        if (bookPanel == null) yield break;

        RectTransform rectTransform = bookPanel.GetComponent<RectTransform>();
        if (rectTransform == null) yield break;

        Vector3 startPosition = rectTransform.anchoredPosition;
        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            rectTransform.anchoredPosition = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
        onComplete?.Invoke();
    }

    // Public method to jump to a specific page
    public void GoToPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < pages.Length)
        {
            currentPage = pageIndex;
            UpdatePageDisplay();
        }
    }

    // Keyboard shortcuts (optional)
    private void Update()
    {
        // Press H to toggle guide book (open/close)
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (isBookOpen)
            {
                CloseBook();
            }
            else
            {
                OpenBook();
            }
        }

        // Additional shortcuts when book is open
        if (isBookOpen)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                NextPage();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                PreviousPage();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseBook();
            }
        }
    }
}
