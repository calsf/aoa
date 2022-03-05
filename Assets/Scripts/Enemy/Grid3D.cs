using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Grid3D : MonoBehaviour
{
    public Node[, ,] grid { get; set; }
    private LayerMask unwalkableMask;

    // The entire grid size in world units
    [SerializeField] private Vector3 gridSizeTotal;
    public float nodeRadius;

    // The grid sizes based on the size of each node
    public int gridSizeX { get; set; }
    public int gridSizeZ { get; set; }
    public int gridSizeY { get; set; }

    public Vector3 gridBounds { get { return new Vector3(gridSizeTotal.x / 2, gridSizeTotal.y / 2, gridSizeTotal.z / 2); } }

    private Vector3 botLeft;

    public float nodeDiameter { get { return nodeRadius * 2; } }

    public UnityEvent<(int, int, int)> OnNodeWalkableUpdate;

    void Awake()
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
                    grid[x, y, z].cardinalNeighbors = GetCardinalNeighborNodes(grid, grid[x, y, z]);
                    grid[x, y, z].diagonalNeighbors = GetDiagonalNeighborNodes(grid, grid[x, y, z]);
                }
            }
        }
    }

    public void UpdateNodeWalkable(Vector3 worldPos)
    {
        bool isWalkable = Physics.CheckBox(worldPos, Vector3.one * (nodeRadius - .5f), Quaternion.Euler(0, 0, 0), unwalkableMask) ? false : true;

        (int, int, int) nodeIndex = GetNodeIndexFromPosition(worldPos);
        grid[nodeIndex.Item1, nodeIndex.Item2, nodeIndex.Item3].isWalkable = isWalkable;

        // Invoke event to update any grid copies
        OnNodeWalkableUpdate.Invoke(nodeIndex);
    }

    public (int, int, int) GetNodeIndexFromPosition(Vector3 worldPos)
    {
        // Get node index from world position, if a given world position is out of the grid bounds, set to be out of index bounds -- caller must handle out of bounds check
        int x = worldPos.x > gridSizeTotal.x ? gridSizeX : gridSizeX - 1 - (int) (Mathf.Abs(worldPos.x - gridSizeTotal.x / 2) / nodeDiameter);
        int y = worldPos.y > gridSizeTotal.y ? gridSizeY : gridSizeY - 1 - (int) (Mathf.Abs((worldPos.y - gridSizeTotal.y / 2) - transform.position.y) / nodeDiameter); // Account for any Y offset of the grid game object
        int z = worldPos.y > gridSizeTotal.z ? gridSizeZ : gridSizeZ - 1 - (int) (Mathf.Abs(worldPos.z - gridSizeTotal.z / 2) / nodeDiameter);

        // Avoid returning negative index, will still need to check for upper index bound
        x = Mathf.Clamp(x, 0, x);
        y = Mathf.Clamp(y, 0, y);
        z = Mathf.Clamp(z, 0, z);

        return (x, y, z);
    }

    public List<Node> GetCardinalNeighborNodes(Node[, ,] grid, Node node)
    {
        List<Node> neighborNodes = new List<Node>();

        // Check for neighbors in every direction from node
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    int originDimCount = 0; // Number of dimensions with 0 value to determine cardinal or diagonal node
                    originDimCount = x == 0 ? originDimCount + 1 : originDimCount;
                    originDimCount = y == 0 ? originDimCount + 1 : originDimCount;
                    originDimCount = z == 0 ? originDimCount + 1 : originDimCount;

                    // Skip if vector zero which is the original node or if is a diagonal node
                    if ((x == 0 && y == 0 && z == 0) || originDimCount <= 1)
                    {
                        continue;
                    }

                    int neighborX = node.gridXIndex + x;
                    int neighborY = node.gridYIndex + y;
                    int neighborZ = node.gridZIndex + z;

                    // Check that node is within grid bounds
                    if (neighborX >= 0 && neighborX < gridSizeX
                        && neighborY >= 0 && neighborY < gridSizeY
                        && neighborZ >= 0 && neighborZ < gridSizeZ)
                    {
                        neighborNodes.Add(grid[neighborX, neighborY, neighborZ]);
                    }
                }
            }
        }

        return neighborNodes;
    }

    public List<Node> GetDiagonalNeighborNodes(Node[,,] grid, Node node)
    {
        List<Node> neighborNodes = new List<Node>();

        // Check for neighbors in every direction from node
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    int originDimCount = 0; // Number of dimensions with 0 value to determine cardinal or diagonal node
                    originDimCount = x == 0 ? originDimCount + 1 : originDimCount;
                    originDimCount = y == 0 ? originDimCount + 1 : originDimCount;
                    originDimCount = z == 0 ? originDimCount + 1 : originDimCount;

                    // Skip if vector zero which is the original node or if is a cardinal node
                    if ((x == 0 && y == 0 && z == 0) || originDimCount >= 2)
                    {
                        continue;
                    }

                    int neighborX = node.gridXIndex + x;
                    int neighborY = node.gridYIndex + y;
                    int neighborZ = node.gridZIndex + z;

                    // Check that node is within grid bounds
                    if (neighborX >= 0 && neighborX < gridSizeX
                        && neighborY >= 0 && neighborY < gridSizeY
                        && neighborZ >= 0 && neighborZ < gridSizeZ)
                    {
                        neighborNodes.Add(grid[neighborX, neighborY, neighborZ]);
                    }
                }
            }
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
