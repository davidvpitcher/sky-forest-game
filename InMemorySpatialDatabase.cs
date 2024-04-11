using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InMemorySpatialDatabase
{
    public struct TreeInfo
    {
        public int TreeID;
        public Vector3 Position;
    }

    // InMemorySpatialDatabase method to clear all data
    public void ClearAllData()
    {
        grid.Clear();
    }
    private Dictionary<Vector2Int, List<TreeInfo>> grid = new Dictionary<Vector2Int, List<TreeInfo>>();
    private float cellSize = 10.0f;

    // Converts world position to grid cell coordinates
    private Vector2Int PositionToCell(Vector3 position)
    {
        return new Vector2Int(Mathf.FloorToInt(position.x / cellSize), Mathf.FloorToInt(position.z / cellSize));
    }

    // Adds tree information to the spatial database
    public void AddTree(CONSTRUCTIONMANAGER.TreeData treeData)
    {
        TreeInfo treeInfo = new TreeInfo { TreeID = treeData.TreeID, Position = treeData.Location };
        Vector2Int cell = PositionToCell(treeData.Location);
        if (!grid.ContainsKey(cell))
        {
            grid[cell] = new List<TreeInfo>();
        }
        grid[cell].Add(treeInfo);
    }

    public void UpdateLastFailedTreePosition(Vector3 where)
    {
        lastFailedTreePosition = where;
    }

    // Checks for nearby structures within a certain range
    public bool CheckForNearbyStructures(Vector3 position, float range, List<SERVERTREEMANAGER.TreeSpawnLocation> systemPlacedTrees)
    {
        Vector2Int cell = PositionToCell(position);
        int cellRange = Mathf.CeilToInt(range / cellSize);

        HashSet<int> systemPlacedTreeIds = new HashSet<int>(systemPlacedTrees.Select(t => t.treeId));

        for (int x = -cellRange; x <= cellRange; x++)
        {
            for (int y = -cellRange; y <= cellRange; y++)
            {
                Vector2Int searchCell = new Vector2Int(cell.x + x, cell.y + y);
                if (grid.ContainsKey(searchCell))
                {
                    foreach (var treeInfo in grid[searchCell])
                    {
                        // Skip the check if the treeInfo corresponds to a tree placed by the system
                        if (systemPlacedTreeIds.Contains(treeInfo.TreeID))
                        {
                            continue;
                        }

                        if (Vector3.Distance(position, treeInfo.Position) <= range)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        lastFailedTreePosition = position;
        Debug.LogError($"Failed to place a tree as the spatial system thinks the spot is invalid at {position} .");

        return false;  // If no nearby user-placed structures interfere, the location is valid
    }


    // Removes a tree from the spatial database using its TreeID
    public void RemoveTree(int treeId)
    {
        foreach (var cell in grid.Keys)
        {
            grid[cell].RemoveAll(treeInfo => treeInfo.TreeID == treeId);
        }
    }

    public Vector3 GetLastFailedTreePosition()
    {

        return lastFailedTreePosition;
    }

    private Vector3 lastFailedTreePosition;

    // Updates the position of a tree in the spatial database using its TreeID
    public void UpdateTreePosition(int treeId, Vector3 newPosition)
    {
        // First, find and remove the old entry
        RemoveTree(treeId);

        // Then add a new entry with the updated position
        var treeData = new CONSTRUCTIONMANAGER.TreeData
        {
            TreeID = treeId,
            Location = newPosition,
            // Populate other necessary fields from your TreeData structure
        };

        AddTree(treeData);
    }
    public string GetDetailedSpatialMemoryInfo()
    {
        int totalCells = grid.Count;
        int totalTrees = 0;
        foreach (var cell in grid)
        {
            totalTrees += cell.Value.Count;
        }

        string summary = "";
        // Optionally add more details about each cell
        foreach (var cell in grid)
        {
            summary += $"Cell {cell.Key}: {cell.Value.Count} Trees\n";
            // Uncomment to list trees in each cell, be mindful of potential verbosity:
            // foreach (var treeInfo in cell.Value)
            // {
            //     summary += $"- Tree ID: {treeInfo.TreeID}, Position: {treeInfo.Position}\n";
            // }
        }

         summary += $"Total Grid Cells: {totalCells}\nTotal Trees: {totalTrees}\n";

        return summary;
    }

}
