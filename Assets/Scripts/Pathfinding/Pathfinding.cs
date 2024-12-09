using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Pathfinding : MonoBehaviour {
	
	PathRequestManager requestManager;
	Grid grid;
	
	void Awake() {
		requestManager = GetComponent<PathRequestManager>();
		grid = GetComponent<Grid>();
	}
	
	
	public void StartFindPath((int, int) startPos, (int, int) targetPos, bool longDistance) {
		Node nodeStartPos = grid.grid[startPos.Item1, startPos.Item2];
        Node nodeTargetPos = grid.grid[targetPos.Item1, targetPos.Item2];

        StartCoroutine(FindPath(nodeStartPos, nodeTargetPos, longDistance));
	}

    IEnumerator FindPath(Node startNode, Node targetNode, bool longDistance) {

		Node[] waypoints = new Node[0];
		bool pathSuccess = false;		
		
		if (startNode.walkable && targetNode.walkable) {
			Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
			HashSet<Node> closedSet = new HashSet<Node>();
			openSet.Add(startNode);
			
			while (openSet.Count > 0) {
				Node currentNode = openSet.RemoveFirst();
				closedSet.Add(currentNode);
				
				if (currentNode == targetNode) {
					pathSuccess = true;
					break;
				}
				
				foreach (Node neighbour in grid.GetNeighbours(currentNode)) {
					if (!neighbour.walkable || closedSet.Contains(neighbour)) {
						continue;
					}
					
					if (longDistance)
					{
                        int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                        if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                        {
                            neighbour.gCost = newMovementCostToNeighbour;
                            neighbour.hCost = GetDistance(neighbour, targetNode);
                            neighbour.parent = currentNode;

                            if (!openSet.Contains(neighbour))
                                openSet.Add(neighbour);
                        }
                    }
					else if (DistanceOne(currentNode, neighbour))
					{
                        int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                        if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                        {
                            neighbour.gCost = newMovementCostToNeighbour;
                            neighbour.hCost = GetDistance(neighbour, targetNode);
                            neighbour.parent = currentNode;

                            if (!openSet.Contains(neighbour))
                                openSet.Add(neighbour);
                        }
                    }
					
				}
			}
		}
		yield return null;
		if (pathSuccess) {
			waypoints = RetracePath(startNode,targetNode);
		}
		requestManager.FinishedProcessingPath(waypoints,pathSuccess, longDistance);
		
	}
	
	Node[] RetracePath(Node startNode, Node endNode) {
		List<Node> path = new List<Node>();
		Node currentNode = endNode;
		
		while (currentNode != startNode) {
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}
		Node[] reversedPath = new Node[path.Count];

        for (int i = path.Count - 1, j = 0; i > -1; i--, j++) { 
			reversedPath[j] = path[i];
		}
		return reversedPath;
		
	}
	bool DistanceOne(Node nodeA, Node nodeB)
	{
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

		if (dstX > 1 || dstY > 1)
			return false;
		return true;
    }
	
	int GetDistance(Node nodeA, Node nodeB) {
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
		
		if (dstX > dstY)
			return 14*dstY + 10* (dstX-dstY);
		return 14*dstX + 10 * (dstY-dstX);
	}
	
	
}
