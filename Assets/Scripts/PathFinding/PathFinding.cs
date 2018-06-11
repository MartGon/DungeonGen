using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding {

    public static List<FloorCell> Astar(FloorCell firstPoint, FloorCell endPoint, Dictionary<Vector3Int, FloorCell> FloorCellDictionary, DungeonNode endNode = null)
    {
        initGraph(FloorCellDictionary);

        List<Vector3Int> keys = new List<Vector3Int>(FloorCellDictionary.Keys);

        foreach (var tileKey in keys)
        {
            FloorCellDictionary[tileKey].StraightLineDistanceToEnd = (tileKey - endPoint.position).magnitude;
        }

        firstPoint.MinCostToStart = 0;
        List<FloorCell> prioQueue = new List<FloorCell>();
        prioQueue.Add(firstPoint);

        do
        {
            prioQueue.Sort((x, y) => (x.MinCostToStart + x.StraightLineDistanceToEnd).CompareTo(y.MinCostToStart + y.StraightLineDistanceToEnd));
            FloorCell node = prioQueue[0];
            prioQueue.Remove(node);

            List<FloorCell.Connection> connections = node.connections;
            connections.Sort((x, y) => x.cost.CompareTo(y.cost));

            foreach (FloorCell.Connection cnn in node.connections)
            {
                FloorCell childNode = cnn.ConnectedNode;
                if (childNode.visited)
                    continue;

                if (childNode.MinCostToStart == -1 || node.MinCostToStart + cnn.cost + getTurningCost(node, childNode) < childNode.MinCostToStart)
                {
                    childNode.MinCostToStart = node.MinCostToStart + cnn.cost + getTurningCost(node, childNode);
                    childNode.nearestTileToStart = node;
                    if (!prioQueue.Contains(childNode))
                        prioQueue.Add(childNode);
                }
            }
            node.visited = true;

            if (node.position == endPoint.position)
                break;
            // Quitar este if para modo normal
            if (endNode != null)
                if (node.type == FloorCell.FloorCellType.ROOM_CELL || node.type == FloorCell.FloorCellType.CORRIDOR_CELL)
                {
                    Vector3Int firstNodeVert = new Vector3Int(endNode.startX, 0, endNode.startZ);
                    Vector3Int lastNodeVert = new Vector3Int(endNode.endX, 0, endNode.endZ);
                    if(node.isWithinBoundaries(firstNodeVert, lastNodeVert))
                    {
                        //Debug.Log("La primera es " + firstPoint.position + " el final es " + node.position);
                        endPoint = node;
                        break;
                    }
                }

        } while (prioQueue.Count != 0);

        //Debug.Log("Coste al inicio" + endPoint.MinCostToStart);

        List<FloorCell> roadPath = new List<FloorCell>();
        BuildShortestPath(ref roadPath, endPoint);
        roadPath.Remove(firstPoint);
        roadPath.Remove(endPoint);

        return roadPath;
    }

    private static void BuildShortestPath(ref List<FloorCell> list, FloorCell node)
    {
        if (node.nearestTileToStart == null)
            return;
        list.Add(node.nearestTileToStart);
        BuildShortestPath(ref list, node.nearestTileToStart);
    }

    static void initGraph(Dictionary<Vector3Int, FloorCell> FloorCellDictionary)
    {
        List<Vector3Int> keys = new List<Vector3Int>(FloorCellDictionary.Keys);

        foreach (var tileKey in keys)
        {
            FloorCell tile = FloorCellDictionary[tileKey];
            tile.MinCostToStart = -1;
            tile.nearestTileToStart = null;
            tile.StraightLineDistanceToEnd = -1;
            tile.visited = false;

            List<FloorCell> neighbours = DungeonGenerator.getNeighbourList(tile, FloorCellDictionary);

            foreach (FloorCell nei in neighbours)
            {
                FloorCell.Connection connection = new FloorCell.Connection();
                connection.cost = 10;
                connection.ConnectedNode = nei;
                tile.connections.Add(connection);
            }
        }
    }

    static int getTurningCost(FloorCell currentCell, FloorCell nextCell)
    {
        int turningCost = 0;

        if (currentCell.nearestTileToStart == null)
            return 0;

        Vector3 diffWithPrevious = currentCell.position - currentCell.nearestTileToStart.position;
        Vector3 diffWithNext = currentCell.position - nextCell.position;

        if(Mathf.Abs(diffWithNext.x) != Mathf.Abs(diffWithPrevious.x))
        {
            //Debug.Log("Es un giro");
            turningCost = 2;
        }

        return turningCost;
    }


}
