using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NumberPickerCanvas : MonoBehaviour
{

    public void ResetComponent()
    {
        numberpickerBag.SetActive(false);

        pickingForMarket = false;
        pickingForSell = false;

        inputField.text = "";

        inputField.DeactivateInputField();
    }

    public GameObject numberpickerBag;

    public TMP_InputField inputField;
    public void Start()
    {
        REFERENCENEXUS.Instance.numberpickercanvas = this;
    }

    public bool pickingForMarket = false;
    public bool pickingForSell = false;

    public void OpenNumberPicker()
    {

        REFERENCENEXUS.Instance.menuManager.OnOpenedMenu(numberpickerBag);

        inputField.ActivateInputField();
        inputField.text = "0";
        PositionPanelAtCursor();
    }

    private void PositionPanelAtCursor()
    {

        // Ensure the panel is active before setting its position
        if (!panelTransform.gameObject.activeSelf)
        {
            panelTransform.gameObject.SetActive(true);
    
        }

        Vector2 position;
        // Convert the mouse position to canvas space
        bool isPositionConverted = RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasTransform, Input.mousePosition, null, out position);

        if (!isPositionConverted)
        {
            //    Debug.LogError("Failed to convert mouse position to local canvas position.");
            return;
        }

        //  Debug.Log($"Mouse position converted to canvas space: {position}");
        // Debug.Log($"Panel pivot before adjustment: {panelTransform.pivot}");

        // Ensure the panel's pivot is set to the top-left
        panelTransform.pivot = new Vector2(0, 1);
      
        panelTransform.anchoredPosition = position + new Vector2(0, 140f);

     
    }

    public RectTransform canvasTransform; //
    public RectTransform panelTransform;
    public void CloseNumberPicker()
    {
        inputField.DeactivateInputField();
        REFERENCENEXUS.Instance.menuManager.OnClosedMenu(numberpickerBag);

        REFERENCENEXUS.Instance.menuManager.DisableCursorIfNoMenus();
        pickingForMarket = false;
        pickingForSell = false;
    }

    public void OnPressedOne()
    {
        PlayerPressedNumberKey(1);

    }

    
    public void OnPressedTwo()
    {
        PlayerPressedNumberKey(2);
    }

    
    public void OnPressedThree()
    {
        PlayerPressedNumberKey(3);
    }

    
    public void OnPressedFour()
    {
        PlayerPressedNumberKey(4);
    }

    
    public void OnPressedFive()
    {
        PlayerPressedNumberKey(5);
    }

    
    public void OnPressedSix()
    {
        PlayerPressedNumberKey(6);
    }

    
    public void OnPressedSeven()
    {
        PlayerPressedNumberKey(7);
    }

    
    public void OnPressedEight()
    {
        PlayerPressedNumberKey(8);
    }

    
    public void OnPressedNine()
    {
        PlayerPressedNumberKey(9);
    }

    
    public void OnPressedMax()
    {

        if (pickingForMarket)
        {
            if (string.IsNullOrEmpty(REFERENCENEXUS.Instance.marketplacecanvas.lastClickedItem))
            {
                return;
            }


       

            string whichShop = REFERENCENEXUS.Instance.marketplacecanvas.lastMarketplaceType;

            List<MARKETPLACEMANAGER.ShopMassItem> shopItems = new List<MARKETPLACEMANAGER.ShopMassItem>();

            if (whichShop == "bear")
            {
                shopItems = REFERENCENEXUS.Instance.marketplacemanager.bearShopMassItemsForSale;
            }
            else if (whichShop == "fox")
            {

                shopItems = REFERENCENEXUS.Instance.marketplacemanager.shopMassItemsForSale;
            }



            int priceofgoods = REFERENCENEXUS.Instance.marketplacemanager.GetPricePerUnit(REFERENCENEXUS.Instance.marketplacecanvas.lastClickedItem, shopItems);



            int currentFunds = DBManager.funds;

            int totalpossible = currentFunds / priceofgoods;
       

            if (totalpossible < 0)
            {
                totalpossible = 0;
            }
            inputField.text = totalpossible.ToString();

        } else if (pickingForSell)
        {

            if (string.IsNullOrEmpty(REFERENCENEXUS.Instance.marketplacecanvas.lastClickedSellItem))
            {
                return;
            }


  



            int totalpossible = REFERENCENEXUS.Instance.gsc.realLocalInventory.GetInventoryItemCount(REFERENCENEXUS.Instance.marketplacecanvas.lastClickedSellItem);
        

            if (totalpossible < 0)
            {
                totalpossible = 0;
            }
            inputField.text = totalpossible.ToString();





        }


    }

    public void OnPressedOkay()
    {
        if (inputField.text == "0")
        {
            CloseNumberPicker();
            return;
        }
        if (pickingForMarket)
        {

            REFERENCENEXUS.Instance.marketplacecanvas.OnPressedOkayForMultiple(inputField.text);
        }
        if (pickingForSell)
        {

            REFERENCENEXUS.Instance.marketplacecanvas.OnPressedOkayForMultipleSell(inputField.text);
        }

        CloseNumberPicker();
    }

    public void OnPressedCancel()
    {

        CloseNumberPicker();
    }

    public void OnPressedZero()
    {
        PlayerPressedNumberKey(0);


    }

    public void OnInputValueChanged()
    {
        string ourtext = inputField.text;

        if (ourtext == "0")
        {
            return;
        }

        if (!int.TryParse(ourtext, out int textAsInt))
        {
            REFERENCENEXUS.Instance.SendLocalMessage("Invalid input. Please enter a valid number.");
            return;
        }


        int totalpossible = 0;

        if (pickingForSell)
        {
            if (string.IsNullOrEmpty(REFERENCENEXUS.Instance.marketplacecanvas.lastClickedSellItem))
            {
                REFERENCENEXUS.Instance.SendLocalMessage("No item selected.");
                return;
            }
            totalpossible = REFERENCENEXUS.Instance.gsc.realLocalInventory.GetInventoryItemCount(REFERENCENEXUS.Instance.marketplacecanvas.lastClickedSellItem);
        }
        if (pickingForMarket)
        {
            if (string.IsNullOrEmpty(REFERENCENEXUS.Instance.marketplacecanvas.lastClickedItem))
            {
                REFERENCENEXUS.Instance.SendLocalMessage("No item selected.");
                return;
            }

            string whichShop = REFERENCENEXUS.Instance.marketplacecanvas.lastMarketplaceType;

            List<MARKETPLACEMANAGER.ShopMassItem> shopItems = new List<MARKETPLACEMANAGER.ShopMassItem>();

            if (whichShop == "bear")
            {
                shopItems = REFERENCENEXUS.Instance.marketplacemanager.bearShopMassItemsForSale;
            }
            else if (whichShop == "fox")
            {

                shopItems = REFERENCENEXUS.Instance.marketplacemanager.shopMassItemsForSale;
            }


            int priceofgoods = REFERENCENEXUS.Instance.marketplacemanager.GetPricePerUnit(REFERENCENEXUS.Instance.marketplacecanvas.lastClickedItem, shopItems);



            int currentFunds = DBManager.funds;

            totalpossible = currentFunds / priceofgoods;

        }


        if (textAsInt > totalpossible)
        {
            if (totalpossible < 0)
            {
                return;
            }
            else
            {

                ourtext = totalpossible.ToString();
            }
        }




        inputField.text = ourtext;


    }
    public void OnPressedBack()
    {


        string currentInput = inputField.text;

        if (inputField.text == "0")
        {
            return;
        }

        if (inputField.text.Length == 1)
        {
            inputField.text = "0";
            return;
        }
        if (inputField.text.Length < 1)
        {
            return;
        }

        currentInput = currentInput.Substring(0, currentInput.Length - 1);

        inputField.text = currentInput;
    }

    public void OnPressedClear()
    {

        inputField.text = "0";

    }


    public void PlayerPressedNumberKey(int whichNumber)
    {
        string ourtext = inputField.text;

        if (ourtext.Length > 4)
        {
            return;
        }
        if (ourtext == "0")
        {
            ourtext = "";
        }
        ourtext += whichNumber.ToString();
      
        
        if (!int.TryParse(ourtext, out int textAsInt))
        {
            REFERENCENEXUS.Instance.SendLocalMessage("Invalid input. Please enter a valid number.");
            return;
        }

        if (textAsInt < 0)
        {

            REFERENCENEXUS.Instance.SendLocalMessage("Invalid input. Please enter a valid number.");
            return;

        }

        int totalpossible = 0;

        if (pickingForSell)
        {
            if (string.IsNullOrEmpty(REFERENCENEXUS.Instance.marketplacecanvas.lastClickedSellItem))
            {
                REFERENCENEXUS.Instance.SendLocalMessage("No item selected.");
                return;
            }
            totalpossible =    REFERENCENEXUS.Instance.gsc.realLocalInventory.GetInventoryItemCount(REFERENCENEXUS.Instance.marketplacecanvas.lastClickedSellItem);
        }
        if (pickingForMarket)
        {
            if (string.IsNullOrEmpty(REFERENCENEXUS.Instance.marketplacecanvas.lastClickedItem))
            {
                REFERENCENEXUS.Instance.SendLocalMessage("No item selected.");
                return;
            }

            string whichShop = REFERENCENEXUS.Instance.marketplacecanvas.lastMarketplaceType;

            List<MARKETPLACEMANAGER.ShopMassItem> shopItems = new List<MARKETPLACEMANAGER.ShopMassItem>();

            if (whichShop == "bear")
            {
                shopItems = REFERENCENEXUS.Instance.marketplacemanager.bearShopMassItemsForSale;
            }
            else if (whichShop == "fox")
            {

                shopItems = REFERENCENEXUS.Instance.marketplacemanager.shopMassItemsForSale;
            }

            int priceofgoods = REFERENCENEXUS.Instance.marketplacemanager.GetPricePerUnit(REFERENCENEXUS.Instance.marketplacecanvas.lastClickedItem, shopItems);



            int currentFunds = DBManager.funds;

             totalpossible = currentFunds / priceofgoods;
            
        }

      
        if (textAsInt > totalpossible)
        {
            if (totalpossible < 0)
            {
                Debug.Log("problem with total possible");
                return;
            }
            else
            {

                ourtext = totalpossible.ToString();
            }
        }

        


        inputField.text = ourtext;


    }
}
