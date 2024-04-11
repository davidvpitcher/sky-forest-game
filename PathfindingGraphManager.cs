using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
public class PathfindingGraphManager : MonoBehaviour
{
 
    void Start()
    {
        REFERENCENEXUS.Instance.pathfindinggraphmanager = this;

        astarPath = REFERENCENEXUS.Instance.aimanager.pathFinder;
    }

    public AstarPath astarPath;


    public bool PrepareGraphForMob(Transform mobTransform)
    {
        var relevantGraph = DetermineRelevantGraph(mobTransform.position);
        if (relevantGraph == null)
        {
            // Create a new graph if none is suitable.
            relevantGraph = CreateGraphForMob(mobTransform.position);
        }

        if (relevantGraph != null)
        {
            // Scan the relevant graph.
            AstarPath.active.Scan(relevantGraph);
            return true;
        }
        return false;
    }
    private NavGraph CreateGraphForMob(Vector3 position)
    {
        var graph = astarPath.data.AddGraph(typeof(Pathfinding.LayerGridGraph)) as Pathfinding.LayerGridGraph;

        graph.center = position - new Vector3 (0f, 5f, 0f); // slightly lower than the mob to include all the floor


        graph.SetDimensions(100, 100, 1);

 
        graph.maxClimb = 2;       // Maximum height the character can climb (adjust as needed)
        graph.maxSlope = 30;      // Maximum slope (in degrees) the character can traverse

        graph.collision.use2D = false;    
  
        graph.collision.height = 2;         
        graph.collision.diameter = 1;     

        graph.collision.heightMask = REFERENCENEXUS.Instance.astarRegularLayerMask;
        graph.collision.mask = REFERENCENEXUS.Instance.astarLayerMask;
        graph.characterHeight = 2.0f;     

        AstarPath.active.Scan(graph);


        return graph;
    }

    private NavGraph DetermineRelevantGraph(Vector3 position)
    {
        if (astarPath == null)
        {
    
            return null;
        }
        if (astarPath.data == null)
        {
          
            return null;
        }
        foreach (var graph in astarPath.data.graphs)
        {
            if (GraphCoversPosition(graph, position))
            {
                return graph;
            }
        }

        return null;
    }


    private bool GraphCoversPosition(NavGraph graph, Vector3 position)
    {
        if (graph is GridGraph gridGraph)
        {
            return GraphCoversPositionGrid(gridGraph, position);
        }
        else if (graph is NavmeshBase navGraph)
        {
            return GraphCoversPositionNavmesh(navGraph, position);
        }

        return false;
    }


    private bool GraphCoversPositionGrid(GridGraph graph, Vector3 position)
    {
        var node = graph.GetNearest(position).node;
        return node != null && node.Walkable;
    }

    private bool GraphCoversPositionNavmesh(NavmeshBase graph, Vector3 position)
    {
        var nearestNode = graph.GetNearest(position, NNConstraint.Default).node;
        return nearestNode != null && nearestNode.Walkable;
    }






}
