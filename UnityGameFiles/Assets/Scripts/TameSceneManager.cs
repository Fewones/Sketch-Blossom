using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Manages the Tame reward scene
/// Shows captured enemy message and transitions to WorldMapScene
/// </summary>
public class TameSceneManager : MonoBehaviour
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
        // Set message text - could be enhanced to show the tamed plant's name
        if (messageText != null)
        {
            messageText.text = "Plant successfully tamed!\n\n" +
                              "Added to your collection!";
        }

        // Setup continue button
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        Debug.Log("TameScene: Setup complete");
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
        Debug.Log("TameScene: Transitioning to WorldMapScene...");

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
