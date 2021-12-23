using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Node : IComparable<Node>
{
    // X, Y, Z indices in Grid
    public int gridXIndex { get; set; }
    public int gridYIndex { get; set; }
    public int gridZIndex { get; set; }

    public Vector3 position { get; set; } // Position in world space
    public bool isWalkable { get; set; } // If is walkable

    public Node parent { get; set; } // Parent node to travel from
    public List<Node> cardinalNeighbors { get; set; } // Cardinal neighbor nodes

    public List<Node> diagonalNeighbors { get; set; } // Diagonal neighbor nodes

    public int gCost { get; set; } // Cost to reach this node from start pos
    public int hCost { get; set; } // Cost from this node to target pos
    public int fCost { get { return gCost + hCost; } } // Total cost of G + H

    public Node(int gridXIndex, int gridYIndex, int gridZIndex, Vector3 position, bool isWalkable)
    {
        this.gridXIndex = gridXIndex;
        this.gridYIndex = gridYIndex;
        this.gridZIndex = gridZIndex;
        this.position = position;
        this.isWalkable = isWalkable;
    }

    // Lower fCost or same fCost but lower hCost gives more priority to Node
    public int CompareTo(Node other)
    {
        if (this.fCost < other.fCost || (this.fCost == other.fCost && this.hCost < other.hCost))
        {
            return -1; // More priority
        }
        else
        {
            return 1; // Less priority
        }
    }
}
