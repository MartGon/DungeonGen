using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room{

    // Room Size
    public Vector3Int firstVert = new Vector3Int();
    public Vector3Int lastVert = new Vector3Int();

    // FloorCells
    public Dictionary<Vector3Int, FloorCell> floorCells = new Dictionary<Vector3Int, FloorCell>();

    // GameObject Room si lo tiene
    public GameObject roomGameObject;

    // Corridors
    public List<Corridor> exitCorridors =  new List<Corridor>();

    // Parent Node
    public DungeonNode parentNode;

    // Room type
    public RoomType type = RoomType.STANDARD;

    public enum RoomType
    {
        STANDARD,
        KEY_ROOM,
        PLAYER_ROOM,
        FINISH_ROOM,
        TREASURE_ROOM
    }

    // Room sizes
    public int getRoomWidth()
    {
        return firstVert.x - lastVert.x;
    }

    public int getRoomDepth()
    {
        return firstVert.z - lastVert.z;
    }

    public int getRoomArea()
    {
        return getRoomWidth() * getRoomDepth();
    }

    public FloorCell getCentricFloorCell()
    {
        // Si no hay celdas no hay nada que hacer
        if (floorCells.Keys.Count == 0)
        {
            Debug.Log("No hay celdas para esta sala");
            return null;
        }

        Vector3 vector = lastVert - firstVert;
        vector.Scale(new Vector3(0.5f, 0, 0.5f));

        Vector3Int centricPoint = new Vector3Int(Mathf.RoundToInt(vector.x), 0, Mathf.RoundToInt(vector.z)) + firstVert;

        if (floorCells.ContainsKey(centricPoint))
        {
            //Debug.Log("Lo contiene!!! " + centricPoint);
            return floorCells[centricPoint];
        }
        else
        {
            Debug.Log("Error, no lo contiene!!! " + centricPoint);
        }

        return null;
    }

    public void printFloorCells()
    {
        foreach (FloorCell cell in floorCells.Values)
        {
            Debug.Log(cell.position);
        }
    }

    public Vector3Int getRandomPosition()
    {
        List<Vector3Int> positionList = new List<Vector3Int>(floorCells.Keys);
        int randomIndex = Random.Range(0, positionList.Count);

        return positionList[randomIndex] * 6 + new Vector3Int(3, 0, 3);
    }
}
