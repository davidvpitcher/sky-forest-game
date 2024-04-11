using UnityEngine;
using UnityEngine.EventSystems;

public class FarmPlotItemDragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public string itemName;
    public GameObject parentIcon;
  

    public void OnDrag(PointerEventData eventData)
    {

        REFERENCENEXUS.Instance.inventory.OnDragFromFarmPlot(eventData);
 
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
 
        REFERENCENEXUS.Instance.inventory.OnBeginDragFromFarmPlot(eventData, itemName, parentIcon);
  
    }

    public void OnEndDrag(PointerEventData eventData)
    {
     
        REFERENCENEXUS.Instance.inventory.OnEndDragFromFarmPlot(eventData, itemName);
  
    }

}
