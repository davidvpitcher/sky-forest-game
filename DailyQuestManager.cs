using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mono.Data.Sqlite;
using System.Data;
using FishNet;
using FishNet.Object;
using FishNet.Connection;
using System.Globalization;

[System.Serializable] 
public class DailyQuestItem
{
    public string itemName;
    public int medalReward;
    public int minQuantity;
    public int maxQuantity;

    public DailyQuestItem(string itemName, int medalReward, int minQuantity, int maxQuantity)
    {
        this.itemName = itemName;
        this.medalReward = medalReward;
        this.minQuantity = minQuantity;
        this.maxQuantity = maxQuantity;
    }
    // Default constructor
    public DailyQuestItem() { }
}

[System.Serializable]
public class DailyQuestDetails
{
    public string itemName;
    public int medalReward;
    public int requiredQuantity;

    public DailyQuestDetails(string itemName, int medalReward, int requiredQuantity)
    {
        this.itemName = itemName;
        this.medalReward = medalReward;
        this.requiredQuantity = requiredQuantity;
    }
    // Default constructor
    public DailyQuestDetails() { }
}

[System.Serializable]
public class PlayerDailyProgress
{
    public string Username;
    public DateTime CookingDate;
    public DateTime FarmingDate;
    public DateTime FishingDate;
    public string CookingTaskStatus;
    public string FarmingTaskStatus;
    public string FishingTaskStatus;
    public string CookingItemTurnedIn;
    public string FarmingItemTurnedIn;
    public string FishingItemTurnedIn;
    public int CookingItemCount;
    public int FarmingItemCount;
    public int FishingItemCount;

    public PlayerDailyProgress(string username)
    {
        Username = username;
    }
    // Default constructor
    public PlayerDailyProgress() { }
}

public class DailyQuestManager : NetworkBehaviour
{

    [Header("Daily Quests Setup")]
    public List<DailyQuestItem> cookingDailyQuestItems;
    public List<DailyQuestItem> farmingDailyQuestItems;
    public List<DailyQuestItem> fishingDailyQuestItems;


    private System.Random rng = new System.Random();


    public override void OnStartServer()
    {
        base.OnStartServer();

        if (!InstanceFinder.IsServer)
        {
            return;
        }
        CheckDailiesOnInitialization();
        REFERENCENEXUS.Instance.servercommunication.OnServerBooted();
    }
   void Start()
    {
        REFERENCENEXUS.Instance.dailyquestmanager = this;
        
    }

    private DailyQuestItem ChooseRandomDailyQuestItem(List<DailyQuestItem> items)
    {
        int index = rng.Next(items.Count);
        return items[index];
    }
    private int DetermineRequiredQuantity(DailyQuestItem item)
    {
        return rng.Next(item.minQuantity, item.maxQuantity + 1);
    }
    public void ResetCompletely()
    {
        if (!InstanceFinder.IsServer)
        {
            return;
        }

        DailyQuestItem cookingItem = ChooseRandomDailyQuestItem(cookingDailyQuestItems);
        int cookingRequiredQuantity = DetermineRequiredQuantity(cookingItem);
        DailyQuestItem farmingItem = ChooseRandomDailyQuestItem(farmingDailyQuestItems);
        int farmingRequiredQuantity = DetermineRequiredQuantity(farmingItem);
        DailyQuestItem fishingItem = ChooseRandomDailyQuestItem(fishingDailyQuestItems);
        int fishingRequiredQuantity = DetermineRequiredQuantity(fishingItem);

        UpdateDailyQuest("CookingDailyItem", SerializeQuestDetails(cookingItem, cookingRequiredQuantity));
        UpdateDailyQuest("FarmingDailyItem", SerializeQuestDetails(farmingItem, farmingRequiredQuantity));
        UpdateDailyQuest("FishingDailyItem", SerializeQuestDetails(fishingItem, fishingRequiredQuantity));

        // Update the last reset time
        UpdateLastResetTime(CalculateNextResetTime());

        LOCALDATABASEMANAGER.Instance.InsertDailyQuestHistory(cookingItem, cookingRequiredQuantity, farmingItem, farmingRequiredQuantity, fishingItem, fishingRequiredQuantity);
       ResetAllPlayersProgress();


    }
    public void ResetCompletelyDontUpdateTime()
    {
        if (!InstanceFinder.IsServer)
        {
            return;
        }

       DailyQuestItem cookingItem = ChooseRandomDailyQuestItem(cookingDailyQuestItems);
        int cookingRequiredQuantity = DetermineRequiredQuantity(cookingItem);
        DailyQuestItem farmingItem = ChooseRandomDailyQuestItem(farmingDailyQuestItems);
        int farmingRequiredQuantity = DetermineRequiredQuantity(farmingItem);
        DailyQuestItem fishingItem = ChooseRandomDailyQuestItem(fishingDailyQuestItems);
        int fishingRequiredQuantity = DetermineRequiredQuantity(fishingItem);

       UpdateDailyQuest("CookingDailyItem", SerializeQuestDetails(cookingItem, cookingRequiredQuantity));
        UpdateDailyQuest("FarmingDailyItem", SerializeQuestDetails(farmingItem, farmingRequiredQuantity));
        UpdateDailyQuest("FishingDailyItem", SerializeQuestDetails(fishingItem, fishingRequiredQuantity));


        LOCALDATABASEMANAGER.Instance.InsertDailyQuestHistory(cookingItem, cookingRequiredQuantity, farmingItem, farmingRequiredQuantity, fishingItem, fishingRequiredQuantity);
       ResetAllPlayersProgress();


    }
    private string SerializeQuestDetails(DailyQuestItem item)
    {
        var details = new DailyQuestDetails(item.itemName, item.medalReward, DetermineRequiredQuantity(item));
        return JsonUtility.ToJson(details);
    }
    private string SerializeQuestDetails(DailyQuestItem item, int requiredQuantity)
    {
        var details = new DailyQuestDetails(item.itemName, item.medalReward, requiredQuantity);
        return JsonUtility.ToJson(details);
    }

    private DailyQuestDetails DeserializeQuestDetails(string json)
    {
        if (IsValidJson(json))
        {
            return JsonUtility.FromJson<DailyQuestDetails>(json);
        }
        else
        {
           return null; // Or handle it according to your error handling policy
        }
    }
    public void CheckDailiesOnInitialization()
    {
       using (IDbConnection dbConnection = new SqliteConnection(DBManager.conn))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = "SELECT SettingValue FROM GeneralSettings WHERE SettingKey = 'LastDailyReset'";
                var result = cmd.ExecuteScalar();
                DateTime lastReset;

                if (result != null && DateTime.TryParseExact(result?.ToString(), "yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out lastReset))
                {
                
                    if (DateTime.UtcNow >= lastReset)
                    {
                         ResetCompletelyDontUpdateTime();

                        // Calculate the next reset time after completing the current reset
                        DateTime nextResetTime = CalculateNextResetTime();
                        UpdateLastResetTime(nextResetTime);
                    }
                    else
                    {
                        TimeSpan timeUntilReset = lastReset - DateTime.UtcNow;
                    }
                }
                else
                {
                    DateTime nextResetTime = CalculateNextResetTime();
                    UpdateLastResetTime(nextResetTime);
                }
            }
            dbConnection.Close();
        }
    }
    private void UpdateLastResetTime(DateTime lastResetTime) // LastDailyReset
    {
        using (IDbConnection dbConnection = new SqliteConnection(DBManager.conn))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = @"
                INSERT OR REPLACE INTO GeneralSettings (SettingKey, SettingValue) 
                VALUES ('LastDailyReset', @LastResetTime);";
                cmd.Parameters.Add(new SqliteParameter("@LastResetTime", lastResetTime.ToString("o", CultureInfo.InvariantCulture)));
                cmd.ExecuteNonQuery();
            }
            dbConnection.Close();
        }
    }


    public DateTime CalculateNextResetTime()
    {
        TimeZoneInfo pstZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        DateTime nowUtc = DateTime.UtcNow;
        DateTime nowPst = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, pstZone);

       DateTime nextResetPst = nowPst.Date.AddDays(1);

        DateTime nextResetUtc = TimeZoneInfo.ConvertTimeToUtc(nextResetPst, pstZone);

        return nextResetUtc;
    }


    public void CheckDailiesOnPlayerRequest(NetworkConnection requester, string username)
    {
        if (!InstanceFinder.IsServer)
        {
            return;
        }

   
        using (IDbConnection dbConnection = new SqliteConnection(DBManager.conn))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = "SELECT SettingValue FROM GeneralSettings WHERE SettingKey = 'LastDailyReset'";
                var result = cmd.ExecuteScalar();
                DateTime lastReset;

                if (result != null && DateTime.TryParseExact(result?.ToString(), "yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out lastReset))
                {
                    DateTime currentUtcTime = DateTime.UtcNow;

          
                    DateTime correctResetTimeToShipToClient = lastReset;

                    // Calculate the time until the next reset
                    TimeSpan timeUntilReset = lastReset - currentUtcTime;
                    if (currentUtcTime >= lastReset)
                    {
                        ResetCompletelyDontUpdateTime();
                         // Calculate and store the next reset time after the reset is done
                           DateTime nextResetTime = CalculateNextResetTime();

                       UpdateLastResetTime(nextResetTime);
            

                         correctResetTimeToShipToClient = nextResetTime;
                    }


                    REFERENCENEXUS.Instance.networkbroadcaster.advancednetworkfunctions.ProcessDailiesAfterCheckingDailies(requester, username, correctResetTimeToShipToClient);
                }
            }
            dbConnection.Close();
        }
    }
    public DateTime GetCurrentResetTime()
    {
        using (IDbConnection dbConnection = new SqliteConnection(DBManager.conn))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = "SELECT SettingValue FROM GeneralSettings WHERE SettingKey = 'LastDailyReset'";
                var result = cmd.ExecuteScalar();
                DateTime lastReset;

                if (result != null && DateTime.TryParseExact(result?.ToString(), "yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out lastReset))
                {
                   return lastReset;
                }
                else
                {
                   // Return a sensible default or handle this case appropriately.
                    return DateTime.UtcNow;
                }
            }
        }
    }


    private void UpdateDailyQuest(string key, string json)
    {
        using (IDbConnection dbConnection = new SqliteConnection(DBManager.conn))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                DateTime nextReset = DateTime.UtcNow.Date.AddDays(1);
                cmd.CommandText = @"
                INSERT OR REPLACE INTO GeneralSettings (SettingKey, SettingValue) 
                VALUES (@Key, @Value);
                INSERT OR REPLACE INTO GeneralSettings (SettingKey, SettingValue) 
                VALUES ('LastDailyReset', @LastReset);";
                cmd.Parameters.Add(new SqliteParameter("@Key", key));
                cmd.Parameters.Add(new SqliteParameter("@Value", json));
                cmd.Parameters.Add(new SqliteParameter("@LastReset", nextReset.ToString("o")));

                cmd.ExecuteNonQuery();
            }
            dbConnection.Close();
        }
    
}


    public DailyQuestDetails[] GetCurrentDailyQuestDetails()
    {
        DailyQuestDetails[] currentQuests = new DailyQuestDetails[3];
        string[] questKeys = new string[] { "CookingDailyItem", "FarmingDailyItem", "FishingDailyItem" };

        using (IDbConnection dbConnection = new SqliteConnection(DBManager.conn))
        {
            dbConnection.Open();

            for (int i = 0; i < questKeys.Length; i++)
            {
                using (IDbCommand cmd = dbConnection.CreateCommand())
                {
                    cmd.CommandText = $"SELECT SettingValue FROM GeneralSettings WHERE SettingKey = '{questKeys[i]}'";
                    var result = cmd.ExecuteScalar();

                    // Check if the result is null or invalid JSON; if so, initialize and insert defaults
                    if (result == null || !IsValidJson(result.ToString()))
                    {
                        Debug.Log($"{questKeys[i]} not found or invalid. Initializing with default values.");

                        // Choose a random daily quest item for initialization
                        DailyQuestItem item = ChooseRandomDailyQuestItem(i == 0 ? cookingDailyQuestItems : (i == 1 ? farmingDailyQuestItems : fishingDailyQuestItems));
                        DailyQuestDetails newDetails = new DailyQuestDetails(item.itemName, item.medalReward, DetermineRequiredQuantity(item));

                        // Serialize and insert the new details
                        string jsonDetails = SerializeQuestDetails(item);
                        cmd.CommandText = $"INSERT INTO GeneralSettings (SettingKey, SettingValue) VALUES ('{questKeys[i]}', @Value)";
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(new SqliteParameter("@Value", jsonDetails));
                        cmd.ExecuteNonQuery();

                        // Add the new details to the currentQuests array
                        currentQuests[i] = newDetails;
                    }
                    else
                    {
                        // If valid JSON is found, deserialize it into a DailyQuestDetails object
                        currentQuests[i] = DeserializeQuestDetails(result.ToString());
                    }
                }
            }

            dbConnection.Close();
        }

        return currentQuests;
    }





    // Validate JSON string for DailyQuestDetails
    private bool IsValidJson(string strInput)
    {
        strInput = strInput.Trim();
        if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || // For object
            (strInput.StartsWith("[") && strInput.EndsWith("]"))) // For array
        {
            try
            {
                var obj = JsonUtility.FromJson<DailyQuestDetails>(strInput);
                return true; // JSON is valid and corresponds to the expected type
            }
            catch
            {
                // Invalid JSON format or not matching the DailyQuestDetails type
                return false;
            }
        }
        else
        {
            return false; // Not valid JSON format
        }
    }


    public PlayerDailyProgress GetPlayerDailyProgress(string username)
    {
        PlayerDailyProgress progress = new PlayerDailyProgress(username);

        using (IDbConnection dbConnection = new SqliteConnection(DBManager.conn))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM PlayerDailyProgress WHERE Username = @Username";
                cmd.Parameters.Add(new SqliteParameter("@Username", username));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        progress.CookingDate = DateTime.Parse(reader["CookingDate"].ToString());
                        progress.FarmingDate = DateTime.Parse(reader["FarmingDate"].ToString());
                        progress.FishingDate = DateTime.Parse(reader["FishingDate"].ToString());
                        progress.CookingTaskStatus = reader["CookingTaskStatus"].ToString();
                        progress.FarmingTaskStatus = reader["FarmingTaskStatus"].ToString();
                        progress.FishingTaskStatus = reader["FishingTaskStatus"].ToString();
                        progress.CookingItemTurnedIn = reader["CookingItemTurnedIn"].ToString();
                        progress.FarmingItemTurnedIn = reader["FarmingItemTurnedIn"].ToString();
                        progress.FishingItemTurnedIn = reader["FishingItemTurnedIn"].ToString();

                        int itemCount;
                        if (int.TryParse(reader["CookingItemCount"].ToString(), out itemCount))
                        {
                            progress.CookingItemCount = itemCount;
                        }

                        if (int.TryParse(reader["FarmingItemCount"].ToString(), out itemCount))
                        {
                            progress.FarmingItemCount = itemCount;
                        }

                        if (int.TryParse(reader["FishingItemCount"].ToString(), out itemCount))
                        {
                            progress.FishingItemCount = itemCount;
                        }
                    }
                }
            }
            dbConnection.Close();
        }

        return progress;
    }

    public string GetCurrentDailiesDescription()
    {
        DailyQuestDetails[] currentQuests = GetCurrentDailyQuestDetails();
        if (currentQuests == null || currentQuests.Length != 3)
        {
            return "Error: Could not retrieve current daily quests.";
        }

        string description = "Current Daily Quests:\n";

        // Assuming the order is Cooking, Farming, Fishing
        description += $"Cooking: {currentQuests[0].itemName} (Reward: {currentQuests[0].medalReward} medals, Required: {currentQuests[0].requiredQuantity})\n";
        description += $"Farming: {currentQuests[1].itemName} (Reward: {currentQuests[1].medalReward} medals, Required: {currentQuests[1].requiredQuantity})\n";
        description += $"Fishing: {currentQuests[2].itemName} (Reward: {currentQuests[2].medalReward} medals, Required: {currentQuests[2].requiredQuantity})";

        return description;
    }

    private void ResetAllPlayersProgress()
    {
        using (IDbConnection dbConnection = new SqliteConnection(DBManager.conn))
        {
            dbConnection.Open();

            // Reset each player's daily progress
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = @"
                UPDATE PlayerDailyProgress
                SET 
                    CookingTaskStatus = 'Pending', 
                    FarmingTaskStatus = 'Pending', 
                    FishingTaskStatus = 'Pending',
                    CookingItemTurnedIn = '',
                    FarmingItemTurnedIn = '',
                    FishingItemTurnedIn = '',
                    CookingItemCount = 0,
                    FarmingItemCount = 0,
                    FishingItemCount = 0
                ";
                cmd.ExecuteNonQuery();
            }

            dbConnection.Close();
        }
    }

}
