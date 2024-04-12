using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyQuestCanvas : MonoBehaviour
{
    public GameObject dailiesbag;


    public GameObject rewardsShopPanel;


    public List<GameObject> bunnyImagesForRewardsShop = new List<GameObject>();


    public Transform shopContent;

    public Image medalsOwned;

    public TextMeshProUGUI medalsOwnedCount;
 

    public RewardShopItemUIAdjuster shopItemRewardPrefab;

    public GameObject purchaseButton;

    public GameObject fishingMedalOwnedImageBag;
    public GameObject farmingMedalOwnedImageBag;
    public GameObject cookingMedalOwnedImageBag;

    public GameObject fishingMedalOwnedImage;
    public GameObject farmingMedalOwnedImage;
    public GameObject cookingMedalOwnedImage;



    public Sprite fishingMedalSprite;
    public Sprite cookingMedalSprite;
    public Sprite farmingMedalSprite;

    public Color tabButtonColorNormal;
    public Color tabButtonColorSelected;

    public Image fishingTabButtonImage;
    public Image farmingTabButtonImage;
    public Image cookingTabButtonImage;

    public GameObject purchaseResponseSuccess;
    public GameObject purchaseResponseFail;

    public TextMeshProUGUI purchaseResponseSuccessText;
    public TextMeshProUGUI purchaseResponseFailText;

    public void OnPurchaseSuccess(string itemname)
    {
        purchaseResponseSuccessText.text = "Successfully purchased " + itemname;

        purchaseResponseSuccess.SetActive(true);
        StartCoroutine(ShowSuccessCD());
    }
    public void OnPurchaseFail()
    {
        purchaseResponseFail.SetActive(true);
        StartCoroutine(ShowFailCD());
    }
    private IEnumerator ShowSuccessCD()
    {
        yield return new WaitForSeconds(3f);
        
        purchaseResponseSuccess.SetActive(false);
    }
    private IEnumerator ShowFailCD()
    {
        yield return new WaitForSeconds(3f);

        purchaseResponseFail.SetActive(false);
    }

    public void OnEnable()
    {
        purchaseResponseSuccess.SetActive(false);
        purchaseResponseFail.SetActive(false);

    }
    public string currentShop;
    public void PlayerPressedRewardsShopFishingTab()
    {
        string shopType = "ANGLERSTRIUMPHMEDAL";
        PlayerPrefs.SetString("REWARDSSHOP", shopType);
        RefreshFishingContent(shopType);

        fishingTabButtonImage.color = tabButtonColorSelected;
        farmingTabButtonImage.color = tabButtonColorNormal;
        cookingTabButtonImage.color = tabButtonColorNormal;

        //  medalsOwned.sprite = fishingMedalSprite;

        fishingMedalOwnedImageBag.SetActive(true);
        farmingMedalOwnedImageBag.SetActive(false);
        cookingMedalOwnedImageBag.SetActive(false);
        ResetLastClicked();

        REFERENCENEXUS.Instance.clientsidesfx.PlaySoundEffectByName("Click (3)");
        int medalsOwned = REFERENCENEXUS.Instance.gsc.realLocalInventory.GetInventoryItemCount(shopType);

        if (medalsOwned > 99)
        {
            medalsOwned = 99;
        }
        medalsOwnedCount.text = medalsOwned.ToString();
        currentShop = shopType;
    }
    public void PlayerPressedRewardsShopFarmingTab()
    {

        string shopType = "HARVESTCRESTMEDAL";
        PlayerPrefs.SetString("REWARDSSHOP", shopType);
        RefreshFarmingContent(shopType);

        fishingTabButtonImage.color = tabButtonColorNormal;
        farmingTabButtonImage.color = tabButtonColorSelected;
        cookingTabButtonImage.color = tabButtonColorNormal;

        //   medalsOwned.sprite = farmingMedalSprite;
        fishingMedalOwnedImageBag.SetActive(false);
        farmingMedalOwnedImageBag.SetActive(true);
        cookingMedalOwnedImageBag.SetActive(false);

        ResetLastClicked();
        REFERENCENEXUS.Instance.clientsidesfx.PlaySoundEffectByName("Click (3)");

        int medalsOwned = REFERENCENEXUS.Instance.gsc.realLocalInventory.GetInventoryItemCount(shopType);

        if (medalsOwned > 99)
        {
            medalsOwned = 99;
        }
        medalsOwnedCount.text = medalsOwned.ToString();
        currentShop = shopType;
    }
    public void PlayerPressedRewardsShopCookingTab()
    {
        string shopType = "CULINARYSTARMEDAL";
        PlayerPrefs.SetString("REWARDSSHOP", shopType);
        RefreshCookingContent(shopType);
        fishingTabButtonImage.color = tabButtonColorNormal;
        farmingTabButtonImage.color = tabButtonColorNormal;
        cookingTabButtonImage.color = tabButtonColorSelected;

        fishingMedalOwnedImageBag.SetActive(false);
        farmingMedalOwnedImageBag.SetActive(false);
        cookingMedalOwnedImageBag.SetActive(true);
        ResetLastClicked();
        REFERENCENEXUS.Instance.clientsidesfx.PlaySoundEffectByName("Click (3)");
        int medalsOwned = REFERENCENEXUS.Instance.gsc.realLocalInventory.GetInventoryItemCount(shopType);

        if (medalsOwned > 99)
        {
            medalsOwned = 99;
        }
        medalsOwnedCount.text = medalsOwned.ToString();
        currentShop = shopType;
    }

    public void RefreshFarmingContent(string shopType)
    {
        ScrubShopContent();
        PopulateFarmingShopContent(shopType);
    }

    public void RefreshFishingContent(string shopType)
    {
        ScrubShopContent();
        PopulateFishingShopContent(shopType);

    }

    public void RefreshCookingContent(string shopType)
    {
        ScrubShopContent();

        PopulateCookingShopContent(shopType);

    }


    public void ScrubShopContent()
    {
        rewardShopItems.Clear();
        foreach (Transform child in shopContent)
        {

            Destroy(child.gameObject);
        }
    }

    public string lastClickedItemName;
    public LOCALDATABASEMANAGER.SerializableUniqueItem lastClickedSerializableUnique;
    public UniqueItem lastClickedUniqueItem;
    
    public void ResetLastClicked()
    {
        lastClickedItemName = "";
        lastClickedSerializableUnique = null;
        lastClickedUniqueItem = null;
        purchaseButton.SetActive(false);
        
    }

    public void PlayerPressedPurchase()
    {

        if (string.IsNullOrEmpty(lastClickedItemName))
        {
            Debug.Log("no item selected");
            REFERENCENEXUS.Instance.SendLocalMessage("No selected item");
            return;
        }
   

        REFERENCENEXUS.Instance.networkbroadcaster.advancednetworkfunctions.AttemptRewardShopPurchase(lastClickedItemName, lastClickedSerializableUnique);

        REFERENCENEXUS.Instance.clientsidesfx.PlaySoundEffectByName("Click (3)");

    }

    public void RefreshRewardsShop()
    {
        if (string.IsNullOrEmpty(currentShop))
        {
            return;
        }
        int medalsOwned = REFERENCENEXUS.Instance.gsc.realLocalInventory.GetInventoryItemCount(currentShop);

        if (medalsOwned > 99)
        {
            medalsOwned = 99;
        }
        medalsOwnedCount.text = medalsOwned.ToString();


    }
    public void OnResultReceivedFromServer(bool success, string itemName, int quantity)
    {

        lastClickedItemName = "";
        lastClickedSerializableUnique = null;
        lastClickedUniqueItem = null;

        purchaseButton.SetActive(false);
        if (success)
        {

            REFERENCENEXUS.Instance.SendLocalMessage($"Successfully purchased {itemName} x{quantity}");
            REFERENCENEXUS.Instance.clientsidesfx.PlaySoundEffectByName("Item purchase 13");
            OnPurchaseSuccess(itemName);
        } else
        {
            REFERENCENEXUS.Instance.SendLocalMessage($"Insufficient medals to purchase {itemName}");
            REFERENCENEXUS.Instance.clientsidesfx.PlaySoundEffectByName("Buzz Error (3)");
            OnPurchaseFail();
        }
    }
    public void PlayerPressedButtonOnRewardShopItem(string itemName, LOCALDATABASEMANAGER.SerializableUniqueItem serUnique, UniqueItem uniqueItem, bool canAfford)
    {

        lastClickedItemName = itemName;
        lastClickedSerializableUnique = serUnique;
        lastClickedUniqueItem = uniqueItem;
        REFERENCENEXUS.Instance.clientsidesfx.PlaySoundEffectByName("Click (3)");
        foreach (RewardShopItemUIAdjuster rewardShopItem in rewardShopItems)
        {

            rewardShopItem.ResetColor();

        }

        if (canAfford)
        {

            purchaseButton.SetActive(true);
        } else
        {

            purchaseButton.SetActive(false);
        }

        Debug.Log("BUTTN PRESSED");

    }

    public List<RewardShopItemUIAdjuster> rewardShopItems = new List<RewardShopItemUIAdjuster>();

    public Sprite getSpriteForCoinType(string ShopType)
    {
        switch (ShopType)
        {

            case "CULINARYSTARMEDAL":
                return cookingMedalSprite;
            case "HARVESTCRESTMEDAL":

                return farmingMedalSprite;
            case "ANGLERSTRIUMPHMEDAL":

                return fishingMedalSprite;
            default:

                return fishingMedalSprite;

        }
    }
    public void PopulateFishingShopContent(string shopType)
    {

        Sprite corectSprite = getSpriteForCoinType(shopType);
        foreach (ADVANCEDNETWORKFUNCTIONS.RewardShopMassItem massItem in REFERENCENEXUS.Instance.networkbroadcaster.advancednetworkfunctions.purchaseableMassItemsFishing)
        {

            RewardShopItemUIAdjuster newContent = Instantiate(shopItemRewardPrefab, shopContent);
            newContent.dailyquestcanvas = this;
            rewardShopItems.Add(newContent);
            newContent.SetUpComponent(massItem.massItem.itemName, null, massItem.price, shopType, null, corectSprite);

        }
        foreach(ADVANCEDNETWORKFUNCTIONS.RewardShopUniqueItem uniqeItem in REFERENCENEXUS.Instance.networkbroadcaster.advancednetworkfunctions.purchaseableUniqueItemsFishing)
        {

            RewardShopItemUIAdjuster newRewards = Instantiate(shopItemRewardPrefab, shopContent);

            newRewards.dailyquestcanvas = this;
            rewardShopItems.Add(newRewards);
            LOCALDATABASEMANAGER.SerializableUniqueItem newSerializable = REFERENCENEXUS.Instance.ConvertUniqueToSerializableUniqueItem(uniqeItem.uniqueItem);
          
            newRewards.SetUpComponent(uniqeItem.uniqueItem.ItemName, newSerializable, uniqeItem.price, shopType, uniqeItem.uniqueItem, corectSprite);

        }



    }
    
    public void PopulateFarmingShopContent(string shopType)
    {



        Sprite corectSprite = getSpriteForCoinType(shopType);
        foreach (ADVANCEDNETWORKFUNCTIONS.RewardShopMassItem massItem in REFERENCENEXUS.Instance.networkbroadcaster.advancednetworkfunctions.purchaseableMassItemsFarming)
        {

            RewardShopItemUIAdjuster newContent = Instantiate(shopItemRewardPrefab, shopContent);
            newContent.dailyquestcanvas = this;
            rewardShopItems.Add(newContent);
            newContent.SetUpComponent(massItem.massItem.itemName, null, massItem.price, shopType, null, corectSprite);

        }
        foreach (ADVANCEDNETWORKFUNCTIONS.RewardShopUniqueItem uniqeItem in REFERENCENEXUS.Instance.networkbroadcaster.advancednetworkfunctions.purchaseableUniqueItemsFarming)
        {

            RewardShopItemUIAdjuster newRewards = Instantiate(shopItemRewardPrefab, shopContent);

            newRewards.dailyquestcanvas = this;
            rewardShopItems.Add(newRewards);
            LOCALDATABASEMANAGER.SerializableUniqueItem newSerializable = REFERENCENEXUS.Instance.ConvertUniqueToSerializableUniqueItem(uniqeItem.uniqueItem);

            newRewards.SetUpComponent(uniqeItem.uniqueItem.ItemName, newSerializable, uniqeItem.price, shopType, uniqeItem.uniqueItem, corectSprite);

        }

    }
    
    public void PopulateCookingShopContent(string shopType)
    {


        Sprite corectSprite = getSpriteForCoinType(shopType);
        foreach (ADVANCEDNETWORKFUNCTIONS.RewardShopMassItem massItem in REFERENCENEXUS.Instance.networkbroadcaster.advancednetworkfunctions.purchaseableMassItemsCooking)
        {

            RewardShopItemUIAdjuster newContent = Instantiate(shopItemRewardPrefab, shopContent);

            newContent.dailyquestcanvas = this;
            rewardShopItems.Add(newContent);
            newContent.SetUpComponent(massItem.massItem.itemName, null, massItem.price, shopType, null, corectSprite);

        }
        foreach (ADVANCEDNETWORKFUNCTIONS.RewardShopUniqueItem uniqeItem in REFERENCENEXUS.Instance.networkbroadcaster.advancednetworkfunctions.purchaseableUniqueItemsCooking)
        {

            RewardShopItemUIAdjuster newRewards = Instantiate(shopItemRewardPrefab, shopContent);

            newRewards.dailyquestcanvas = this;
            rewardShopItems.Add(newRewards);
            LOCALDATABASEMANAGER.SerializableUniqueItem newSerializable = REFERENCENEXUS.Instance.ConvertUniqueToSerializableUniqueItem(uniqeItem.uniqueItem);

            newRewards.SetUpComponent(uniqeItem.uniqueItem.ItemName, newSerializable, uniqeItem.price, shopType, uniqeItem.uniqueItem, corectSprite);

        }


    }


    public void OpenRewardsShopPanel()
    {


        EndCountdownGraceful();
        REFERENCENEXUS.Instance.menuManager.OnClosedMenu(dailiesbag);
        REFERENCENEXUS.Instance.menuManager.OnOpenedMenu(rewardsShopPanel);


        REFERENCENEXUS.Instance.EnableCursor();

        string lastShopType = PlayerPrefs.GetString("REWARDSSHOP", "CULINARYSTARMEDAL");

        switch (lastShopType) {

            case "CULINARYSTARMEDAL":
                PlayerPressedRewardsShopCookingTab();
                break;
            case "HARVESTCRESTMEDAL":

                PlayerPressedRewardsShopFarmingTab();
                break;
            case "ANGLERSTRIUMPHMEDAL":

                PlayerPressedRewardsShopFishingTab();
                break;
            default:

                PlayerPressedRewardsShopFishingTab();
                break;




        }




        EnsureRewardsMedalsHaveTooltips();


        if (lastMarket == null)
        {
            return;
        }
        REFERENCENEXUS.Instance.TrackPositionFrom(lastMarket, rewardsShopPanel, "DailyRewardsShop");
    }

    public void SwitchFromRewardsPanelToDailies()
    {


        REFERENCENEXUS.Instance.menuManager.OnClosedMenu(rewardsShopPanel);
        REFERENCENEXUS.Instance.menuManager.OnOpenedMenu(dailiesbag);

        REFERENCENEXUS.Instance.EnableCursor();
    }

    void Start()
    {


        REFERENCENEXUS.Instance.dailyquestcanvas = this;

    }

    public GameObject submitCookingDailiesButton;  // if the player has not completed this daily and has the correct items in their inventory, we'll show this
    public GameObject submitFarmingDailiesButton; // if the player has not completed this daily and has the correct items in their inventory, we'll show this
    public GameObject submitFishingDailiesButton; // if the player has not completed this daily and has the correct items in their inventory, we'll show this

    public GameObject greenCheckMarkCookingDailies; // if the player is done the daily , this will be shown
    public GameObject greenCheckMarkFarmingDailies;// if the player is done the daily , this will be shown
    public GameObject greenCheckMarkFishingDailies;// if the player is done the daily , this will be shown

    public GameObject rewardsBagCooking;  // show either way
    public GameObject rewardsBagFarming;  // show either way
    public GameObject rewardsBagFishing;  // show either way

    public TextMeshProUGUI nextDailiesTimer;  // so the player knows when the reset will be 


    public Transform cookingDailiesItemContent; // we'll populate this with a generic menu prefab set up to represent the item
    public Transform farmingDailiesItemContent; // we'll populate this with a generic menu prefab set up to represent the item
    public Transform fishingDailiesItemContent; // we'll populate this with a generic menu prefab set up to represent the item


    public TextMeshProUGUI medalCountCookingDailies; // this will show how number of medals they get for this turn in
    public TextMeshProUGUI medalCountFarmingDailies;// this will show how number of medals they get for this turn in
    public TextMeshProUGUI medalCountFishingDailies;// this will show how number of medals they get for this turn in

    public TextMeshProUGUI requiredCountCookingDailies; // how many items are required to complete cooking dailies
    public TextMeshProUGUI requiredCountFarmingDailies;// how many items are required to complete farming dailies
    public TextMeshProUGUI requiredCountFishingDailies;// how many items are required to complete fishing dailies


    public void ScrubDailiesContent()
    {

        foreach(Transform child in cookingDailiesItemContent)
        {
            Destroy(child.gameObject);
        }
        foreach(Transform child in farmingDailiesItemContent)
        {
            Destroy(child.gameObject);
        }
        foreach(Transform child in fishingDailiesItemContent)
        {
            Destroy(child.gameObject);
        }
    }


    public void RequestCurrentDailiesFromServer() 
    {
 

        REFERENCENEXUS.Instance.networkbroadcaster.advancednetworkfunctions.RequestCurrentDailiesFromServer();

    }

    private string currentCookingDailyTurnIn;
    private string currentFishingDailyTurnIn;
    private string currentFarmingDailyTurnIn;
    public void ReceiveCurrentDailiesFromServer(DailyQuestDetails[] dailyQuestDetails, PlayerDailyProgress playerDailyProgress, DateTime nextResetTime, DateTime utcNow)
    {

        if (!dailiesbag.activeInHierarchy)
        {
            return;
        }
        menuDim.SetActive(false);
        ScrubDailiesContent();

   
        AddItemToContentHolder(dailyQuestDetails[0].itemName, cookingDailiesItemContent);
        medalCountCookingDailies.text = $"{dailyQuestDetails[0].medalReward}";
        requiredCountCookingDailies.text = $"{dailyQuestDetails[0].requiredQuantity}";
        submitCookingDailiesButton.SetActive(playerDailyProgress.CookingTaskStatus != "Completed" && InventoryContains(dailyQuestDetails[0].itemName, dailyQuestDetails[0].requiredQuantity));
        greenCheckMarkCookingDailies.SetActive(playerDailyProgress.CookingTaskStatus == "Completed");

        currentCookingDailyTurnIn = dailyQuestDetails[0].itemName;
  
        AddItemToContentHolder(dailyQuestDetails[1].itemName, farmingDailiesItemContent);
        medalCountFarmingDailies.text = $"{dailyQuestDetails[1].medalReward}";
        requiredCountFarmingDailies.text = $"{dailyQuestDetails[1].requiredQuantity}";
        submitFarmingDailiesButton.SetActive(playerDailyProgress.FarmingTaskStatus != "Completed" && InventoryContains(dailyQuestDetails[1].itemName, dailyQuestDetails[1].requiredQuantity));
        greenCheckMarkFarmingDailies.SetActive(playerDailyProgress.FarmingTaskStatus == "Completed");

        currentFarmingDailyTurnIn = dailyQuestDetails[1].itemName;

 
        AddItemToContentHolder(dailyQuestDetails[2].itemName, fishingDailiesItemContent);
        medalCountFishingDailies.text = $"{dailyQuestDetails[2].medalReward}";
        requiredCountFishingDailies.text = $"{dailyQuestDetails[2].requiredQuantity}";
        submitFishingDailiesButton.SetActive(playerDailyProgress.FishingTaskStatus != "Completed" && InventoryContains(dailyQuestDetails[2].itemName, dailyQuestDetails[2].requiredQuantity));
        greenCheckMarkFishingDailies.SetActive(playerDailyProgress.FishingTaskStatus == "Completed");

        currentFishingDailyTurnIn = dailyQuestDetails[2].itemName;

        rewardsBagCooking.SetActive(true);
        rewardsBagFarming.SetActive(true);
        rewardsBagFishing.SetActive(true);

        // Update reset timer display
        UpdateDailiesResetTimer(nextResetTime, utcNow);

        // Additional checks or functionalities
        EnsureMedalsHaveTooltips();
    }

    // Helper method to check if the inventory contains the required item in necessary quantity
    private bool InventoryContains(string itemName, int requiredQuantity)
    {
        // This logic assumes you have an 'inventoryObjects' collection accessible from this context
        foreach (PlayerInventory.InventoryObject inventoryObject in REFERENCENEXUS.Instance.gsc.realLocalInventory.inventoryObjects)
        {
            if (inventoryObject.item.itemName == itemName && inventoryObject.amount >= requiredQuantity)
            {
                return true;
            }
        }
        return false;
    }
    public void CheckPlayerInventoryForQuestItems(DailyQuestDetails[] dailyQuestDetails)
    {
    
        // Cooking Dailies Check
        bool hasEnoughCookingItems = InventoryContainsAmount(dailyQuestDetails[0].itemName, dailyQuestDetails[0].requiredQuantity);
        submitCookingDailiesButton.SetActive(hasEnoughCookingItems);

        

        // Farming Dailies Check
        bool hasEnoughFarmingItems = InventoryContainsAmount(dailyQuestDetails[1].itemName, dailyQuestDetails[1].requiredQuantity);
        submitFarmingDailiesButton.SetActive(hasEnoughFarmingItems);

        // Fishing Dailies Check
        bool hasEnoughFishingItems = InventoryContainsAmount(dailyQuestDetails[2].itemName, dailyQuestDetails[2].requiredQuantity);
        submitFishingDailiesButton.SetActive(hasEnoughFishingItems);
    }
    // Enhanced method to check for a specific item and its quantity in the inventory
    public bool InventoryContainsAmount(string itemName, int requiredQuantity)
    {
        foreach (PlayerInventory.InventoryObject inventoryObject in REFERENCENEXUS.Instance.gsc.realLocalInventory.inventoryObjects)
        {
            if (inventoryObject.item.itemName == itemName && inventoryObject.amount >= requiredQuantity)
            {
                return true;
            }
        }
        return false;
    }

    public bool GaveMedalsTools = false;
    public bool GaveRewardsMedalsTools = false;

    private Coroutine countdownCoroutine;  // To keep a reference to the countdown coroutine
    private DateTime nextResetTime;  // To store the next reset time

    // Call this method to update the next dailies reset timer and start the countdown
    public void UpdateDailiesResetTimer(DateTime resetTime, DateTime serverutcNow)
    {
    
        // Validate if the client's current UTC time aligns with what server calculates
       DateTime clientNow = DateTime.UtcNow;
    
   
        // Store the next reset time
        nextResetTime = resetTime;

        // Log the comparison between now and next reset
        TimeSpan timeUntilReset = nextResetTime - serverutcNow;
  
        // If there's an existing countdown coroutine, stop it
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }

        // Start a new countdown coroutine
        countdownCoroutine = StartCoroutine(CountdownToNextReset(nextResetTime, serverutcNow));
     
    }

    private void EndCountdownGraceful()
    {


        // If there's an existing countdown coroutine, stop it
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
       
        }
    }
    // Coroutine to update the countdown every second
    private IEnumerator CountdownToNextReset(DateTime serverNextResetTime, DateTime serverCurrentTime)
    {
  

        // Get the client's start time for the coroutine
        DateTime clientStartTime = DateTime.UtcNow;
        while (true)
        {
            // Calculate the elapsed time on the client since the coroutine started
            TimeSpan clientElapsedTime = DateTime.UtcNow - clientStartTime;

            // Assume the same amount of time has passed on the server
            DateTime assumedServerCurrentTime = serverCurrentTime + clientElapsedTime;

            // Now calculate the remaining time until the reset based on the server's next reset time
            TimeSpan timeUntilReset = serverNextResetTime - assumedServerCurrentTime;

            // Break the loop if there is no time left
            if (timeUntilReset <= TimeSpan.Zero)
            {
                break;
            }

            // Update the display timer on the HUD
            nextDailiesTimer.text = $"{timeUntilReset.Hours}h {timeUntilReset.Minutes}m {timeUntilReset.Seconds}s";
      
            // Wait for one second
            yield return new WaitForSeconds(1);
        }

        // Once the countdown is complete, update the display to show 0 and request new dailies
        nextDailiesTimer.text = "0h 0m 0s";
   
        // Request new dailies information from the server
        RequestCurrentDailiesFromServer();
    }

    public void EnsureMedalsHaveTooltips()
    {
        if (GaveMedalsTools)
        {
            return;
        }
        InventoryItemTooltipHandler cookingMedalHandler = rewardsImageCooking.GetComponent<InventoryItemTooltipHandler>();

        if (cookingMedalHandler == null)
        {
            InventoryItemTooltipHandler inventoryItemTooltipHandler = rewardsImageCooking.AddComponent<InventoryItemTooltipHandler>();
            inventoryItemTooltipHandler.itemname = "CULINARYSTARMEDAL";
            inventoryItemTooltipHandler.tooltipmanager = REFERENCENEXUS.Instance.tooltipmanager;
            inventoryItemTooltipHandler.dontAnchor = true;
        }
        
        InventoryItemTooltipHandler fishingMedalHandler = rewardsImageFishing.GetComponent<InventoryItemTooltipHandler>();

        if (fishingMedalHandler == null)
        {
            InventoryItemTooltipHandler inventoryItemTooltipHandler = rewardsImageFishing.AddComponent<InventoryItemTooltipHandler>();
            inventoryItemTooltipHandler.itemname = "ANGLERSTRIUMPHMEDAL";
            inventoryItemTooltipHandler.tooltipmanager = REFERENCENEXUS.Instance.tooltipmanager;
            inventoryItemTooltipHandler.dontAnchor = true;
        }
        
        InventoryItemTooltipHandler farmingMedalHandler = rewardsImageFarming.GetComponent<InventoryItemTooltipHandler>();

        if (farmingMedalHandler == null)
        {
            InventoryItemTooltipHandler inventoryItemTooltipHandler = rewardsImageFarming.AddComponent<InventoryItemTooltipHandler>();
            inventoryItemTooltipHandler.itemname = "HARVESTCRESTMEDAL";
            inventoryItemTooltipHandler.tooltipmanager = REFERENCENEXUS.Instance.tooltipmanager;
            inventoryItemTooltipHandler.dontAnchor = true;
        }
        GaveMedalsTools = true;
    }
    
    public void EnsureRewardsMedalsHaveTooltips()
    {
        if (GaveRewardsMedalsTools)
        {
            return;
        }
        InventoryItemTooltipHandler cookingMedalHandler = cookingMedalOwnedImage.GetComponent<InventoryItemTooltipHandler>();

        if (cookingMedalHandler == null)
        {
            InventoryItemTooltipHandler inventoryItemTooltipHandler = cookingMedalOwnedImage.AddComponent<InventoryItemTooltipHandler>();
            inventoryItemTooltipHandler.itemname = "CULINARYSTARMEDAL";
            inventoryItemTooltipHandler.tooltipmanager = REFERENCENEXUS.Instance.tooltipmanager;
            inventoryItemTooltipHandler.dontAnchor = true;
        }
        
        InventoryItemTooltipHandler fishingMedalHandler = fishingMedalOwnedImage.GetComponent<InventoryItemTooltipHandler>();

        if (fishingMedalHandler == null)
        {
            InventoryItemTooltipHandler inventoryItemTooltipHandler = fishingMedalOwnedImage.AddComponent<InventoryItemTooltipHandler>();
            inventoryItemTooltipHandler.itemname = "ANGLERSTRIUMPHMEDAL";
            inventoryItemTooltipHandler.tooltipmanager = REFERENCENEXUS.Instance.tooltipmanager;
            inventoryItemTooltipHandler.dontAnchor = true;
        }
        
        InventoryItemTooltipHandler farmingMedalHandler = farmingMedalOwnedImage.GetComponent<InventoryItemTooltipHandler>();

        if (farmingMedalHandler == null)
        {
            InventoryItemTooltipHandler inventoryItemTooltipHandler = farmingMedalOwnedImage.AddComponent<InventoryItemTooltipHandler>();
            inventoryItemTooltipHandler.itemname = "HARVESTCRESTMEDAL";
            inventoryItemTooltipHandler.tooltipmanager = REFERENCENEXUS.Instance.tooltipmanager;
            inventoryItemTooltipHandler.dontAnchor = true;
        }
        GaveRewardsMedalsTools = true;
    }

    public GameObject rewardsImageCooking;
    public GameObject rewardsImageFishing;
    public GameObject rewardsImageFarming;

   
    public void ResetCompletely()
    {
        lastMarket = null;
        rewardShopItems.Clear();
        dailiesbag.SetActive(false);

        menuDim.SetActive(false);

        currentCookingDailyTurnIn = "";
        currentFishingDailyTurnIn = "";
        currentFarmingDailyTurnIn = "";



        lastClickedItemName = "";
        lastClickedSerializableUnique = null;
        lastClickedUniqueItem = null;

        purchaseResponseFail.SetActive(false);
        purchaseResponseSuccess.SetActive(false);
    }

    public GameObject menuDim;

    public void OpenDailies(Transform market)
    {

        menuDim.SetActive(true);

        REFERENCENEXUS.Instance.menuManager.OnOpenedMenu(dailiesbag);


        RequestCurrentDailiesFromServer();

        lastMarket = market;
        REFERENCENEXUS.Instance.TrackPositionFrom(market, dailiesbag, "DailyQuests");
    }

    public Transform lastMarket;

    public void CloseDailies()
    {

        EndCountdownGraceful();
        REFERENCENEXUS.Instance.menuManager.OnClosedMenu(dailiesbag);
        REFERENCENEXUS.Instance.menuManager.DisableCursorIfNoMenus();
    }
    
    public void CloseRewards()
    {

        REFERENCENEXUS.Instance.menuManager.OnClosedMenu(rewardsShopPanel);
        REFERENCENEXUS.Instance.menuManager.DisableCursorIfNoMenus();
    }


    public void AddItemToContentHolder(string itemName, Transform contentHolder)
    {

        GameObject obj = Instantiate(REFERENCENEXUS.Instance.otherinventory.standardizedMenuPrefab, contentHolder);
        InventoryItemTooltipHandler inventoryItemTooltipHandler = obj.AddComponent<InventoryItemTooltipHandler>();
        inventoryItemTooltipHandler.itemname = itemName;
        inventoryItemTooltipHandler.tooltipmanager = REFERENCENEXUS.Instance.tooltipmanager;
        inventoryItemTooltipHandler.dontAnchor = true;
        ItemGuideClient theItemGuide = REFERENCENEXUS.Instance.clientPossibleObjectInformation.getItemGuideFromString(itemName);


        if (theItemGuide == null)
        {
            return;
        }

        Image imageToAdjust = obj.gameObject.transform.GetChild(0).gameObject.GetComponent<Image>();


        imageToAdjust.sprite = theItemGuide.itemSprite;

        bool xbox = REFERENCENEXUS.Instance.isUsingXboxController;

        if (xbox)
        {

            REFERENCENEXUS.Instance.otherinventory.CleanObjectForXbox(obj);
        }
    }


    public void PlayerPressedSubmitCookingQuest() // player pressed button
    {

        if (REFERENCENEXUS.Instance.gsc.realLocalInventory.IsMassItemLocked(currentCookingDailyTurnIn))
        {
            REFERENCENEXUS.Instance.SendLocalMessage($"Can't turn in locked items {currentCookingDailyTurnIn}");
      
            return;
        }

        REFERENCENEXUS.Instance.networkbroadcaster.advancednetworkfunctions.RequestSubmitCookingQuest();

    }
    public void PlayerPressedSubmitFarmingQuest() // player pressed button
    {


        if (REFERENCENEXUS.Instance.gsc.realLocalInventory.IsMassItemLocked(currentFarmingDailyTurnIn))
        {
            REFERENCENEXUS.Instance.SendLocalMessage($"Can't turn in locked items {currentFarmingDailyTurnIn}");
  
            return;
        }
        REFERENCENEXUS.Instance.networkbroadcaster.advancednetworkfunctions.RequestSubmitFarmingQuest();
    }
    public void PlayerPressedSubmitFishingQuest() // player pressed button
    {

        if (REFERENCENEXUS.Instance.gsc.realLocalInventory.IsMassItemLocked(currentFishingDailyTurnIn))
        {
            REFERENCENEXUS.Instance.SendLocalMessage($"Can't turn in locked items {currentFishingDailyTurnIn}");
         
            return;
        }
        REFERENCENEXUS.Instance.networkbroadcaster.advancednetworkfunctions.RequestSubmitFishingQuest();

    }

}
