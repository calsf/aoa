using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyGround : Enemy
{
    protected const int MAX_NODES_TO_LOOK = 10;

    // General Enemy pathfinding
    [SerializeField] protected float fCostLimit; // fCost limit for a path

    protected Grid3D grid;
    protected Node[,,] gridArrayCopy;
    protected Vector3 lastValidStartPos;
    protected Vector3 targetPos;
    protected List<Node> path;
    protected bool pathChanged;

    // Movement
    protected int currPathPos;
    protected Vector3 nextPathPos;

    protected float nextPathfind;
    protected int pathfindDelay;

    protected bool canMove;

    protected Rigidbody rb;

    protected override void Start()
    {
        base.Start();
        grid = GameObject.FindGameObjectWithTag("GridGround").GetComponent<Grid3D>(); // Use ground grid
        rb = GetComponent<Rigidbody>();

        // Create copy of grid array for this enemy to use
        gridArrayCopy = new Node[grid.gridSizeX, grid.gridSizeY, grid.gridSizeZ];
        for (int x = 0; x < grid.gridSizeX; x++)
        {
            for (int y = 0; y < grid.gridSizeY; y++)
            {
                for (int z = 0; z < grid.gridSizeZ; z++)
                {
                    gridArrayCopy[x, y, z] = new Node(x, y, z, grid.grid[x, y, z].position, grid.grid[x, y, z].isWalkable);
                }
            }
        }

        // Assign the neighbor nodes for this grid copy
        for (int x = 0; x < grid.gridSizeX; x++)
        {
            for (int y = 0; y < grid.gridSizeY; y++)
            {
                for (int z = 0; z < grid.gridSizeZ; z++)
                {
                    gridArrayCopy[x, y, z].cardinalNeighbors = grid.GetCardinalNeighborNodes(gridArrayCopy, gridArrayCopy[x, y, z]);
                    gridArrayCopy[x, y, z].diagonalNeighbors = grid.GetDiagonalNeighborNodes(gridArrayCopy, gridArrayCopy[x, y, z]);
                }
            }
        }

        lastValidStartPos = transform.position;
        canMove = true;
    }

    void OnEnable()
    {
        if (grid == null)
        {
            grid = GameObject.FindGameObjectWithTag("GridGround").GetComponent<Grid3D>();
        }

        // Update node walkable for this grid copy when original gets updated
        grid.OnNodeWalkableUpdate.AddListener((nodeIndex) => UpdateNodeWalkable(nodeIndex));
    }

    void OnDisable()
    {
        grid.OnNodeWalkableUpdate.RemoveListener((nodeIndex) => UpdateNodeWalkable(nodeIndex));
    }

    protected void UpdateNodeWalkable((int, int, int) nodeIndex)
    {
        gridArrayCopy[nodeIndex.Item1, nodeIndex.Item2, nodeIndex.Item3].isWalkable = grid.grid[nodeIndex.Item1, nodeIndex.Item2, nodeIndex.Item3].isWalkable;
    }

    void FixedUpdate()
    {
        // Check for and update last valid start node position for enemy
        (int, int, int) startNodeIndex = grid.GetNodeIndexFromPosition(transform.position);
        if (startNodeIndex.Item1 <= grid.gridSizeX - 1 && startNodeIndex.Item2 <= grid.gridSizeY - 1 && startNodeIndex.Item3 <= grid.gridSizeZ - 1)
        {
            lastValidStartPos = transform.position;
        }

        // Check for next pathfind call
        if (nextPathfind < Time.time)
        {
            // Determine next pathfind call depending on enemy aggro state
            pathfindDelay = isAggro ? Random.Range(1, 4) : 1;
            nextPathfind = Time.time + pathfindDelay;

            PathFind();
        }

        // Move
        if (canMove)
        {
            Move();
        }
        else // Stop if cannot move
        {
            Vector3 velocity = Vector3.zero - rb.velocity;
            velocity.Normalize();
 
            velocity.y = 0; // Set y velocity to 0

            rb.AddForce(velocity, ForceMode.VelocityChange);
        }
    }

    protected override void Move()
    {
        if (!isAggro && (healthCurr < healthMax || (path != null && path.Count > 0))) // Aggro when hit or when path to player is close enough
        {
            isAggro = true;
        }
        else if (isAggro) // Aggro on player
        {
            transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));

            // Get next path position
            if (path != null) // Get next position from path
            {
                // Reset if path was changed
                if (pathChanged)
                {
                    pathChanged = false;
                    currPathPos = 0;
                }

                // Once path has been fulfilled, try to go to player
                if (currPathPos > path.Count - 1)
                {
                    nextPathPos = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
                }
                else
                {
                    nextPathPos = new Vector3(path[currPathPos].position.x, transform.position.y, path[currPathPos].position.z);
                }
            }
            else // No path, just try to move to player
            {
                nextPathPos = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
            }

            // Move to next path position
            if (Vector3.Distance(transform.position, nextPathPos) > 1f || nextPathPos == new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z))
            {
                Vector3 moveDir = nextPathPos - transform.position;
                moveDir.Normalize();

                Vector3 velocity = (moveDir * moveSpeedCurr) - rb.velocity;
                velocity.y = 0; // Set y velocity to 0

                rb.AddForce(velocity, ForceMode.VelocityChange);
            }
            else
            {
                currPathPos++;
            }
        }
    }

    // --- Pathfinding ---
    protected void PathFind()
    {
        targetPos = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z) + (Vector3.up * 5); // Offset player position

        (int, int, int) startNodeIndex = grid.GetNodeIndexFromPosition(lastValidStartPos);
        (int, int, int) targetNodeIndex = grid.GetNodeIndexFromPosition(targetPos);

        // Check that the start and target nodes are valid nodes, return if not
        if (startNodeIndex.Item1 > grid.gridSizeX - 1 || startNodeIndex.Item2 > grid.gridSizeY - 1 || startNodeIndex.Item3 > grid.gridSizeZ - 1 ||
            targetNodeIndex.Item1 > grid.gridSizeX - 1 || targetNodeIndex.Item2 > grid.gridSizeY - 1 || targetNodeIndex.Item3 > grid.gridSizeZ - 1)
        {
            return;
        }

        Node startNode = gridArrayCopy[startNodeIndex.Item1, startNodeIndex.Item2, startNodeIndex.Item3];
        Node targetNode = gridArrayCopy[targetNodeIndex.Item1, targetNodeIndex.Item2, targetNodeIndex.Item3];

        // Already at target node
        if (startNode == targetNode)
        {
            return;
        }

        PriorityQueue<Node> openList = new PriorityQueue<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        // Start node
        openList.Enqueue(startNode);

        // Look for path
        int numNodesLooked = 0;
        while (!openList.IsEmpty())
        {
            // Get next node
            Node currNode = openList.Dequeue();

            // Set node as already visited
            closedList.Add(currNode);
            numNodesLooked++;

            if (currNode == targetNode) // Path found, get the final path
            {
                GetPath(startNode, targetNode);
                return;
            }
            else if (!isAggro && currNode.fCost > fCostLimit) // If not aggro and cost to reach this node is too high, we can ignore its neighbors and continue through open list
            {
                continue;
            }
            else if (numNodesLooked > MAX_NODES_TO_LOOK) // Limit number of nodes to look through and return partial path
            {
                GetPath(startNode, currNode);
                return;
            }
            else // Else, keep looking
            {
                // Look at curr node cardinal neighbors
                foreach (Node neighborNode in currNode.cardinalNeighbors)
                {
                    // Skip if neighbor is not walkable or is already checked
                    if (!neighborNode.isWalkable || closedList.Contains(neighborNode))
                    {
                        continue;
                    }

                    // Calculate move costs
                    int gCost = currNode.gCost + GetManhattanDistance(currNode, neighborNode);

                    if (gCost < neighborNode.gCost || !openList.Contains(neighborNode))
                    {
                        neighborNode.gCost = gCost; // Cost to reach this neighbor node from start state
                        neighborNode.hCost = GetManhattanDistance(neighborNode, targetNode); // Cost to reach target from this neighbor node
                        neighborNode.parent = currNode; // Set parent of neighbor so we can backtrack for final path

                        // If isAggro, no cost limit
                        // If cost to reach this node is too high, do not add to open list
                        if ((isAggro || neighborNode.fCost < fCostLimit) && !openList.Contains(neighborNode))
                        {
                            openList.Enqueue(neighborNode);
                        }
                    }
                }

                // Look at curr node diagonal neighbors
                foreach (Node neighborNode in currNode.diagonalNeighbors)
                {
                    // Skip if neighbor is not walkable or is already checked
                    if (!neighborNode.isWalkable || closedList.Contains(neighborNode))
                    {
                        continue;
                    }

                    // Check for any blocking cardinal blocks for this diagonal neighbor
                    int unwalkableCount = 0;
                    if (neighborNode.gridXIndex != 0)
                    {
                        Node cardinalNode = gridArrayCopy[neighborNode.gridXIndex, currNode.gridYIndex, currNode.gridZIndex];
                        if (!cardinalNode.isWalkable)
                        {
                            unwalkableCount++;
                        }
                    }

                    if (neighborNode.gridYIndex != 0)
                    {
                        Node cardinalNode = gridArrayCopy[currNode.gridXIndex, neighborNode.gridYIndex, currNode.gridZIndex];
                        if (!cardinalNode.isWalkable)
                        {
                            unwalkableCount++;
                        }
                    }

                    if (neighborNode.gridZIndex != 0)
                    {
                        Node cardinalNode = gridArrayCopy[currNode.gridXIndex, currNode.gridYIndex, neighborNode.gridZIndex];
                        if (!cardinalNode.isWalkable)
                        {
                            unwalkableCount++;
                        }
                    }

                    // Skip if at least 2 are blocking (realistically, a node of type (1, 1, 1) from curr should be reachable as long as < 3 are blocking but we'll cut off at 2)
                    if (unwalkableCount >= 2)
                    {
                        continue;
                    }

                    // Calculate move costs
                    int gCost = currNode.gCost + GetManhattanDistance(currNode, neighborNode);

                    if (gCost < neighborNode.gCost || !openList.Contains(neighborNode))
                    {
                        neighborNode.gCost = gCost; // Cost to reach this neighbor node from start state
                        neighborNode.hCost = GetManhattanDistance(neighborNode, targetNode); // Cost to reach target from this neighbor node
                        neighborNode.parent = currNode; // Set parent of neighbor so we can backtrack for final path

                        // If isAggro, no cost limit
                        // If cost to reach this node is too high, do not add to open list
                        if ((isAggro || neighborNode.fCost < fCostLimit) && !openList.Contains(neighborNode))
                        {
                            openList.Enqueue(neighborNode);
                        }
                    }
                }
            }
        }
    }

    protected void GetPath(Node startNode, Node targetNode)
    {
        // Find all nodes in path by backtracking through parent nodes
        List<Node> path = new List<Node>();
        Node currNode = targetNode;

        while (currNode != startNode)
        {
            path.Add(currNode);
            currNode = currNode.parent;
        }

        // Reverse path to get the path from start to target
        path.Reverse();
        this.path = path;
        this.pathChanged = true;
    }

    protected int GetManhattanDistance(Node startNode, Node targetNode)
    {
        int x = Mathf.Abs(startNode.gridXIndex - targetNode.gridXIndex);
        int y = Mathf.Abs(startNode.gridYIndex - targetNode.gridYIndex);
        int z = Mathf.Abs(startNode.gridZIndex - targetNode.gridZIndex);

        return x + y + z;
    }

    private void OnDrawGizmos()
    {
        if (grid == null || grid.grid == null)
            return;

        foreach (Node node in gridArrayCopy)
        {
            if (path != null && path.Contains(node))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(node.position, Vector3.one * 10);
            }
        }

        (int, int, int) startNodeIndex = grid.GetNodeIndexFromPosition(lastValidStartPos);
        (int, int, int) targetNodeIndex = grid.GetNodeIndexFromPosition(targetPos);

        // Check that the start and target nodes are valid nodes
        if (startNodeIndex.Item1 > grid.gridSizeX - 1 || startNodeIndex.Item2 > grid.gridSizeY - 1 || startNodeIndex.Item3 > grid.gridSizeZ - 1 ||
            targetNodeIndex.Item1 > grid.gridSizeX - 1 || targetNodeIndex.Item2 > grid.gridSizeY - 1 || targetNodeIndex.Item3 > grid.gridSizeZ - 1)
        {
            return;
        }

        Node startNode = gridArrayCopy[startNodeIndex.Item1, startNodeIndex.Item2, startNodeIndex.Item3];
        Node targetNode = gridArrayCopy[targetNodeIndex.Item1, targetNodeIndex.Item2, targetNodeIndex.Item3];

        Gizmos.color = Color.white;
        Gizmos.DrawCube(startNode.position, Vector3.one * 10);
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(targetNode.position, Vector3.one * 10);
    }
}
