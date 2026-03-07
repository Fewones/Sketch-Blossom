using System.Collections;
using UnityEngine;

public class IntroController : MonoBehaviour
{
    [SerializeField] private CanvasGroup menuRoot;
    [SerializeField] private float revealAtSeconds = 2.3f;
    [SerializeField] private float fadeDuration = 0.5f;

    private void Start()
    {
        // Ensure hidden at start (matches what you set manually)
        if (menuRoot != null)
        {
            menuRoot.alpha = 0f;
            menuRoot.interactable = false;
            menuRoot.blocksRaycasts = false;
        }

        StartCoroutine(RevealMenu());
    }

    private IEnumerator RevealMenu()
    {
        yield return new WaitForSeconds(revealAtSeconds);

        if (menuRoot == null) yield break;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            menuRoot.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        menuRoot.alpha = 1f;
        menuRoot.interactable = true;
        menuRoot.blocksRaycasts = true;
    }
}