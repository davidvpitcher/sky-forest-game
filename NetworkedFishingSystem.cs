using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet;
using FishNet.Connection;
using System;
using System.Linq;
using Mono.Data.Sqlite;

public class NetworkedFishingSystem : NetworkBehaviour
{
    private Dictionary<string, PlayerFishingData> fishingPlayers = new Dictionary<string, PlayerFishingData>();
    // Add a dictionary to track active coroutines for each player.
    private Dictionary<string, Coroutine> activeFishingCoroutines = new Dictionary<string, Coroutine>();

    public class PlayerFishingData
    {
        public NetworkConnection Connection { get; private set; }
        public NetworkedAdmiral Admiral { get; private set; }

        public string AuthToken;

        public string Username;
        public PlayerFishingData(NetworkConnection connection, NetworkedAdmiral admiral, string authToken, string username)
        {
            Connection = connection;
            Admiral = admiral;
            AuthToken = authToken;
            Username = username;
        }
    }

    public int GetRarityForFish(string itemName)
    {
        // Iterate through each LootTableEntry
        foreach (var lootTableEntry in LootTables)
        {
            // Check if the itemName is present in MassItems or UniqueItems of this LootTableEntry
            var hasMassItem = lootTableEntry.MassItems.Any(massItem => massItem.itemName == itemName);
            var hasUniqueItem = lootTableEntry.UniqueItems.Any(uniqueItem => uniqueItem.ItemName == itemName);

            // If the item is found, return its rarity as an integer
            if (hasMassItem || hasUniqueItem)
            {
                // Assuming you want to convert the LootTier to an integer
                // Adjust this part if you want to return a string or any other format
                return (int)lootTableEntry.Tier;
            }
        }

        // Return -1 or any other value you choose to signify that the item was not found
        return -1;
    }

    public void ResetCompletely()
    {
        activeFishingCoroutines.Clear();
        fishingPlayers.Clear();
        playerInputReceived.Clear();
        StopAllCoroutines();
        playerLastBiteTime.Clear();
    }

    public void Start()
    {


        REFERENCENEXUS.Instance.networkedfishingsystem = this;

    }
    // Add a dictionary to track if a player has successfully responded in time
    private Dictionary<string, bool> playerInputReceived = new Dictionary<string, bool>();


    public void RegisterPlayerForFishingSERVERNORPC(string username, NetworkedAdmiral admiral, NetworkConnection connection, string authtoken)
    {
        if (string.IsNullOrEmpty(authtoken))
        {
            Debug.LogError("Error missing username at fish time");
            return;
        }
        if (string.IsNullOrEmpty(authtoken))
        {
            Debug.LogError("Error missing username at fish time");
            return;
        }
        if (!fishingPlayers.ContainsKey(authtoken))
        {
            var playerData = new PlayerFishingData(connection, admiral, authtoken, username);
            fishingPlayers[authtoken] = playerData;
            // Store the started coroutine in the dictionary
            activeFishingCoroutines[authtoken] = StartCoroutine(FishingCoroutine(authtoken, playerData));
        }
        else
        {
            Debug.Log($"Player {username} is already registered for fishing.");
        }
    }

    public void UnregisterForFishingGameSERVERNORPC(string username, string authtoken)
    {
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogError("Error missing username at fish time");
            return;
        }
        if (string.IsNullOrEmpty(authtoken))
        {
            Debug.LogError("Error missing authtoken at fish time");
            return;
        }
        if (fishingPlayers.ContainsKey(authtoken))
        {
            // Check if there's an active coroutine for this user and stop it.
            if (activeFishingCoroutines.TryGetValue(authtoken, out var coroutine))
            {
                StopCoroutine(coroutine);
                activeFishingCoroutines.Remove(authtoken);
            }

            fishingPlayers.Remove(authtoken);
            playerInputReceived.Remove(authtoken);
            playerLastBiteTime.Remove(authtoken);
        }
    }

    public void OnPlayerDisconnectedFromServer(string username, string authtoken)
    {
        if (string.IsNullOrEmpty(username))
        {

            Debug.LogError("Error missing username at fish time dc");
            return;
        }
        if (string.IsNullOrEmpty(authtoken))
        {

            Debug.LogError("Error missing authtoken at fish time dc");
            return;
        }
        if (fishingPlayers.ContainsKey(authtoken))
        {
            StopCoroutine(FishingCoroutine(authtoken, fishingPlayers[authtoken]));
            fishingPlayers.Remove(authtoken);
            playerInputReceived.Remove(authtoken);
            playerLastBiteTime.Remove(authtoken);
        }
    }


    // Add a dictionary to track the last bite time for each player.
    private Dictionary<string, float> playerLastBiteTime = new Dictionary<string, float>();


    // The main fishing coroutine on the server
    private IEnumerator FishingCoroutine(string authtoken, PlayerFishingData playerData)
    {
        yield return new WaitForSeconds(0.5f);

        while (fishingPlayers.ContainsKey(authtoken))
        {
            // Safely initialize player's input received status to false
            playerInputReceived[authtoken] = false;

            float waitTime = UnityEngine.Random.Range(5, 15);


            yield return new WaitForSeconds(waitTime);

            playerInputReceived[authtoken] = false;
            // Notify player and observers of bite event
            NotifyPlayerBiteReceivedSERVERNORPC(playerData.Connection, playerData.Admiral, authtoken, playerData.Username);
            // Record the time at which the bite event was sent
            playerLastBiteTime[authtoken] = Time.time;
            float responseDeadline = Time.time + 1.4f;

            // Wait for the deadline or input received
            while (Time.time < responseDeadline)
            {
                // Ensure the player is still part of the game before proceeding
                if (!fishingPlayers.ContainsKey(authtoken) || !playerInputReceived.ContainsKey(authtoken))
                {
                    Debug.Log($"Failed to locate player {authtoken}, server turning off game during wait.");
                    yield break;
                }

                // Break out of the loop early if input has been received
                if (playerInputReceived[authtoken])
                {
                    break;
                }

                yield return null; // Continue checking in the next frame
            }

            // Check the player's success only if they are still registered
            if (fishingPlayers.ContainsKey(authtoken) && playerInputReceived.ContainsKey(authtoken))
            {
                bool success = playerInputReceived[authtoken];
                SendPlayerResultsOfCatchFishAttempt(playerData.Connection, success);

                if (FishingSystemDebugMode)
                {
                    string msg = $"{authtoken} caught fish ";
                    REFERENCENEXUS.Instance.networkbroadcaster.ToggleFishingDebugFORALL(msg); // seen by broken client

                }

                if (success)
                {
                    HandleSuccessfulCatch(playerData.Username, playerData, authtoken);
                }
            }
            else
            {
                Debug.Log($"Failed to evaluate success for player {authtoken} as they are no longer part of the game.");
            }

        }
    }


    public bool FishingSystemDebugMode = false;


    private void HandleSuccessfulCatch(string username, PlayerFishingData playerData, string authtoken)
    {
        if (FishingSystemDebugMode)
        {
            Debug.Log($"successful catch for {username}");
        }
        LootTier tier = DetermineLootTier(playerData);
        AssignLootToPlayer(username, tier, playerData);
     

        UnregisterForFishingGameSERVERNORPC(username, playerData.AuthToken);
    }



    [ServerRpc(RequireOwnership = false)]
    public void ReceiveMinigameInputFromPlayerSERVER(string username, NetworkConnection requester, string authtoken)
    {
        ValidateConnection(requester);

    
        if (fishingPlayers.ContainsKey(authtoken))
        {
            if (!playerInputReceived.ContainsKey(authtoken))
            {
                // Initialize the player's input status to avoid KeyNotFoundException.
                playerInputReceived[authtoken] = false;
            }

            float biteTime = playerLastBiteTime.ContainsKey(authtoken) ? playerLastBiteTime[authtoken] : float.NegativeInfinity;
            if (Time.time - biteTime <= 1.4f)
            {
                playerInputReceived[authtoken] = true;
                SendPlayerResultsOfCatchFishAttempt(requester, true);
            }
        }
        else
        {
            Debug.LogWarning($"Player {username} {authtoken} is not registered for fishing but attempted to send input.");
        }
    }

    public void ValidateConnection(NetworkConnection requester)
    {

        REFERENCENEXUS.Instance.networkbroadcaster.ValidateConnection(requester); // for the server to catch and log certain targetrpc connection related errors
    }


    
 
    public void NotifyPlayerBiteReceivedSERVERNORPC(NetworkConnection requester, NetworkedAdmiral networkedAdmiral, string authtoken, string username)
    {

        ValidateConnection(requester);


        NotifyPlayerBiteReceivedTARGET(requester, username, authtoken);
        NotifyPlayerBiteReceivedFORALL(networkedAdmiral, username, authtoken);
    }

    [ObserversRpc(BufferLast = false, ExcludeOwner = false)]
    public void NotifyPlayerBiteReceivedFORALL(NetworkedAdmiral networkedAdmiral, string username, string authtoken)
    {

        networkedAdmiral.myCharacterController.ReceiveBite(); // visually causes the admiral to see a nibble on the line 
    
    }
    
    [TargetRpc]
    public void NotifyPlayerBiteReceivedTARGET(NetworkConnection requester, string username, string authtoken)
    {
        NetworkedAdmiral localAdmiral = REFERENCENEXUS.Instance.gsc.realLocalNetworkedAdmiral;

        localAdmiral.myCharacterController.BeginFishingMinigame(); // allows player opportunity to click in the game 
      
   
        if (username != DBManager.username)
        {
            Debug.LogError("maybe received bite for wrong person");
        }
    
    }

    public void ReceiveMinigameInputFromPlayer()
    {
        ReceiveMinigameInputFromPlayerSERVER(DBManager.username, REFERENCENEXUS.Instance.gsc.realLocalPlayerConnection, REFERENCENEXUS.Instance.GetAuthToken());

    }
 

    [TargetRpc]
    public void SendPlayerResultsOfCatchFishAttempt(NetworkConnection requester, bool success)
    {

        if (REFERENCENEXUS.Instance.networkbroadcaster.FishingDebugMode)
        {

            REFERENCENEXUS.Instance.SendLocalMessage("CATCH RESULTS: success is " + success);
        }

        REFERENCENEXUS.Instance.gsc.realLocalNetworkedAdmiral.myCharacterController.ReceiveCatchResults(success); // implemented but does nothing yet
    }
    public enum LootTier
    {
        Common,
        Uncommon,
        Rare,
        Legendary,
        Mythical
    }

    [Serializable]
    public class LootTableEntry
    {
        public LootTier Tier;
        public List<LOOTDROPSYSTEM.MassItem> MassItems;
        public List<UniqueItem> UniqueItems;
        public float Probability; // Probability of this tier being selected
    }

    public List<LootTableEntry> LootTables;

    public float GetFishingBonusFromCharmPower(int charmPower)
    {
        float fishingBonus = CalculateCharmBonus(new NETWORKBROADCASTER.CharmEffect(true, charmPower)); // Use centralized method
        return fishingBonus * 100; // Convert to percentage for display
    }
    public float CalculateCharmBonus(NETWORKBROADCASTER.CharmEffect charmEffect)
    {
        if (!charmEffect.HasCharm)
        {
            return 0.0f;  // No bonus if no charm
        }

        // Start with a 1% bonus.
        float bonus = 0.01f;

        // Scale the bonus based on charm power, ranging from 0% to an additional 3%.
        bonus += (charmEffect.Power / 100.0f) * 0.03f;

        // If the charm effect is at max power, add an extra 1% bonus.
        if (charmEffect.Power == 100)
        {
            bonus += 0.01f;
        }

        return bonus;
    }




    private LootTier DetermineLootTier(PlayerFishingData playerData)
    {
        NETWORKBROADCASTER.CharmEffect charmEffect = GetAquamystCharmBonus(playerData.Username, playerData.AuthToken);
        float charmBonus = CalculateCharmBonus(charmEffect);

        float roll = UnityEngine.Random.Range(0f, 0.9999999f);

        if (charmEffect.HasCharm)
        {
            roll -= charmBonus;
        }

        roll = Mathf.Clamp(roll, 0f, 0.9999999f);
        roll = 1 - roll;

        float cumulativeProbability = 0f;
        foreach (var entry in LootTables)
        {
            cumulativeProbability += entry.Probability;

            if (roll < cumulativeProbability || Math.Abs(roll - cumulativeProbability) < float.Epsilon)
            {
                return entry.Tier;
            }
        }

        return LootTables.LastOrDefault()?.Tier ?? LootTier.Common;
    }

    private NETWORKBROADCASTER.CharmEffect GetAquamystCharmBonus(string username, string authToken)
    {
        int userId = LOCALDATABASEMANAGER.Instance.GetUserIdByUsernameAndToken(username, authToken);

        using (var dbConnection = new SqliteConnection(DBManager.conn))
        {
            dbConnection.Open();
            using (var cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = @"
            SELECT upi.prop_int1 FROM unique_player_items upi
            JOIN AccessorySlots acs ON upi.unique_id = acs.unique_item_id
            WHERE upi.item_name = 'AQUAMYSTCHARM' AND acs.user_id = @userId";
                cmd.Parameters.Add(new SqliteParameter("@userId", userId));

                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    int charmPower = Convert.ToInt32(result);
                    return new NETWORKBROADCASTER.CharmEffect(true, charmPower);
                }
            }
        }

        return new NETWORKBROADCASTER.CharmEffect(false, 0);  // No charm found
    }



    private void AssignLootToPlayer(string username, LootTier tier, PlayerFishingData playerData)
    {
        var tierTable = LootTables.FirstOrDefault(table => table.Tier == tier);

        if (tierTable != null)
        {
            // Randomly choose between mass and unique items if available
            bool chooseUnique = (tierTable.UniqueItems.Count > 0) && (UnityEngine.Random.value > 0.5f || tierTable.MassItems.Count == 0);
            if (chooseUnique)
            {
                // Send unique item
                var uniqueItem = tierTable.UniqueItems[UnityEngine.Random.Range(0, tierTable.UniqueItems.Count)];
                SendUniqueItemToPlayer(username, uniqueItem, playerData, true);
            }
            else
            {
                // Send mass item
                var massItem = tierTable.MassItems[UnityEngine.Random.Range(0, tierTable.MassItems.Count)];
                SendMassItemToPlayer(username, massItem, playerData, true);
            }
        }
        if (FishingSystemDebugMode)
        {
            Debug.Log($"FISH GAME OVER FOR PLAYER {username}");
        }
        InformPlayerFishGameOver(playerData.Connection);
        InformEveryoneFishGameOverForPlayer(playerData.Admiral, username);
    }


    [ObserversRpc(BufferLast = false, ExcludeOwner = false)]
    public void InformEveryoneFishGameOverForPlayer(NetworkedAdmiral networkedAdmiral, string username)
    {

        if (REFERENCENEXUS.Instance.networkbroadcaster.FishingDebugMode)
        {

            REFERENCENEXUS.Instance.SendLocalMessage($"Fish game off for player {username}");
        }
        networkedAdmiral.myCharacterController.TurnOffFishingTrigger();



    }
    [TargetRpc] 
    public void InformPlayerFishGameOver(NetworkConnection conn)
    {
        Debug.Log("Server send player fish game over");
        REFERENCENEXUS.Instance.gsc.realLocalNetworkedAdmiral.myCharacterController.ServerDeclaresFishGameOver();


    }

    private void SendUniqueItemToPlayer(string username, UniqueItem uniqueItem, PlayerFishingData playerData, bool shouldUpdateFishbook)
    {

        LOCALDATABASEMANAGER.SerializableUniqueItem newSerializableUniqueItem = new LOCALDATABASEMANAGER.SerializableUniqueItem();
        newSerializableUniqueItem.ItemName = uniqueItem.ItemName;
        newSerializableUniqueItem.Username = username;
        newSerializableUniqueItem.PropString1 = uniqueItem.PropString1;
        newSerializableUniqueItem.PropString2 = uniqueItem.PropString2;
        newSerializableUniqueItem.PropString3 = uniqueItem.PropString3;
        newSerializableUniqueItem.PropInt1 = uniqueItem.PropInt1;
        newSerializableUniqueItem.PropInt2 = uniqueItem.PropInt2;
        newSerializableUniqueItem.PropInt3 = uniqueItem.PropInt3;

    REFERENCENEXUS.Instance.networkbroadcaster.StoreUniqueItemForPlayerServerAuthoritatively(playerData.Connection, username, uniqueItem.ItemName, newSerializableUniqueItem, true, shouldUpdateFishbook); // update the database item

    }

    private void SendMassItemToPlayer(string username, LOOTDROPSYSTEM.MassItem massItem, PlayerFishingData playerData, bool shouldUpdateFishbook)
    {

       
   
        bool validItem = ValidateItem(massItem.itemName);

        if (!validItem)
        {
            Debug.LogError("Error with item");
            REFERENCENEXUS.Instance.networkbroadcaster.BroadcastAnnouncement("Error with item : " + massItem.itemName);
            return;
        }


        REFERENCENEXUS.Instance.networkbroadcaster.StoreItemForPlayerServerAuthoritatively(playerData.Connection, username, massItem.itemName, 1, massItem.quantity, true, shouldUpdateFishbook); // update the database item

    }

    public string GetDetailedFishingInfo()
    {
        // Start building the info string with a header
        string info = "Fishing Info:\n";

        // Iterate through each registered player and append their status
        foreach (var pair in fishingPlayers)
        {
            string username = pair.Key;

            // Check if the player has responded or is waiting for a bite
            bool isWaitingForBite = playerInputReceived.ContainsKey(username) && !playerInputReceived[username];
            bool isActive = playerInputReceived.ContainsKey(username);
            bool isCoroutineActive = activeFishingCoroutines.ContainsKey(username) && activeFishingCoroutines[username] != null;

            // Append detailed information for this player
            info += $"Player: {username}\n";
            info += $" - Active: {isActive}\n";
            info += $" - Coroutine Active: {isCoroutineActive}\n";
            info += $" - Waiting for Bite: {isWaitingForBite}\n";
        }

        // Additionally, check the coroutine states independently
        info += "\nCoroutine States:\n";
        foreach (var coroutinePair in activeFishingCoroutines)
        {
            string username = coroutinePair.Key;
            bool coroutineExists = coroutinePair.Value != null;
            info += $"Player: {username} - Coroutine Active: {coroutineExists}\n";
        }

        // Summarize the total registered players and active coroutines
        info += $"\nTotal registered players: {fishingPlayers.Count}\n";
        info += $"Total active coroutines: {activeFishingCoroutines.Count(kv => kv.Value != null)}\n";

        // If no players are registered, note that as well
        if (fishingPlayers.Count == 0)
        {
            info += "No players are currently fishing.\n";
        }

        return info;
    }


    public bool ValidateItem(string itemName)
    {
        ItemGuideBase itemGuide = REFERENCENEXUS.Instance.possibleloot.getItemGuideFromString(itemName);

        if (itemGuide == null)
        {
            return false;
        } else
        {
            return true;
        }


    }
}
