using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonNode {

    public char NodeId;

    // Cell Size
    public int startX;
    public int startZ;
    public int endX;
    public int endZ;

    public Room room;

    public int nodeLevel;

    // Children nodes
    public List<DungeonNode> children;

    // Parent
    public DungeonNode parent;

    // Brother
    public DungeonNode brother;

    // Corridor
    public Corridor corridorToBro;

    // Room List
    public List<Room> roomsInNode = new List<Room>();

    // Administrative distance to start - How many times did we haveg to iterate until finding the appropiate dummy starting room
    public int distanceToStartRoom = 0;

    public GameObject nodeGameObject;

    // Cell sizes
    public int getWidth()
    {
        return endX - startX;
    }

    public int getDepth()
    {
        return endZ - startZ;
    }

    public int getArea()
    {
        return getDepth() * getWidth();
    }

    public int getTotalChildrenCount()
    {
        int childrenTotal = 0;
        Stack<DungeonNode> childrenToExplore = new Stack<DungeonNode>();

        if (children == null)
            return childrenTotal;

        childrenToExplore.Push(children[0]);
        childrenToExplore.Push(children[1]);
        childrenTotal = 2;

        DungeonNode currentChildren;

        while(childrenToExplore.Count != 0)
        {
            currentChildren = childrenToExplore.Pop();
            if(currentChildren.children != null)
            {
                childrenTotal += 2;
                childrenToExplore.Push(currentChildren.children[0]);
                childrenToExplore.Push(currentChildren.children[1]);
            }
        }

        return childrenTotal;
    }

    public List<DungeonNode> getTotalChildren()
    {
        int childrenTotal = 0;
        Stack<DungeonNode> childrenToExplore = new Stack<DungeonNode>();
        List<DungeonNode> totalChildren = new List<DungeonNode>();

        if (children == null)
            return totalChildren;

        childrenToExplore.Push(children[0]);
        childrenToExplore.Push(children[1]);
        childrenTotal = 2;

        DungeonNode currentChildren;

        while (childrenToExplore.Count != 0)
        {
            currentChildren = childrenToExplore.Pop();
            totalChildren.Add(currentChildren);
            if (currentChildren.children != null)
            {
                childrenTotal += 2;
                childrenToExplore.Push(currentChildren.children[0]);
                childrenToExplore.Push(currentChildren.children[1]);
            }
        }

        return totalChildren;
    }

    public void addRoomToNode(Room roomToAdd)
    {
        roomsInNode.Add(roomToAdd);
        if (parent != null)
            parent.addRoomToNode(roomToAdd);
    }
}
