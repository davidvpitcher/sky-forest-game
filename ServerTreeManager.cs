using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet;
using FishNet.Connection;
using System.Linq;
using Mono.Data.Sqlite;
using System.Data;

public class ServerTreeManager : NetworkBehaviour
{
  
    public string GetDetailedSpatialMemoryInfo()
    {
        return spatialDatabase.GetDetailedSpatialMemoryInfo();
    }

    public Vector3 GetLastFailedTreePosition()
    {

        return spatialDatabase.GetLastFailedTreePosition();
    }
    public string GetDetailedTreeInfo()
    {
        int totalTrees = spawnLocations.Count;
        int occupiedTrees = spawnLocations.Count(loc => loc.isOccupied);
        bool isRoutineRunning = treeRoutine1 != null;

        string info = $"Total Spawn Locations: {totalTrees}\n" +
                      $"Occupied Spawn Locations: {occupiedTrees}\n" +
                      $"Spawn Routine Running: {isRoutineRunning}\n" +
                      $"Max Trees Allowed: {maxTrees}\n" +
                      $"Tree Spawn Delay: {spawnDelay} seconds\n" +
                      $"Database Loaded: {databaseLoaded}\n" +
                      $"Server Booted: {serverBooted}\n" +
                      $"Tree Manager Initialized: {initializedTreeManager}";

        return info;
    }


    [System.Serializable]
    public class TreeSpawnLocation
    {
        public string locationTag;
        public bool isOccupied;
        public int treeId;
        public Quaternion rotation;
        public Vector3 position;
        public string treeType;
    }


    public List<TreeSpawnLocation> spawnLocations = new List<TreeSpawnLocation>();


    private int maxTrees = 30; 
    private float spawnDelay = 30.0f;  

    public override void OnStartServer()
    {
        base.OnStartServer();

    

        serverBooted = true;
        CheckTreeManagerInitialization();
    }

    public void CheckTreeManagerInitialization()
    {

        if (initializedTreeManager)
        {
            return;
        }

        if (databaseLoaded && serverBooted)
        {
            InitializeTreeManager();
        }


    }

    public void InitializeTreeManager()
    {
        if (!InstanceFinder.IsServer)
        {
            return;
        }

        UpdateSpawnLocationsFromDatabase();

      


        var trees = REFERENCENEXUS.Instance.constructionmanager.GetTrees();  // Retrieve all trees from the database
        foreach (var tree in trees)
        {
            spatialDatabase.AddTree(tree);
        }

        EndTreeRoutineGracefully();
        treeRoutine1 = StartCoroutine(TreeSpawnRoutine());

        initializedTreeManager = true;

    }
    private void UpdateSpawnLocationsFromDatabase()
    {
        string conn = LOCALDATABASEMANAGER.Instance.Conn;
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = "SELECT LocationTag, IsOccupied, TreeID FROM TreeSpawnLocations";
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string locationTag = reader.GetString(0);
                        bool isOccupied = reader.GetInt32(1) == 1;
                        int treeId = reader.IsDBNull(2) ? -1 : reader.GetInt32(2);

                        var spawnLocation = spawnLocations.FirstOrDefault(loc => loc.locationTag == locationTag);
                        if (spawnLocation != null)
                        {
                            spawnLocation.isOccupied = isOccupied;
                            spawnLocation.treeId = treeId;
                        }
                    }
                }
            }
            dbConnection.Close();
        }
    }
    public void OnDatabaseLoaded()
    {
        if (databaseLoaded)
        {
            return;

        } 

        databaseLoaded = true;
        CheckTreeManagerInitialization();

    }

    public bool serverBooted = false;

    public bool databaseLoaded = false;

    public bool initializedTreeManager = false;

    private void EndTreeRoutineGracefully()
    {

        if (treeRoutine1 != null)
        {
            StopCoroutine(treeRoutine1);
            treeRoutine1 = null;
        }
    }

    private Coroutine treeRoutine1;
    private IEnumerator TreeSpawnRoutine()
    {
        while (true)
        {

            yield return new WaitForSeconds(spawnDelay);

            if (spawnLocations.Count(tree => tree.isOccupied) < maxTrees)
            {
                SpawnTreeAtAvailableLocation();
            } else
            {
                EndTreeRoutineGracefully();
            }
        }
    }

    private InMemorySpatialDatabase spatialDatabase = new InMemorySpatialDatabase();

    private bool IsLocationValid(Vector3 location)
    {
        float minDistanceFromStructures = 10.0f;
       return !spatialDatabase.CheckForNearbyStructures(location, minDistanceFromStructures, spawnLocations);
    }



    private void SpawnTreeAtAvailableLocation()
    {
        var unoccupiedLocations = spawnLocations.Where(loc => !loc.isOccupied).ToList();

        if (unoccupiedLocations.Any())
        {
            var randomIndex = Random.Range(0, unoccupiedLocations.Count);
            var selectedLocation = unoccupiedLocations[randomIndex];

           if (IsLocationValid(selectedLocation.position))
            {
                Vector3 whereToPlaceTree = selectedLocation.position;
                Quaternion howToPlaceTree = selectedLocation.rotation;

                ItemGuideBase itemGuideForTree = REFERENCENEXUS.Instance.possibleloot.getItemGuideFromString(selectedLocation.treeType);
                int treeId = REFERENCENEXUS.Instance.constructionmanager.ServerSpawnTreeAuthoritatively(whereToPlaceTree, howToPlaceTree, selectedLocation.treeType, itemGuideForTree.durability);

                selectedLocation.isOccupied = true;
                selectedLocation.treeId = treeId;

                LOCALDATABASEMANAGER.Instance.UpdateTreeSpawnLocationStatus(selectedLocation.locationTag, true, treeId);

            }
            else
            {
              spatialDatabase.UpdateLastFailedTreePosition(selectedLocation.position);
            }
        }
    }


    public void OnTreeRemoved(int treeId)
    {
        var location = spawnLocations.FirstOrDefault(loc => loc.treeId == treeId);
        if (location != null)
        {
            location.isOccupied = false;
            location.treeId = -1;

         LOCALDATABASEMANAGER.Instance.UpdateTreeSpawnLocationStatus(location.locationTag, false, -1);


            EndTreeRoutineGracefully(); 
            treeRoutine1 = StartCoroutine(TreeSpawnRoutine());

       } 
    }

    public void OnTreeAdded(CONSTRUCTIONMANAGER.TreeData treeData)
    {
        
        spatialDatabase.AddTree(treeData);

    }
    public void OnTreeUpdated(int treeId, Vector3 newPosition)
    {
        spatialDatabase.UpdateTreePosition(treeId, newPosition);

    }
    public void OnAnyTreeRemoved(int treeId)
    {
    
        spatialDatabase.RemoveTree(treeId);

    }
    

    public void ClearAllData()
    {

        spatialDatabase.ClearAllData();
    }
    public void OnAnyBulkTreeRemoval(List<int> treeIDs)
    {
        foreach (int treeId in treeIDs)
        {
            spatialDatabase.RemoveTree(treeId);
        }

    }

    public void OnAnyBulkTreeAddition(List<CONSTRUCTIONMANAGER.TreeData> treeDatas)
    {
        foreach (var treeData in treeDatas)
        {
            spatialDatabase.AddTree(treeData);
        }
    }


    public string DetermineTreeType()
    {


        return "FIRSAPLING"; // default to firsapling but gives me the option of setting to pine sapling in inspector

    }

    public void ResetCompletely()
    {
        EndTreeRoutineGracefully();
    }
    void Start()
    {

        REFERENCENEXUS.Instance.servertreemanager = this;
    }
    public List<TreeSpawnLocation> runtimeAddedLocations = new List<TreeSpawnLocation>();

    public void AddTreeSpawnLocation(Vector3 where, Quaternion rotation)
    {
        var newLocation = new TreeSpawnLocation
        {
            position = where,
            rotation = rotation,
            treeType = DetermineTreeType(),  // Or however you determine this at runtime
            locationTag = $"NewTreeSpawn_{where}_{System.DateTime.Now.Ticks}"  // Ensure uniqueness
        };

        runtimeAddedLocations.Add(newLocation);

    }


    private void UpdateTreeLocationsFromDatabase()
    {
       using (IDbConnection dbConnection = new SqliteConnection(DBManager.conn))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string query = "SELECT LocationTag, IsOccupied, TreeID FROM TreeSpawnLocations";
                dbCmd.CommandText = query;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string locationTag = reader.GetString(0);
                        bool isOccupied = reader.GetInt32(1) == 1;
                        int treeId = reader.IsDBNull(2) ? -1 : reader.GetInt32(2);

                        var spawnLocation = spawnLocations.FirstOrDefault(loc => loc.locationTag == locationTag);
                        if (spawnLocation != null)
                        {
                            spawnLocation.isOccupied = isOccupied;
                            spawnLocation.treeId = treeId;
                        }
                    }
                    reader.Close();
                }
            }

            dbConnection.Close();
        }

    }

}
