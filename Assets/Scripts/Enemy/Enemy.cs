using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [SerializeField] GameObject deathEffect;
 
    protected GameObject player;
    protected PlayerHealth playerHealth;
    protected GameObject enemyGameObject;

    protected bool isAggro = false;

    // Enemy props
    [SerializeField] protected EnemyObject enemy;
    protected float moveSpeedMax;
    protected float moveSpeedCurr;
    protected float healthMax;
    protected float healthCurr;
    protected float damage;

    // General Enemy pathfinding
    [SerializeField] protected float fCostLimit; // fCost limit for a path

    protected Grid3D grid;
    protected Vector3 startPos;
    protected Vector3 targetPos;
    protected List<Node> path;
    protected bool pathChanged;

    void Awake()
    {
        grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<Grid3D>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerHealth = player.GetComponent<PlayerHealth>();
        enemyGameObject = transform.GetChild(0).gameObject;

        moveSpeedMax = enemy.MOVE_SPEED_BASE;
        moveSpeedCurr = moveSpeedMax;
        healthMax = enemy.HEALTH_BASE;
        healthCurr = healthMax;
        damage = enemy.DAMAGE_BASE;
    }

    void FixedUpdate()
    {
        Move();
    }

    protected abstract void Move();

    public void Damaged(float dmg)
    {
        healthCurr -= dmg;

        if (healthCurr <= 0)
        {
            GameObject obj = Instantiate(deathEffect);
            obj.transform.position = enemyGameObject.transform.position;
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Attempt to damage player
        if (other.gameObject == player)
        {
            playerHealth.DamagePlayer(damage);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);
    }

    // --- Pathfinding ---
    protected void PathFind()
    {
        startPos = transform.position;
        targetPos = player.transform.position + (Vector3.up * 5); // Offset player position

        Node startNode = grid.GetNodeAtPosition(startPos);
        Node targetNode = grid.GetNodeAtPosition(targetPos);

        PriorityQueue<Node> openList = new PriorityQueue<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        // Start node
        openList.Enqueue(startNode);

        // Look for path
        while (!openList.IsEmpty())
        {
            // Get next node
            Node currNode = openList.Dequeue();

            // Set node as already visited
            closedList.Add(currNode);

            if (currNode == targetNode) // Path found, get the final path
            {
                GetPath(startNode, targetNode);
            }
            else if (!isAggro && currNode.fCost > fCostLimit) // If not aggro and cost to reach this node is too high, we can ignore its neighbors and continue through open list
            {
                continue;
            }
            else // Else, keep looking
            {
                foreach (Node neighborNode in currNode.neighbors) // Look at curr node neighbors
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

        foreach (Node node in grid.grid)
        {
            if (path != null && path.Contains(node))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(node.position, Vector3.one * 10);
            }
        }

        Node targetNode = grid.GetNodeAtPosition(targetPos);
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(targetNode.position, Vector3.one * 10);
    }
}
