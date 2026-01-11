using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Manages the Wild Growth reward scene
/// Shows plant growth animation/message and transitions to WorldMapScene
/// </summary>
public class WildGrowthSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject contentPanel;

    [Header("Transition Settings")]
    [SerializeField] private float autoTransitionDelay = 3f;
    [SerializeField] private bool useAutoTransition = true;

    private void Start()
    {
        SetupUI();

        if (useAutoTransition)
        {
            StartCoroutine(AutoTransitionToWorldMap());
        }
    }

    private void SetupUI()
    {
        // Set message text
        if (messageText != null)
        {
            messageText.text = "Your plant has grown stronger!\n\n" +
                              "Stats have been increased!";
        }

        // Setup continue button
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        Debug.Log("WildGrowthScene: Setup complete");
    }

    private IEnumerator AutoTransitionToWorldMap()
    {
        yield return new WaitForSeconds(autoTransitionDelay);
        TransitionToWorldMap();
    }

    private void OnContinueClicked()
    {
        TransitionToWorldMap();
    }

    private void TransitionToWorldMap()
    {
        Debug.Log("WildGrowthScene: Transitioning to WorldMapScene...");

        // Clear world map encounter flag since we're returning from a completed battle
        if (EnemyEncounterData.Instance != null)
        {
            EnemyEncounterData.Instance.ClearEncounterData();
        }

        SceneManager.LoadScene("WorldMapScene");
    }

    private void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
        }
    }
}
