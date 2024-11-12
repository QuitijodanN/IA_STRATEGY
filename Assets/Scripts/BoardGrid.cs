using UnityEngine;

public class BoardGrid : MonoBehaviour
{
    public int rows = 7;
    public int columns = 14;
    
    public Cell cellPrefab = null;
    public Troop troopPrefab = null;

    private Cell[,] cells;
    private Cell selectedCell;

    void Start()
    {
        GenerateGrid();
        SetTestTroop(0, 0);
    }

    void GenerateGrid()
    {
        cells = new Cell[rows, columns];

        for (int row = 0; row < rows; row++) {
            for (int col = 0; col < columns; col++) {
                // Crear una instancia del prefab de celda
                Cell cell = Instantiate(cellPrefab, new Vector3(col - 6.5f, -row + 3.5f, 0), Quaternion.identity);
                // Establecer el objeto como hijo de la cuadrícula para mantener la jerarquía limpia
                cell.transform.SetParent(transform);
                cell.SetGridPosition(this, row, col);

                // Almacenar la celda en la matriz
                cells[row, col] = cell;
            }
        }
    }

    void SetTestTroop(int row, int col)
    {
        Troop testTroop = Instantiate(troopPrefab, cells[row, col].transform.position, Quaternion.identity);
        testTroop.transform.SetParent(cells[row, col].transform);
        testTroop.MoveToCell(cells[row, col]);
    }

    public void ResetGridActiveSelections()
    {
        for (int row = 0; row < rows; row++) {
            for (int col = 0; col < columns; col++) {
                cells[row, col].SetActiveSelection(false);
            }
        }
    }
    public void ActivateSelection(Cell cell)
    {
        selectedCell = cell;
        Vector2 gridPosition = cell.GetGridPosition();
        int x = (int)gridPosition.x;
        int y = (int)gridPosition.y;

        // Función local para activar la selección si está dentro de los límites
        void ActivateIfInBounds(int i, int j)
        {
            if (i >= 0 && i < cells.GetLength(0) && j >= 0 && j < cells.GetLength(1)) {
                cells[i, j].SetActiveSelection(true);
            }
        }

        // Activar selección en las celdas adyacentes
        ActivateIfInBounds(x, y + 1); // Arriba
        ActivateIfInBounds(x, y - 1); // Abajo
        ActivateIfInBounds(x + 1, y); // Derecha
        ActivateIfInBounds(x - 1, y); // Izquierda
    }

    public void MoveSelectedTroop(Cell destination)
    {
        if (selectedCell != null) {
            Troop selectedTroop = selectedCell.transform.GetChild(0).GetComponent<Troop>();
            if (selectedTroop != null) {
                selectedTroop.transform.SetParent(destination.transform);
                selectedTroop.MoveToCell(destination);
            }
            selectedCell = null;
        }
    }
}