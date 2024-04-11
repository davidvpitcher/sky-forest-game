using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SharedItem", menuName = "Inventory/SharedItem", order = 1)]
public class ItemGuideBase : ScriptableObject
{
    public string itemName;
    public string itemType;
    public int durability;
    public int priceToPlace;
}
    
