using UnityEngine;

public class BoardGrid : MonoBehaviour
{
    public int rows = 7;
    public int columns = 14;
    
    public Cell cellPrefab = null;

    private Cell[,] cells;

    void Start()
    {
        GenerateGrid();
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
                cell.SetPosition(this, row, col);

                // Almacenar la celda en la matriz
                cells[row, col] = cell;
            }
        }
    }

    public void ResetGridActiveSelections()
    {
        for (int row = 0; row < rows; row++) {
            for (int col = 0; col < columns; col++) {
                cells[row, col].SetActiveSelection(false);
            }
        }
    }
}