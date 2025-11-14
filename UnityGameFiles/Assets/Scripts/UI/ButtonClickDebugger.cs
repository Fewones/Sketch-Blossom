using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Attach this to a button to debug why it's not receiving clicks
/// </summary>
public class ButtonClickDebugger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        Debug.Log($"[ButtonClickDebugger] Attached to: {gameObject.name}");
    }

    private void Update()
    {
        // Log button state every 2 seconds
        if (Time.frameCount % 120 == 0)
        {
            Debug.Log($"[{gameObject.name}] Active: {gameObject.activeInHierarchy}, Interactable: {button?.interactable}");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"✓ [{gameObject.name}] MOUSE ENTERED BUTTON AREA");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"← [{gameObject.name}] Mouse exited button");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"↓ [{gameObject.name}] MOUSE DOWN on button");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log($"↑ [{gameObject.name}] MOUSE UP on button");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"★ [{gameObject.name}] BUTTON CLICKED!");
        if (button != null)
        {
            Debug.Log($"  Button interactable: {button.interactable}");
            Debug.Log($"  Button onClick listener count: {button.onClick.GetPersistentEventCount()}");
        }
    }
}
