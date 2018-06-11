using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorCell
{
    public enum FloorCellType
    {
        ROOM_CELL,
        CORRIDOR_CELL,
        VOID_CELL
    }

    // Cell Type
    public FloorCellType type;

    // Position
    public Vector3Int position;

    // PathFinding
    public int MinCostToStart;
    public FloorCell nearestTileToStart;
    public List<Connection> connections = new List<Connection>();
    public float StraightLineDistanceToEnd = -1;

    // Visited Flag
    public bool visited = false;

    public struct Connection
    {
        public FloorCell ConnectedNode;
        public int cost;
    }

    public FloorCell(Vector3Int position)
    {
        this.position = position;
    }

    public bool isWithinBoundaries(Vector3Int firstVert, Vector3Int lastVert)
    {
        bool isWithin = false;

        int x = position.x;
        int z = position.z;
        // Quitar los iguales arreglo un bug
        bool isLesser = position.x > firstVert.x && position.z > firstVert.z;
        bool isNotBigger = position.x < lastVert.x && position.z < lastVert.z;

        isWithin = isLesser && isNotBigger;

        return isWithin;
    }
}
