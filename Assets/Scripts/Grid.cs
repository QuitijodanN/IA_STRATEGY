using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour {

	[SerializeField] BoardGrid boardGrid;

    public Node[,] grid;

	float nodeDiameter;
    [SerializeField]  int gridSizeX, gridSizeY;

	void Start() {
		gridSizeY = boardGrid.rows;
        gridSizeX = boardGrid.columns;

        CreateGrid();
	}

	public int MaxSize {
		get {
			return gridSizeX * gridSizeY;
		}
	}

	void CreateGrid() {
		grid = new Node[gridSizeY,gridSizeX];

		for (int y = 0; y < gridSizeY; y ++) {
			for (int x = 0; x < gridSizeX; x ++) {
                grid[y,x] = new Node(true, x,y);
			}
		}
	}

	public List<Node> GetNeighbours(Node node) {
		List<Node> neighbours = new List<Node>();

		for (int x = -2; x <= 2; x++) {
			for (int y = -2; y <= 2; y++) {
				if (x == 0 && y == 0)
					continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
					if (checkX == node.gridX || checkY == node.gridY)
						neighbours.Add(grid[checkY,checkX]);
				}
			}
		}

		return neighbours;
	}
}