using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SVImageController : MonoBehaviour, IDragHandler, IPointerClickHandler
{
    [SerializeField] private Image pickerImage;
    private RawImage SVImage;

    private DrawingColorSelector colorSelector;

    private RectTransform rectTransform, pickerTransform;

    public Camera mainCamera;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        SVImage = GetComponent<RawImage>();
        colorSelector = FindObjectOfType<DrawingColorSelector>();
        rectTransform = GetComponent<RectTransform>();

        pickerTransform = pickerImage.GetComponent<RectTransform>();
        pickerTransform.position = new Vector2(-(rectTransform.sizeDelta.x * 0.5f), -(rectTransform.sizeDelta.y *0.5f));
    }

    void updateColor(PointerEventData eventData)
    {
        Vector3 pos = mainCamera.ScreenToWorldPoint(eventData.position);
        pos = rectTransform.InverseTransformPoint(pos);
        float deltaX = rectTransform.sizeDelta.x * 0.5f;
        float deltaY = rectTransform.sizeDelta.y * 0.5f;

        if (pos.x < -deltaX)
        {
            pos.x = -deltaX;
        }
        if (pos.y < -deltaY)
        {
            pos.y = -deltaY;
        }
        if (pos.x > deltaX)
        {
            pos.x = deltaX;
        }
        if (pos.y > deltaY)
        {
            pos.y = deltaY;
        }
        pos.z = 0;

        float x = pos.x + deltaX;
        float y = pos.y + deltaY;

        float xNorm = x / rectTransform.sizeDelta.x;
        float yNorm = y / rectTransform.sizeDelta.y;

        pickerTransform.localPosition = pos;
        pickerImage.color = Color.HSVToRGB(0,0,1-yNorm);

        colorSelector.SetSV(xNorm, yNorm);
    }

    public void OnDrag(PointerEventData eventData)
    {
        updateColor(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        updateColor(eventData);
    }
}
