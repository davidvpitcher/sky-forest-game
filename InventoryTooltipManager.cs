using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryTooltipManager : MonoBehaviour
{
    public void ResetComponent()
    {


        HideTooltip();
    }

    public GameObject tooltipPanel; 
    public TextMeshProUGUI tooltipTitle; 
    public TextMeshProUGUI tooltipDescription;
    private float tooltipWidth;
    private float tooltipHeight; 

    public Image backgroundImage;

    void Start()
    {

        REFERENCENEXUS.Instance.inventorytooltipmanager = this;

        RectTransform rt = backgroundImage.GetComponent<RectTransform>();
        tooltipWidth = rt.sizeDelta.x;
        tooltipHeight = rt.sizeDelta.y;
    }



    private readonly Dictionary<string, (string, string)> tooltips = new Dictionary<string, (string, string)>
    {
        {"SeedsTab", ("Seeds", "Sort inventory by seeds")},
        {"SproutsTab", ("Sprouts", "Sort inventory by Sprouts")},
        {"ToolsTab", ("Tools", "Sort inventory by Tools")},
        {"BuildingTab", ("Building materials", "Sort inventory by building materials")},
        {"ResourcesTab", ("Resources", "Sort inventory by resources like harvested crops or wood")},
        {"PapersTab", ("Notes", "Sort inventory by notes and papers")},
        {"ConsumablesTab", ("Consumables", "Sort inventory by meals, potions, and other consumables.")},
        {"RefreshInventory", ("View All", "View All Inventory Items")},
        {"FishTab", ("Fish", "Sort inventory by fish and aquatic creatures")},
    };
    public void ShowTooltip(string buttonName)
    {
    
        if (tooltips.ContainsKey(buttonName))
        {
            tooltipTitle.text = tooltips[buttonName].Item1;
            tooltipDescription.text = tooltips[buttonName].Item2;
        }

    
        Vector2 cursorPosition = Input.mousePosition;

        float bufferDistanceX = cursorBufferX;
        float bufferDistanceY = cursorBufferY;

        Vector2 tooltipPosition = cursorPosition;

        tooltipPosition.x = cursorPosition.x < Screen.width / 2
            ? cursorPosition.x + bufferDistanceX + tooltipWidth
            : cursorPosition.x - bufferDistanceX - tooltipWidth;

        tooltipPosition.y = cursorPosition.y < Screen.height / 2
            ? cursorPosition.y + bufferDistanceY + tooltipHeight
            : cursorPosition.y - bufferDistanceY - tooltipHeight;

        tooltipPosition.x = Mathf.Clamp(tooltipPosition.x, tooltipWidth / 2, Screen.width - tooltipWidth / 2);
        tooltipPosition.y = Mathf.Clamp(tooltipPosition.y, tooltipHeight / 2, Screen.height - tooltipHeight / 2);

        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        tooltipRect.position = new Vector3(tooltipPosition.x, tooltipPosition.y, 0f);

        tooltipRect.pivot = new Vector2(cursorPosition.x < Screen.width / 2 ? 0 : 1, cursorPosition.y < Screen.height / 2 ? 0 : 1);

        tooltipPanel.SetActive(true);

    }

    private float cursorBufferX = 10f;
    private float cursorBufferY = 10f; 





    public void HideTooltip()
    {
        if (tooltipPanel == null)
        {
            return;
        }
        tooltipPanel.SetActive(false);
    }
}
