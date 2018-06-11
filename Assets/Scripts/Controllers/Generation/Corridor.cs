using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corridor
{
    // FloorCells
    public Dictionary<Vector3Int, FloorCell> floorCells = new Dictionary<Vector3Int, FloorCell>();

    public FloorCell firstCell;
    public FloorCell lastCell;

    public Room originRoom;
    public Room destRoom;

    public Gate firstGate;
    public Gate lastGate;

    public GameObject corridorGameObject;

    // Return length
    public int getLenght()
    {
        return floorCells.Keys.Count;
    }

    public void setCorridorCells(List<FloorCell> floorCellsList)
    {
        // TODO - Revisar esto -> ¡Resulta que funciona!
        firstCell = floorCellsList[floorCellsList.Count - 1];
        lastCell = floorCellsList[0];

        foreach(FloorCell cell in floorCellsList)
        {
            floorCells.Add(cell.position, cell);
        }
    }
}
