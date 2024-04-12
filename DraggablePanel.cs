using UnityEngine;
using UnityEngine.EventSystems;

public class DraggablePanel : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform panelRectTransform;
    public RectTransform canvasRectTransform; // The RectTransform of the canvas containing the panel

    public bool isDragging = false;
    private Vector2 originalPanelPosition;
    private Vector2 pointerOffset;

    public string identifierKey = "";

    public void ResetComponent()
    {

        panelRectTransform.localPosition = defaultPosition;
        REFERENCENEXUS.Instance.resizablePanel.resetComponent();
        PlayerPrefs.SetFloat("PanelPosX" + identifierKey, defaultPosition.x);
        PlayerPrefs.SetFloat("PanelPosY" + identifierKey, defaultPosition.y);
        PlayerPrefs.Save();
    }


    public void Update()
    {

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!REFERENCENEXUS.Instance.chatUp())
            {
                if (!REFERENCENEXUS.Instance.InventorySearchActivated())
                {
                    if (!REFERENCENEXUS.Instance.recipesmanager.recipeSearch.isFocused)
                    {

                        if (!REFERENCENEXUS.Instance.lootdropcanvas.lootDropBag.activeInHierarchy)
                        {

                            resetComponent();
                        }
                        else
                        {


                            REFERENCENEXUS.Instance.lootdropcanvas.TakeAll();
                        }
                    }
                }

            }
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, eventData.position, eventData.pressEventCamera, out pointerOffset);
        originalPanelPosition = panelRectTransform.localPosition;
    }

    public Vector2 defaultPosition;
    private void Start()
    {
        float x = PlayerPrefs.GetFloat("PanelPosX" + identifierKey, defaultPosition.x);
        float y = PlayerPrefs.GetFloat("PanelPosY" + identifierKey, defaultPosition.y);
        panelRectTransform.localPosition = new Vector3(x, y, panelRectTransform.localPosition.z);



        if (canvasRectTransform == null)
        {
            canvasRectTransform = transform.root.gameObject.GetComponent<RectTransform>();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            Vector2 localPointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, eventData.position, eventData.pressEventCamera, out localPointerPosition))
            {
                panelRectTransform.localPosition = originalPanelPosition + (localPointerPosition - pointerOffset);
                ClampToWindow();
            }
        }
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;

        PlayerPrefs.SetFloat("PanelPosX" + identifierKey, panelRectTransform.localPosition.x);
        PlayerPrefs.SetFloat("PanelPosY" + identifierKey, panelRectTransform.localPosition.y);
        PlayerPrefs.Save();
    }
    public float leftMargin = 0f;
    public float rightMargin = 0f;
    private void ClampToWindow()
    {
        Vector2 panelSize = new Vector2(panelRectTransform.rect.width, panelRectTransform.rect.height);
        Vector2 minPosition = (Vector2)canvasRectTransform.rect.min + panelSize * 0.5f;
        minPosition.x += leftMargin; 
        Vector2 maxPosition = (Vector2)canvasRectTransform.rect.max - panelSize * 0.5f;
        maxPosition.x -= rightMargin; 

        Vector2 clampedPosition = panelRectTransform.localPosition;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minPosition.x, maxPosition.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minPosition.y, maxPosition.y);

        panelRectTransform.localPosition = clampedPosition;
    }



}
