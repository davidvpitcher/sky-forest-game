using UnityEngine;
using UnityEngine.EventSystems;

public class ResizablePanel : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform panelRectTransform;
    public RectTransform cornerDragRectTransform;

    public bool isResizing = false;
    private Vector2 originalMousePosition;
    private Vector2 initialSize;
    private Vector2 initialSizeOriginal;

    public Vector2 minSize = new Vector2(100, 100);
    public Vector2 maxSize = new Vector2(600, 600);

    public bool squareOnlyMode = false;

    public string identifierKey;

    public Vector3 defaultScale;
    public Vector3 defaultSizeDelta;
    public void ResetComponent()
    {

        panelRectTransform.sizeDelta = initialSizeOriginal;
      
        PlayerPrefs.SetFloat("PanelWidth" + identifierKey, panelRectTransform.sizeDelta.x);
        PlayerPrefs.SetFloat("PanelHeight" + identifierKey, panelRectTransform.sizeDelta.y);
        PlayerPrefs.Save();

    }
    private void Start()
    {
        initialSizeOriginal = panelRectTransform.sizeDelta; // Store the initial size of the panel
         panelRectTransform.pivot = new Vector2(0, 1);


        float width = PlayerPrefs.GetFloat("PanelWidth" + identifierKey, defaultSizeDelta.x);
        float height = PlayerPrefs.GetFloat("PanelHeight" + identifierKey, defaultSizeDelta.y);


        panelRectTransform.sizeDelta = new Vector2(width, height);


      
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(cornerDragRectTransform, eventData.position, eventData.pressEventCamera))
        {
            isResizing = true;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, eventData.position, eventData.pressEventCamera, out originalMousePosition);
            initialSize = panelRectTransform.sizeDelta; 
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isResizing)
        {
            Vector2 localPointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, eventData.position, eventData.pressEventCamera, out localPointerPosition))
            {
                Vector2 sizeDelta = localPointerPosition - originalMousePosition;
                Vector2 newSize = initialSize + new Vector2(sizeDelta.x, -sizeDelta.y);

                if (squareOnlyMode)
                {
                    float maxDimension = Mathf.Max(newSize.x, newSize.y);
                    newSize = new Vector2(maxDimension, maxDimension);
                }

                newSize.x = Mathf.Clamp(newSize.x, minSize.x, maxSize.x);
                newSize.y = Mathf.Clamp(newSize.y, minSize.y, maxSize.y);

                panelRectTransform.sizeDelta = newSize;
            }
        }
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        isResizing = false;

        PlayerPrefs.SetFloat("PanelWidth" + identifierKey, panelRectTransform.sizeDelta.x);
        PlayerPrefs.SetFloat("PanelHeight" + identifierKey, panelRectTransform.sizeDelta.y);
        PlayerPrefs.Save();
    }
}
