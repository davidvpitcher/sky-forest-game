using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FlourMillManager : MonoBehaviour
{

    void Start()
    {

        REFERENCENEXUS.Instance.flourmillmanager = this;
    }


    public GameObject flourMillDisplay;




    public void onRightClickFlourMillItem(GameObject whowasI, string itemType, LOCALDATABASEMANAGER.SerializableUniqueItem uniqueItemUIElementUniqueItem, int quantityToSend)
    {


        PlayerLoadedLumberToFlourMill(itemType, quantityToSend);

    }
    public void FlourMillStatusUpdate(int treeID, int newStatus)
    {
        if (treeID != currentFlourMill)
        {
             return;
        }

        if (newStatus == 0)
        {
            flourMillDisplay.SetActive(false);

        }
        else if (newStatus == 1)
        {

            flourMillDisplay.SetActive(true);

        }

    }

    public void OnFlourMillRanOutOfFuel()
    {
        flourMillDisplay.SetActive(false);
    }
    public void UpdateFlourMillUI(int treeID, int flourMillCampfireLitStatus, NETWORKBROADCASTER.FuelData fuelData, NETWORKBROADCASTER.OutputData outputData)
    {
        if (treeID != currentFlourMill)
        {
            return;
        }
        ScrubContent();  
                     
        FlourMillStatusUpdate(treeID, flourMillCampfireLitStatus);
        if (fuelData != null && !string.IsNullOrEmpty(fuelData.fuel))
        {
          AddInventoryItemToAnyMenu(new List<string> { fuelData.fuel }, fuelContentHolder, new List<int> { fuelData.quantity }, treeID);


        }
      



        List<string> outputNames = new List<string>();
        List<int> outputQuantities = new List<int>();

        if (outputData != null && outputData.outputs.Count > 0)
        {
            foreach (var output in outputData.outputs)
            {
                outputNames.Add(output.name);
                outputQuantities.Add(output.quantity);
            }
        }
        else
        {
            Debug.Log("UpdateFlourMillUI: No output data.");
        }
        AddInventoryItemToAnyMenu(outputNames, productContentHolder, outputQuantities, treeID);

        REFERENCENEXUS.Instance.menuManager.OnOpenedMenu(flourMillBag);
        REFERENCENEXUS.Instance.menuManager.OpenInventory();
        REFERENCENEXUS.Instance.EnableCursor();


        FillWithPlaceholders(productContentHolder, faintBackground, limit, outputData.outputs.Count);
    }

    private int limit = 10;
    private void FillWithPlaceholders(Transform contentHolder, GameObject placeholderPrefab, int limit, int existingItems)
    {

        int missingItemsInHolder = limit - existingItems;

 
        for (int i = 0; i < missingItemsInHolder; i++)
        {
            Instantiate(placeholderPrefab, contentHolder);
       
        }

    
    }



    public void PlayerLoadedLumberToFlourMill(string resourceType, int quantity)
    {
        REFERENCENEXUS.Instance.networkbroadcaster.PlayerAddedSomethingToFlourMill(REFERENCENEXUS.Instance.gsc.realLocalPlayerConnection, resourceType, quantity, currentFlourMill, DBManager.username);
    }
    public void PlayerRemovedLumberFromFlourMill()
    {

    }

    public void PlayerClickedUseFlourMill()
    {
        if (currentFlourMill == -1)
        {
            return;

        }
     
        REFERENCENEXUS.Instance.gsc.realLocalInventory.UseFlourMillFlame(currentFlourMill);

    }


    public int currentFlourMill = -1;

    public void ResetComponent()
    {
        flourMillBag.SetActive(false);
        flourMillDisplay.SetActive(false);

        lastClickTime = 0f;

    }

    public GameObject faintBackground;


    public Transform fuelContentHolder;


    public Transform productContentHolder;

    public GameObject flourMillBag;



    public void ScrubContent()
    {
        flourMillDisplay.SetActive(false);
        foreach (Transform child in fuelContentHolder)
        {
            Destroy(child.gameObject);
        }
   
        foreach (Transform child in productContentHolder)
        {
            Destroy(child.gameObject);
        }



    }


    public void OpenFlourMillInterface(int treeID)
    {

        ScrubContent();

        currentFlourMill = treeID;


        REFERENCENEXUS.Instance.networkbroadcaster.RequestFlourMillContents(treeID);

    }


    public void CloseFlourMillInterface()
    {

        bool wasActive = flourMillBag.activeInHierarchy;
        int lastFlourMill = currentFlourMill;

        REFERENCENEXUS.Instance.menuManager.OnClosedMenu(flourMillBag);

        REFERENCENEXUS.Instance.menuManager.DisableCursorIfNoMenus();

        currentFlourMill = -1;

        if (REFERENCENEXUS.Instance.invPanel2 == null)
        {
            return;
        }
        if (REFERENCENEXUS.Instance.invPanel2.activeInHierarchy)
        {

            REFERENCENEXUS.Instance.menuManager.CloseInventory();
        }

        if (wasActive && lastFlourMill != -1)
        {

            REFERENCENEXUS.Instance.networkbroadcaster.NotifyStoppedUsingFlourMill(lastFlourMill);
        }
    }

    public void CloseFlourMillInterfaceWithoutInventory()
    {
        bool wasActive = flourMillBag.activeInHierarchy;
        int lastFlourMill = currentFlourMill;

        REFERENCENEXUS.Instance.menuManager.OnClosedMenu(flourMillBag);

        REFERENCENEXUS.Instance.menuManager.DisableCursorIfNoMenus();

        currentFlourMill = -1;


        if (wasActive && lastFlourMill != -1)
        {

            REFERENCENEXUS.Instance.networkbroadcaster.NotifyStoppedUsingFlourMill(lastFlourMill);
        }
    }



    public void AddInventoryItemToAnyMenu(List<string> itemNames, Transform contentHolder, List<int> quantities, int whatFarmPlot)
    {


        OTHERINVENTORY otherinventory = REFERENCENEXUS.Instance.otherinventory;


        PlayerInventory localPlayerInventory = REFERENCENEXUS.Instance.gsc.realLocalInventory;



        List<string> namess = new List<string>();
        List<int> amts = new List<int>();

        namess = itemNames;
        amts = quantities;

        foreach (string itemName in namess)
        {
            int idex2 = namess.IndexOf(itemName);

            int amtt22 = amts[idex2];

            if (amtt22 > 0)
            {


                GameObject obj = Instantiate(otherinventory.standardizedMenuPrefab, contentHolder);

              InventoryItemTooltipHandler inventoryItemTooltipHandler = obj.AddComponent<InventoryItemTooltipHandler>();
                inventoryItemTooltipHandler.itemname = itemName;
                inventoryItemTooltipHandler.tooltipmanager = otherinventory.tooltipmanager;
                inventoryItemTooltipHandler.dontAnchor = true;
         
                ItemGuideClient theItemGuide = otherinventory.getItemGuideFromString(itemName);


                if (theItemGuide == null)
                {
                    Debug.LogError("missing item guide for " + itemName);
                }
                obj.gameObject.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = theItemGuide.itemSprite;


                bool xbox = REFERENCENEXUS.Instance.isUsingXboxController;

                if (xbox)
                {

                    otherinventory.CleanObjectForXbox(obj);
                }


               FlourMillDragHandler dragHandler = obj.gameObject.AddComponent<FlourMillDragHandler>();
                dragHandler.itemName = name;
                dragHandler.parentIcon = obj;

                obj.transform.GetChild(1).gameObject.SetActive(false);

                int idex = namess.IndexOf(itemName);

                int amtt = amts[idex];

                if (amtt > 1)
                {

                    if (amtt > 999)
                    {
                        amtt = 999;
                    }


                    obj.transform.GetChild(2).gameObject.SetActive(true);
                    obj.transform.GetChild(2).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = amtt.ToString();
                }
                else
                {
                    obj.transform.GetChild(2).gameObject.SetActive(false);

                }


                LOCALDATABASEMANAGER.SerializableUniqueItem uniqueItem = obj.GetComponent<UniqueItemUIElement>().uniqueItem;
                CustomButton customButton = obj.GetComponent<CustomButton>();
            
                customButton.lastclickedwasweapon = false;
                customButton.itemRepresented = itemName;

                customButton.OnRightClick.AddListener(delegate { localPlayerInventory.doSomethingOnRightClickFromFlourMill(obj.gameObject, itemName, false, uniqueItem, whatFarmPlot); });
          customButton.OnShiftRightClick.AddListener(delegate { localPlayerInventory.doSomethingOnShiftRightClickFromFlourMill(obj.gameObject, itemName, false, uniqueItem, whatFarmPlot); });


                customButton.onClick.AddListener(delegate { SingleOrDoubleClick(obj, itemName, false); });

            }
        }
    }


    private float lastClickTime = 0;
    private float doubleClickTime = 0.4f; // Set the time within which a second click will count as a double-click

    public void SingleOrDoubleClick(GameObject clickedObject, string itemName, bool weapon)
    {
        float timeSinceLastClick = Time.time - lastClickTime;



        lastClickTime = Time.time;
    }









}
