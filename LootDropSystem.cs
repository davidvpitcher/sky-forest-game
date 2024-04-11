using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Connection;
using System.Linq;

public class LootDropSystem : NetworkBehaviour
{

    public List<Transform> possibleChestSpawnLocations = new List<Transform>();

    public MassItemLootTable[] massItemLootTablesTierOne;
    public MassItemLootTable[] massItemLootTablesTierTwo;
    public MassItemLootTable[] massItemLootTablesTierThree;
    public MassItemLootTable[] massItemLootTablesTierFour;
    public MassItemLootTable[] massItemLootTablesTierFive;
    public MassItemLootTable[] massItemLootTablesTierSix;
    public UniqueItemLootTable[] uniqueItemLootTablesOne;
    public UniqueItemLootTable[] uniqueItemLootTablesTwo;
    public UniqueItemLootTable[] uniqueItemLootTablesThree;
    public UniqueItemLootTable[] uniqueItemLootTablesFour;
    public UniqueItemLootTable[] uniqueItemLootTablesFive;
    public UniqueItemLootTable[] uniqueItemLootTablesSix;


    private int uniqueItemCounter = 0; 

    public class SerializedUniqueFromTreasureChest
    {
        public int TransientUniqueID { get; set; }  
        public string ItemName;
        public string PropString1;
        public string PropString2;
        public string PropString3;
        public int PropInt1;
        public int PropInt2;
        public int PropInt3;
    }

    [System.Serializable]
    public class MassItemLootTable
    {
        public string itemName;
        public int minQuantity;
        public int maxQuantity;
    }
    [System.Serializable]
    public class MassItem
    {
        public string itemName;
        public int quantity;  

        public MassItem(string itemnames, int quantities)
        {

            this.itemName = itemnames;
            this.quantity = quantities;
        }
        public MassItem()
        {
        }
    }
    [System.Serializable]
    public class UniqueItemLootTable
    {
        public UniqueItem uniqueItemTemplate;
    }

    public ChestInfo GetChestContents(string uniqueChestID)
    {
        return activeChests.FirstOrDefault(chest => chest.UniqueID == uniqueChestID);
    }


    public class ChestInfo
    {
        public string UniqueID { get; set; }
        public int Tier { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public List<MassItem> MassItems { get; set; }
        public List<SerializedUniqueFromTreasureChest> SerializedUniqueItems { get; set; }
        public DateTime DespawnTime { get; set; }
    }


    [SerializeField] private int randomMassItemCount = 5; 
    [SerializeField] private float uniqueItemProbability = 0.1f; 

    private List<ChestInfo> activeChests = new List<ChestInfo>();
    public List<ChestInfo> GetCurrentChestInfo() 
    {
        return activeChests;
    }

    public int GetTotalActiveChests()
    {
        return activeChests.Count;
    }
    public void OnChestEmptied(string ChestID)
    {

     
        var chest = activeChests.FirstOrDefault(chest => chest.UniqueID == ChestID);
        if (chest != null)
        { 
            
            
            if (chestLocationMapping.TryGetValue(ChestID, out int locationIndex))
            {
                occupiedLocations.Remove(locationIndex);
                chestLocationMapping.Remove(ChestID);  
            }


            activeChests.Remove(chest);


            REFERENCENEXUS.Instance.networkbroadcaster.InformObserversChestEmptied(ChestID);

        
        }
        else
        {
            Debug.LogError($"Failed to find the chest with ID {ChestID} to mark as emptied.");
        }
    }
    public void ResetComponent()
    {

        activeChests.Clear();
        chestLocationMapping.Clear();


    }
    private void GenerateChestContents(ChestInfo chestInfo)
    {
        if (chestInfo == null)
        {
            Debug.LogError("failed to generate chest contnets");
            return;
        }
        int tier = chestInfo.Tier;
        MassItemLootTable[] selectedMassLootTables;
        UniqueItemLootTable[] selectedUniqueLootTables;
        switch (tier)
        {
            case 1:
                selectedMassLootTables = massItemLootTablesTierOne;
                selectedUniqueLootTables = uniqueItemLootTablesOne;
                break;
            case 2:
                selectedMassLootTables = massItemLootTablesTierTwo;
                selectedUniqueLootTables = uniqueItemLootTablesTwo;
                break;
            case 3:
                selectedMassLootTables = massItemLootTablesTierThree;
                selectedUniqueLootTables = uniqueItemLootTablesThree;
                break;
            case 4:
                selectedMassLootTables = massItemLootTablesTierFour;
                selectedUniqueLootTables = uniqueItemLootTablesFour;
                break;
            case 5:
                selectedMassLootTables = massItemLootTablesTierFive;
                selectedUniqueLootTables = uniqueItemLootTablesFive;
                break;
            case 6:
                selectedMassLootTables = massItemLootTablesTierSix;
                selectedUniqueLootTables = uniqueItemLootTablesSix;
                break;
            default:
                selectedMassLootTables = massItemLootTablesTierOne;
                selectedUniqueLootTables = uniqueItemLootTablesOne;
                break;
        }
    
        int itemCount = GetRandomItemCount();

        chestInfo.MassItems = GenerateRandomMassItems(selectedMassLootTables, itemCount);


        chestInfo.SerializedUniqueItems = new List<SerializedUniqueFromTreasureChest>();
        if (UnityEngine.Random.value < uniqueItemProbability && selectedUniqueLootTables.Any())
        {
            var index = UnityEngine.Random.Range(0, selectedUniqueLootTables.Length);
            var selectedTable = selectedUniqueLootTables[index];

            if (selectedTable?.uniqueItemTemplate != null)
            {
                SerializedUniqueFromTreasureChest serializedItem = new SerializedUniqueFromTreasureChest
                {
                    TransientUniqueID = ++uniqueItemCounter,  // Assign and increment the counter
                    ItemName = selectedTable.uniqueItemTemplate.ItemName,
                    PropString1 = selectedTable.uniqueItemTemplate.PropString1,
                    PropString2 = selectedTable.uniqueItemTemplate.PropString2,
                    PropString3 = selectedTable.uniqueItemTemplate.PropString3,
                    PropInt1 = selectedTable.uniqueItemTemplate.PropInt1,
                    PropInt2 = selectedTable.uniqueItemTemplate.PropInt2,
                    PropInt3 = selectedTable.uniqueItemTemplate.PropInt3
                };
                chestInfo.SerializedUniqueItems.Add(serializedItem);

                Debug.LogError("ADDED SERIALIZED UNIQUE TO CHEST");
            }
        }
    }
    private int GetRandomItemCount()
    {
        int randomValue = UnityEngine.Random.Range(1, 101); // Random value between 1 and 100
        if (randomValue <= 50) return 2; // 50% chance
        if (randomValue <= 75) return 3; // 25% chance
        if (randomValue <= 90) return 4; // 15% chance
        return 5; // 10% chance
    }

    private List<MassItem> GenerateRandomMassItems(MassItemLootTable[] lootTables, int itemCount)
    {
        List<MassItem> items = new List<MassItem>();
        HashSet<string> selectedItems = new HashSet<string>(); // To track which items have already been selected

        while (items.Count < itemCount)
        {
            var tableEntry = lootTables[UnityEngine.Random.Range(0, lootTables.Length)];

            // Check if the item has already been selected, skip if it has
            if (selectedItems.Contains(tableEntry.itemName))
            {
                continue;
            }

            var quantity = UnityEngine.Random.Range(tableEntry.minQuantity, tableEntry.maxQuantity + 1);
            items.Add(new MassItem { itemName = tableEntry.itemName, quantity = quantity });

            // Mark this item as selected to prevent duplicates
            selectedItems.Add(tableEntry.itemName);

            // Check to ensure we don't loop indefinitely in case itemCount exceeds unique loot table entries
            if (selectedItems.Count >= lootTables.Length) break;
        }

        return items;
    }

    IEnumerator SpawnChestAfterDelay(ChestInfo chestInfo)
    {
        // Wait for a random time between 0 and 30 seconds.
        yield return new WaitForSeconds(UnityEngine.Random.Range(0, 30));



        GenerateChestContents(chestInfo);

        // Store chest information in a server-side collection.
        activeChests.Add(chestInfo); 

        // Notify clients to spawn chest visuals at the right location.
        NotifyClientsToSpawnChest(chestInfo); 
    }

    [ObserversRpc(BufferLast = false, ExcludeOwner = false)]
    public void NotifyClientsToSpawnChest(ChestInfo chestInfo)
    {



        REFERENCENEXUS.Instance.clientlootdropsystem.ReceiveChestInfo(chestInfo);


    }



    public void RequestCurrentChestStateForNewPlayerSERVER(NetworkConnection requester)
    {

        if (REFERENCENEXUS.Instance.DontAlwaysRunChests)
        {
            if (activeChests.Count == 0)
            {

                if (!ChestRoutineWasUsed)
                {
                    if (REFERENCENEXUS.Instance.timemaster.IsItNighttime())
                    {





                        SpawnChestsInstantly();
                        ChestRoutineWasUsed = true;

                        return;
                    }
              


                }
            


            } 
        }


        REFERENCENEXUS.Instance.ValidateConnection(requester);


        foreach (ChestInfo chestInfo in activeChests)
        {
            if (chestInfo == null)
            {
                Debug.LogError("Error: An active chest is null.");
                continue;
            }

            ReceiveCurrentChestStateForNewPlayerTARGET(requester, chestInfo);
        }
    }



    [TargetRpc]
    public void ReceiveCurrentChestStateForNewPlayerTARGET(NetworkConnection requester, ChestInfo chestInfo)
    {

        REFERENCENEXUS.Instance.clientlootdropsystem.ReceiveChestInfo(chestInfo);

    }
    private HashSet<int> occupiedLocations = new HashSet<int>();


    public void SpawnChests()
    {

        for (int i = 0; i < 3; i++)
        {
            int tier = GetRandomTierWithWeights();
            ChestInfo chestInfo = CreateChestInfo(tier);

            if (chestInfo != null)
            {

                StartCoroutine(SpawnChestAfterDelay(chestInfo));
            }
        }
    }
    public void SpawnChestsInstantly()
    {

        for (int i = 0; i < 2; i++)
        {
            int tier = GetRandomTierWithWeights();
            ChestInfo chestInfo = CreateChestInfo(tier);

            if (chestInfo != null)
            {

                GenerateChestContents(chestInfo);

                activeChests.Add(chestInfo);

                NotifyClientsToSpawnChest(chestInfo);
            } 
        }
    }
    public void OnNightStarted()
    {
        DespawnAllChests();


        SpawnChests();
        ChestRoutineWasUsed = true;
    }

    public bool ChestRoutineWasUsed = false;

    private int GetRandomTierWithWeights()
    {
 
        int[] weights = { 44, 25, 15, 9, 5, 2 }; // Tier 1 is more common, Tier 6 is rarest
        int totalWeight = 0;
        int[] cumulativeWeights = new int[weights.Length];

        // Compute the cumulative weight sum
        for (int i = 0; i < weights.Length; i++)
        {
            totalWeight += weights[i];
            cumulativeWeights[i] = totalWeight;
        }

        // Generate a random value within the total weight range
        int randomValue = UnityEngine.Random.Range(0, totalWeight);

        // Determine which tier corresponds to the random value
        for (int i = 0; i < cumulativeWeights.Length; i++)
        {
            if (randomValue < cumulativeWeights[i])
            {
                return i + 1;  // Tiers are 1-indexed
            }
        }

        return 1;  // Default to tier 1 if something goes wrong
    }

    private Dictionary<string, int> chestLocationMapping = new Dictionary<string, int>();
    private int ChooseRandomLocationIndex()
    {
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < possibleChestSpawnLocations.Count; i++)
        {
            if (!occupiedLocations.Contains(i))
            {
                availableIndices.Add(i);
            }
        }

        if (availableIndices.Count == 0)
        {
            Debug.LogWarning("No available locations to spawn chests.");
            return -1; // Return an invalid index to indicate no available location.
        }

        int randomIndex = UnityEngine.Random.Range(0, availableIndices.Count);
        return availableIndices[randomIndex]; // Return the selected index.
    }

    private ChestInfo CreateChestInfo(int tier)
    {
        int locationIndex = ChooseRandomLocationIndex();
        if (locationIndex == -1)
        {
            Debug.LogError("Failed to find an available location for the chest.");
            return null; // Early return if no location is available.
        }
        occupiedLocations.Add(locationIndex);

        Transform chosenLocation = possibleChestSpawnLocations[locationIndex];

        var chestInfo = new ChestInfo
        {
            UniqueID = System.Guid.NewGuid().ToString(),
            Tier = tier,
            Position = chosenLocation.position,
            Rotation = chosenLocation.rotation,
            MassItems = new List<MassItem>(),
            SerializedUniqueItems = new List<SerializedUniqueFromTreasureChest>(),
            DespawnTime = DateTime.Now.AddSeconds(30)
        };

        chestLocationMapping[chestInfo.UniqueID] = locationIndex;  // Map the chest ID to its location index

        return chestInfo;
    }



    public ChestInfo GetChestInfoAtPosition(Vector3 position)
    {
        return activeChests.FirstOrDefault(chest => chest.Position == position);
    }
    

    public void OnNightEnded()
    {
        if (!ChestRoutineWasUsed)
        {
            return;
        }

        DespawnAllChests();

        ChestRoutineWasUsed = false;

    }

    public void MarkLocationAsUnoccupied(Vector3 position)
    {
        int index = possibleChestSpawnLocations.FindIndex(loc => loc.position == position);
        if (index != -1)
        {
            occupiedLocations.Remove(index);
        }
    }


    public void DespawnAllChests()
    {
 
        foreach (var chest in activeChests)
        {
            if (chest == null)
            {
                Debug.Log("NULL CHEST");
                continue;
            }
            NotifyClientsToRemoveChest(chest);
        }
        activeChests.Clear();
        occupiedLocations.Clear();
    }


 

    [ObserversRpc(BufferLast = false, ExcludeOwner = false)]
private void NotifyClientsToRemoveChest(ChestInfo chest)
    {

   //     Debug.LogError("NOTIFY CLIENT TO REMOVE CHES T 2!!!");
        REFERENCENEXUS.Instance.clientlootdropsystem.NotifyClientToRemoveChest(chest);

    }

    private void Start()
    {

        REFERENCENEXUS.Instance.lootdropsystem = this;
    }



    private LOCALDATABASEMANAGER.SerializableUniqueItem ConvertToSerializableUniqueItem(UniqueItem uniqueItem)
    {
   
        return new LOCALDATABASEMANAGER.SerializableUniqueItem
        {
      
            PropString1 = uniqueItem.PropString1,
            PropString2 = uniqueItem.PropString2,
            PropString3 = uniqueItem.PropString3,
            PropInt1 = uniqueItem.PropInt1,
            PropInt2 = uniqueItem.PropInt2,
            PropInt3 = uniqueItem.PropInt3,
            CreatedAt = DateTime.UtcNow,  
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public bool CheckUniqueItemExistsInLootBox(string chestID, int uniqueID)
    {
        var chest = GetChestContents(chestID);
        return chest != null && chest.SerializedUniqueItems.Any(item => item.TransientUniqueID == uniqueID);
    }

    public ChestInfo GetChestByID(string chestID)
    {
        return activeChests.FirstOrDefault(chest => chest.UniqueID == chestID);
    }

    public void RemoveUniqueItemFromLootBox(string chestID, int uniqueID)
    {
        var chest = activeChests.FirstOrDefault(chest => chest.UniqueID == chestID);
        if (chest != null)
        {
            chest.SerializedUniqueItems.RemoveAll(item => item.TransientUniqueID == uniqueID);
        }
    }




}
