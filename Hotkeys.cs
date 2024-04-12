using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
using System;

public class Hotkeys : MonoBehaviour
{

    public void ResetComponent()
    {

        hotkeybag.SetActive(false);
        initializeHotkeys();
        scrubContent();
        loaded = false;
    }


    public GameObject hotkeybag;



    public GameObject hotkeyWindow;
    public bool loaded = false;
    public GameSetupController gsc;
    void initializeHotkeys()
    {
          hotkeyedItems.Clear();
        hotkeyedItems.Add(null);
        hotkeyedItems.Add(null);
        hotkeyedItems.Add(null);
        hotkeyedItems.Add(null);
        hotkeyedItems.Add(null);
        hotkeyedItems.Add(null);
        hotkeyedItems.Add(null);
        hotkeyedItems.Add(null);
        hotkeyedItems.Add(null);
        hotkeyedItems.Add(null);
        hotkeyedItems.Add(null);

  
    }

    public void SetUpHotkeysForXbox()
    {
        SetUpHotkeyForXbox(0, hotkeyContent.transform.GetChild(0).gameObject);
        SetUpHotkeyForXbox(1, hotkeyContent.transform.GetChild(1).gameObject);
        SetUpHotkeyForXbox(2, hotkeyContent.transform.GetChild(2).gameObject);
        SetUpHotkeyForXbox(3, hotkeyContent.transform.GetChild(3).gameObject);
        SetUpHotkeyForXbox(4, hotkeyContent.transform.GetChild(4).gameObject);
        SetUpHotkeyForXbox(5, hotkeyContent.transform.GetChild(5).gameObject);
        SetUpHotkeyForXbox(6, hotkeyContent.transform.GetChild(6).gameObject);
        SetUpHotkeyForXbox(7, hotkeyContent.transform.GetChild(7).gameObject);
        SetUpHotkeyForXbox(8, hotkeyContent.transform.GetChild(8).gameObject);
        SetUpHotkeyForXbox(9, hotkeyContent.transform.GetChild(9).gameObject);
        SetUpHotkeyForXbox(10, hotkeyContent.transform.GetChild(10).gameObject);

    }

    public void reset()
    {
        SetDefaultHotkeys();


    }

    public void PlayerLostUniqueItem(int whichUniqueID)
    {
        for (int i = 0; i < hotkeyedItems.Count; i++)
        {
            var hotkeyedItem = hotkeyedItems[i];
            if (hotkeyedItem != null && hotkeyedItem.IsUnique() && hotkeyedItem.UniqueId == whichUniqueID)
            {
                hotkeyedItems[i] = null;
            }
        }

        refreshContent();
    }

    public void loadHotkeys()
    {

            initializeHotkeys();
       
        if (string.IsNullOrEmpty(DBManager.username))
        {
   
            return;
        }

      if (!gameObject.activeInHierarchy)
        {
            return;
        }
  

        StartCoroutine(LoadHotkeysFromServer());
    }
    public void loadHotkeysOffline()
    {
      
            initializeHotkeys();
       
        if (string.IsNullOrEmpty(DBManager.username))
        {
    
            return;
        }

  
        StartCoroutine(LoadHotkeysFromServer());
    }
    public Transform hotkeyContent;

    public GameObject hotkeyPrefab;

    public void refreshContent()
    {

        scrubContent();
        populateContent();
        saveHotkeys();


    }
    public void refreshContentDONTSAVE()
    {

        scrubContent();
        populateContent();


    }

    public void saveHotkeys()
    {
        if (!loaded)
        {
            return;
        }
        StartCoroutine(SaveHotkeysToServer());
    }

    private IEnumerator SaveHotkeysToServer()
    {
        List<string> hotkeyData = new List<string>();
        foreach (var item in hotkeyedItems)
        {
            if (item == null)
            {
                hotkeyData.Add("NULL");
            }
            else
            {
                int uniqueId = item.UniqueId.HasValue ? item.UniqueId.Value : -1;
                string serializedItem = $"{item.ItemName}|{uniqueId}|{item.Quantity}";
                hotkeyData.Add(serializedItem);
            }
        }

        string hotkeysString = string.Join("/", hotkeyData);
        LOCALDATABASEMANAGER.Instance.SaveHotkeysToLocalDatabase(DBManager.username, hotkeysString);
        yield break;
    }

    public void SetDefaultHotkeys()
    {
 
        hotkeyedItems.Clear();
        hotkeyedItems.Add(null);
        hotkeyedItems.Add(null);
        hotkeyedItems.Add(null);
        hotkeyedItems.Add(null);
        hotkeyedItems.Add(null);
        hotkeyedItems.Add(null);
        hotkeyedItems.Add(null);
        hotkeyedItems.Add(null);
        hotkeyedItems.Add(null);
        hotkeyedItems.Add(null);
        hotkeyedItems.Add(null);

        refreshContentDONTSAVE();


    }

    private IEnumerator LoadHotkeysFromServer()
    {
     

            while (string.IsNullOrEmpty(DBManager.username))
            {
                yield return null;

            }
            string hotkeysString = LOCALDATABASEMANAGER.Instance.LoadHotkeysFromLocalDatabase(DBManager.username);



            ProcessHotkeysData(hotkeysString);
            yield break;
   
      
    }
    public void ProcessHotkeysData(string hotkeysString)
    {

        if (string.IsNullOrEmpty(hotkeysString))
        {
            SetDefaultHotkeys();
        }
        else
        {
            List<string> hotkeyData = new List<string>(hotkeysString.Split('/'));

            for (int i = 0; i < Math.Min(hotkeyData.Count, hotkeyedItems.Count); i++)
            {
                string data = hotkeyData[i];
                if (data == "NULL" || !data.Contains("|"))
                {
                    hotkeyedItems[i] = null;
                }
                else
                {
                    string[] parts = data.Split('|');
                    if (parts.Length == 3)
                    {
                        string itemName = parts[0];
                        int uniqueId = int.Parse(parts[1]);
                        int quantity = int.Parse(parts[2]);

                        if (uniqueId == -1)
                        {
                            hotkeyedItems[i] = new ClassForStoringHotkeyItems(itemName, quantity);
                        }
                        else
                        {
                            LOCALDATABASEMANAGER.SerializableUniqueItem uniqueItem = new LOCALDATABASEMANAGER.SerializableUniqueItem
                            {
                                ItemName = itemName,
                                UniqueId = uniqueId,
                            };
                            hotkeyedItems[i] = new ClassForStoringHotkeyItems(uniqueItem);
                        }
                    }
                    else
                    {
                        hotkeyedItems[i] = null; // Handle unexpected format
                    }
                }
            }

            refreshContentDONTSAVE();
        }


        loaded = true;
        REFERENCENEXUS.Instance.fragmentUnifier.OnHotkeysLoaded();
    }

    public POSSIBLELOOT possibleloot;
    public ItemGuideClient GetItemGuideFromString(string itemtype)
    {

        ItemGuideClient returner = null;


        foreach (ItemGuideClient possibleInventoryObject in possibleloot.clientPossibleObjectInformation.itemGuides)
        {
            if (possibleInventoryObject.itemName == itemtype)
            {

                returner = possibleInventoryObject;

            }

        }



        return returner;


    }

    public List<ClassForStoringHotkeyItems> HotkeyedItems = new List<ClassForStoringHotkeyItems>();
    public void scrubContent()
    {

        foreach (Transform child in hotkeyContent)
        {
            Destroy(child.gameObject);
        }


    }

    public void AssignInformationToHotkey(GameObject prefab, int which)
    {

        prefab.transform.GetChild(0).gameObject.GetComponent<HotkeySlotComponent>().hotKeySlotID = which - 1;
        if (which == 10)
        {
            which = 0;
        }

        string numbertouse = which.ToString();

        if (which == 11)
        {
            numbertouse = "Q";
        }
 
        prefab.transform.GetChild(1).gameObject.GetComponent<Image>().enabled = false;
        prefab.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = numbertouse;



    }

    public GameObject RLP; 


    public void AssignInformationToHotkeyWithSprite(GameObject prefab, int which, ItemGuideClient itemm, bool isUnique, int? uniqueID)
    {

        HotkeySlotComponent hsComponent = prefab.transform.GetChild(0).gameObject.GetComponent<HotkeySlotComponent>();

        hsComponent.hotKeySlotID = which - 1;
        hsComponent.hotkeyedItemname = itemm.itemName;
        hsComponent.isUnique = isUnique;
        hsComponent.uniqueID = uniqueID;
    
        
        
        if (which == 10)
        {
            which = 0;
        }
        string numbertouse = which.ToString();

        if (which == 11)
        {
            numbertouse = "Q";
        }
        if (RLP == null)
        {
            RLP = gsc.realLocalPlayer;


        }
        Sprite correctsprite = null;
        if (RLP == null)
        {
     
        } else
        {
            correctsprite = RLP.GetComponent<PlayerInventory>().getSpriteByItemGuide(itemm);

        }

        if (correctsprite != null) { 

        prefab.transform.GetChild(1).gameObject.GetComponent<Image>().sprite = correctsprite;
        prefab.transform.GetChild(1).gameObject.GetComponent<Image>().enabled = true;
        prefab.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = numbertouse;

        } else
        {
            prefab.transform.GetChild(1).gameObject.GetComponent<Image>().sprite = null;
            prefab.transform.GetChild(1).gameObject.GetComponent<Image>().enabled = false;
            prefab.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = numbertouse;

        }
    }



    public void populateContent()
    {
        int counter = 1;
        foreach (ClassForStoringHotkeyItems hotkeyItem in hotkeyedItems)
        {
            GameObject newPrefab = Instantiate(hotkeyPrefab, hotkeyContent);




            if (hotkeyItem == null)
            {
                AssignInformationToHotkey(newPrefab, counter);
            }
            else
            {

                if (hotkeyItem.IsUnique())
                {

         

                    int? hotkeyUniques = hotkeyItem.UniqueId;

                    if (hotkeyUniques == null)
                    {

             
                        AssignInformationToHotkey(newPrefab, counter);
                        HandleXboxSupport(newPrefab, counter);
                        continue;
                    }
                    int uniqueHotkeyID = (int)hotkeyItem.UniqueId;

                    LOCALDATABASEMANAGER.SerializableUniqueItem uniqueItem = REFERENCENEXUS.Instance.gsc.realLocalInventory.GetUniqueItemById(uniqueHotkeyID);

                    if (uniqueItem == null)
                    {
              
                        AssignInformationToHotkey(newPrefab, counter);
                        HandleXboxSupport(newPrefab, counter);
                        counter++;
                        continue;

                    } else { 

                   

                        ItemGuideClient itemGuide = REFERENCENEXUS.Instance.clientPossibleObjectInformation.getItemGuideFromString(uniqueItem.ItemName);

                        if (itemGuide != null)
                        {
                            AssignInformationToHotkeyWithSprite(newPrefab, counter, itemGuide, hotkeyItem.IsUnique(), hotkeyItem.UniqueId);
                            REFERENCENEXUS.Instance.otherinventory.AddDragEventTriggersFromHotkeys(newPrefab, itemGuide.itemName);
                        }
                   

                    }




                }
                else
                {






                    ItemGuideClient itemGuide = REFERENCENEXUS.Instance.clientPossibleObjectInformation.getItemGuideFromString(hotkeyItem.ItemName);

                    if (itemGuide != null)
                    {
                        assignInformationToHotkeyWithSprite(newPrefab, counter, itemGuide, hotkeyItem.IsUnique(), hotkeyItem.UniqueId);
                        REFERENCENEXUS.Instance.otherinventory.AddDragEventTriggersFromHotkeys(newPrefab, itemGuide.itemName);
                    }
                    
                }
            }

            HandleXboxSupport(newPrefab, counter);
            counter++;
        }


    }

  
    public void notifyRanOut(ItemGuideClient item)
    {
        for (int i = 0; i < hotkeyedItems.Count; i++)
        {
            ClassForStoringHotkeyItems hotkeyItem = hotkeyedItems[i];

            if (hotkeyItem != null && hotkeyItem.ItemName == item.itemName && !hotkeyItem.IsUnique())
            {
                hotkeyedItems[i] = null;
            }
        }

        refreshContent();
    }

    public void HandleXboxSupport(GameObject hotkeyprefab, int which)
{
    bool xboxsupportrequired = REFERENCENEXUS.Instance.isUsingXboxController;

    if (!xboxsupportrequired)
    {

        return;
    }

    which--;
    SetUpHotkeyForXbox(which, hotkeyprefab);
}

public void SetUpHotkeyForXbox(int whichtype, GameObject hotkeyprefab)
{

    GameObject dpadbag = hotkeyprefab.gameObject.transform.GetChild(3).gameObject;
    dpadbag.SetActive(true);

    Transform dpadbagtransform = dpadbag.transform;



    dpadbagtransform.gameObject.transform.GetChild(whichtype).gameObject.SetActive(true);


    dpadbag.name = dpadbag.name + whichtype;
}



[Serializable]
public class ClassForStoringHotkeyItems // needs to store and retrieve hotkey info for both mass items and uniques
{
    public string ItemName;
    public int? UniqueId; 
    public int Quantity; 


    public ClassForStoringHotkeyItems(LOCALDATABASEMANAGER.SerializableUniqueItem uniqueItem)
    {
        this.ItemName = uniqueItem.ItemName;
        this.UniqueId = uniqueItem.UniqueId;
        this.Quantity = 1; 
    }

    public ClassForStoringHotkeyItems(string itemName, int quantity)
    {
        this.ItemName = itemName;
        this.UniqueId = null;
        this.Quantity = quantity;
    }

    public bool IsUnique() => this.UniqueId.HasValue;
}



}
