using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid3D : MonoBehaviour
{
    public Node[, ,] grid { get; set; }
    private LayerMask unwalkableMask;

    // The entire grid size in world units
    [SerializeField] private Vector3 gridSizeTotal;
    [SerializeField] private float nodeRadius;

    // The grid sizes based on the size of each node
    private int gridSizeX;
    private int gridSizeZ;
    private int gridSizeY;

    private Vector3 botLeft;

    public float nodeDiameter { get { return nodeRadius * 2; } }

    void Start()
    {
        unwalkableMask = new LayerMask();
        unwalkableMask = 1 << LayerMask.NameToLayer("Wall");

        // Grid sizes should be based on the grid size total and the size of the nodes
        gridSizeX = Mathf.RoundToInt(gridSizeTotal.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridSizeTotal.y / nodeDiameter);
        gridSizeZ = Mathf.RoundToInt(gridSizeTotal.z / nodeDiameter);

        // Calc bottom corner of the grid
        botLeft = transform.position
            - ((Vector3.right * gridSizeTotal.x) / 2)
            - ((Vector3.up * gridSizeTotal.y) / 2)
            - ((Vector3.forward * gridSizeTotal.z) / 2);

        // Create grid
        CreateGrid();
    }

    private void CreateGrid()
    {
        // Init grid
        grid = new Node[gridSizeX, gridSizeY, gridSizeZ];

        // Init nodes in grid
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    // Calculate world position for this node
                    Vector3 worldPos = botLeft
                        + Vector3.right * (x * nodeDiameter + nodeRadius)
                        + Vector3.up * (y * nodeDiameter + nodeRadius)
                        + Vector3.forward * (z * nodeDiameter + nodeRadius);

                    // Check if node is walkable
                    bool isWalkable = Physics.CheckBox(worldPos, Vector3.one * (nodeRadius - .5f), Quaternion.Euler(0, 0, 0), unwalkableMask) ? false : true;

                    grid[x, y, z] = new Node(x, y, z, worldPos, isWalkable);
                }
            }
        }

        // Find and assign the neighbor nodes for every node
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    grid[x, y, z].neighbors = GetNeighborNodes(grid[x, y, z]);
                }
            }
        }
    }

    public void UpdateNodeWalkable(Vector3 worldPos)
    {
        bool isWalkable = Physics.CheckBox(worldPos, Vector3.one * (nodeRadius - .5f), Quaternion.Euler(0, 0, 0), unwalkableMask) ? false : true;

        GetNodeAtPosition(worldPos).isWalkable = isWalkable;
    }

    public Node GetNodeAtPosition(Vector3 worldPos)
    {
        float xPos = (worldPos.x + gridSizeTotal.x / 2) / gridSizeTotal.x;
        float yPos = (worldPos.y + gridSizeTotal.y / 2) / gridSizeTotal.y;
        float zPos = (worldPos.z + gridSizeTotal.z / 2) / gridSizeTotal.z;

        xPos = Mathf.Clamp01(xPos);
        yPos = Mathf.Clamp01(yPos);
        zPos = Mathf.Clamp01(zPos);

        int x = Mathf.RoundToInt((gridSizeX - 1) * xPos);
        int y = Mathf.RoundToInt((gridSizeY - 1) * yPos);
        int z = Mathf.RoundToInt((gridSizeZ - 1) * zPos);

        return grid[x, y, z];
    }

    public List<Node> GetNeighborNodes(Node node)
    {
        List<Node> neighborNodes = new List<Node>();
        int neighborX;
        int neighborY;
        int neighborZ;

        // Only check in cardinal directions

        // X
        neighborX = node.gridXIndex + 1;
        neighborY = node.gridYIndex;
        neighborZ = node.gridZIndex;
        if (neighborX >= 0 && neighborX < gridSizeX
            && neighborY >= 0 && neighborY < gridSizeY
            && neighborZ >= 0 && neighborZ < gridSizeZ)
        {
            neighborNodes.Add(grid[neighborX, neighborY, neighborZ]);
        }

        neighborX = node.gridXIndex - 1;
        if (neighborX >= 0 && neighborX < gridSizeX
            && neighborY >= 0 && neighborY < gridSizeY
            && neighborZ >= 0 && neighborZ < gridSizeZ)
        {
            neighborNodes.Add(grid[neighborX, neighborY, neighborZ]);
        }

        // Y
        neighborX = node.gridXIndex;
        neighborY = node.gridYIndex + 1;
        neighborZ = node.gridZIndex;
        if (neighborX >= 0 && neighborX < gridSizeX
            && neighborY >= 0 && neighborY < gridSizeY
            && neighborZ >= 0 && neighborZ < gridSizeZ)
        {
            neighborNodes.Add(grid[neighborX, neighborY, neighborZ]);
        }

        neighborY = node.gridYIndex - 1;
        if (neighborX >= 0 && neighborX < gridSizeX
            && neighborY >= 0 && neighborY < gridSizeY
            && neighborZ >= 0 && neighborZ < gridSizeZ)
        {
            neighborNodes.Add(grid[neighborX, neighborY, neighborZ]);
        }

        // Z
        neighborX = node.gridXIndex;
        neighborY = node.gridYIndex;
        neighborZ = node.gridZIndex + 1;
        if (neighborX >= 0 && neighborX < gridSizeX
            && neighborY >= 0 && neighborY < gridSizeY
            && neighborZ >= 0 && neighborZ < gridSizeZ)
        {
            neighborNodes.Add(grid[neighborX, neighborY, neighborZ]);
        }

        neighborZ = node.gridZIndex - 1;
        if (neighborX >= 0 && neighborX < gridSizeX
            && neighborY >= 0 && neighborY < gridSizeY
            && neighborZ >= 0 && neighborZ < gridSizeZ)
        {
            neighborNodes.Add(grid[neighborX, neighborY, neighborZ]);
        }

        return neighborNodes;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridSizeTotal.x, gridSizeTotal.y, gridSizeTotal.z));

        if (grid != null)
        {
            foreach (Node node in grid)
            {
                if (node.isWalkable)
                {
                    Gizmos.color = new Color(0, 0, 1, .1f);
                }
                else
                {
                    Gizmos.color = new Color(1, 0, 0, .5f);
                }

                Gizmos.DrawCube(node.position, Vector3.one * nodeDiameter);
            }
        }
    }
}
