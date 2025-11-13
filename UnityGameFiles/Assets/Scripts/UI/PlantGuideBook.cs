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
            new GuidePageData
            {
                title = "Welcome to the Plant Guide!",
                description = "This guide will help you learn how to draw different plants.\n\n" +
                             "Each plant type has unique characteristics that determine its elemental type:\n\n" +
                             "• <color=#FF6B35>SUNFLOWER</color> → Fire Type 🔥\n" +
                             "• <color=#4ECDC4>CACTUS</color> → Grass Type 🌱\n" +
                             "• <color=#95E1D3>WATER LILY</color> → Water Type 💧\n\n" +
                             "Use the arrow buttons to see drawing tips!",
                pageColor = new Color(1f, 0.95f, 0.8f)
            },
            new GuidePageData
            {
                title = "🔥 Sunflower (Fire Type)",
                description = "<b>How to Draw:</b>\n" +
                             "1. Draw a <b>circle</b> in the center\n" +
                             "2. Add multiple <b>petals</b> radiating outward\n" +
                             "3. Draw 5-8 curved strokes around the center\n\n" +
                             "<b>Key Features:</b>\n" +
                             "• Round, circular shape\n" +
                             "• Multiple radiating strokes\n" +
                             "• Symmetrical petal pattern\n\n" +
                             "<b>Battle Moves:</b>\n" +
                             "• Fireball (circle)\n" +
                             "• Flame Wave (horizontal wave)\n" +
                             "• Burn (zigzag)",
                pageColor = new Color(1f, 0.9f, 0.7f)
            },
            new GuidePageData
            {
                title = "🌱 Cactus (Grass Type)",
                description = "<b>How to Draw:</b>\n" +
                             "1. Draw a <b>tall vertical line</b> for the body\n" +
                             "2. Add <b>small spikes</b> or arms on sides\n" +
                             "3. Keep it narrow and upright\n\n" +
                             "<b>Key Features:</b>\n" +
                             "• Tall and narrow shape\n" +
                             "• Vertical orientation\n" +
                             "• Sharp, spiky edges\n\n" +
                             "<b>Battle Moves:</b>\n" +
                             "• Vine Whip (curved line)\n" +
                             "• Leaf Storm (many short strokes)\n" +
                             "• Root Attack (vertical lines)",
                pageColor = new Color(0.7f, 1f, 0.7f)
            },
            new GuidePageData
            {
                title = "💧 Water Lily (Water Type)",
                description = "<b>How to Draw:</b>\n" +
                             "1. Draw <b>wide, rounded leaves</b>\n" +
                             "2. Make it <b>horizontal and flat</b>\n" +
                             "3. Add 3-5 smooth, curved strokes\n\n" +
                             "<b>Key Features:</b>\n" +
                             "• Wide and low shape\n" +
                             "• Horizontal orientation\n" +
                             "• Smooth, curved lines\n\n" +
                             "<b>Battle Moves:</b>\n" +
                             "• Water Splash (upward waves)\n" +
                             "• Bubble (small circles)\n" +
                             "• Healing Wave (horizontal wave - heals HP!)",
                pageColor = new Color(0.7f, 0.9f, 1f)
            },
            new GuidePageData
            {
                title = "Drawing Tips",
                description = "<b>General Tips:</b>\n\n" +
                             "• Use <b>3-8 strokes</b> for best results\n" +
                             "• Draw with <b>confident, smooth lines</b>\n" +
                             "• Think about the plant's <b>overall shape</b>\n" +
                             "• Vertical = Cactus, Round = Sunflower, Wide = Water Lily\n\n" +
                             "<b>Remember:</b>\n" +
                             "The system detects your plant based on shape patterns, " +
                             "so focus on the characteristic features!\n\n" +
                             "Good luck, and have fun drawing! 🎨",
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
        else
        {
            // Press H to open guide book
            if (Input.GetKeyDown(KeyCode.H))
            {
                OpenBook();
            }
        }
    }
}
