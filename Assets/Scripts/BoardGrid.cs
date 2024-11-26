
using System.Collections;
using UnityEngine;

public class BoardGrid : MonoBehaviour
{
    public int rows = 7;
    public int columns = 14;
    public Cell cellPrefab  = null;
    public Cell selectedTroopCell = null;
    private Cell[,] cells;
    private GameManager gm;

    void Start()
    {
        GenerateGrid();
        StartCoroutine(InitializeTestTroops());
        gm = GameManager.Instance;
    }

    private IEnumerator InitializeTestTroops()
    {
        // Wait for the end of the frame to ensure all components are loaded and rendered.
        yield return new WaitForEndOfFrame();

        int col_for_ally = 0;
        int col_for_enemies = columns - 1;

        for (int i = 0; i < gm.allyTroopPrefabs.Count; i++) {
            Troop testTroop = Instantiate(gm.allyTroopPrefabs[i], cells[0, col_for_ally].transform.position, Quaternion.identity);

            testTroop.MoveToCell(cells[0, col_for_ally]);
            PaintPath(cells[0, col_for_ally], cells[0, col_for_ally], Team.Blue);
            col_for_ally++;
        }

        for (int i = 0; i < gm.enemyTroopPrefabs.Count; i++) {
            Troop testTroop = Instantiate(gm.enemyTroopPrefabs[i], cells[rows - 1, col_for_enemies].transform.position, Quaternion.identity);

            testTroop.MoveToCell(cells[rows - 1, col_for_enemies]);
            PaintPath(cells[rows - 1, col_for_enemies], cells[rows - 1, col_for_enemies], Team.Red);
            col_for_enemies--;
        }
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

    public void ResetGridActiveSelections()
    {
        for (int row = 0; row < rows; row++) {
            for (int col = 0; col < columns; col++) {
                cells[row, col].SetActiveSelection(Selection.Empty);
            }
        }
    }

    public void ActivateMovementSelection(Cell cell, int movement)
    {
        selectedTroopCell = cell;
        (int x, int y) gridPosition = cell.GetGridPosition();
        int x = gridPosition.x;
        int y = gridPosition.y;

        // Función local para activar la selección si está dentro de los límites
        bool ActivateMovementIfInBounds(int i, int j)
        {
            if (i >= 0 && i < cells.GetLength(0) && j >= 0 && j < cells.GetLength(1)) {
                if (cells[i, j].transform.childCount > 0) {
                    return false;
                }
                cells[i, j].SetActiveSelection(Selection.Movement);
            }
            return true;
        }
        //Arriba
        for (int j = 1; j <= movement;  j++)
        {
            if (!ActivateMovementIfInBounds(x, y + j)) break;
        }
        //Abajo
        for (int j = 1; j <= movement; j++)
        {
            if (!ActivateMovementIfInBounds(x, y - j)) break;
        }
        //Derecha
        for (int j = 1; j <= movement; j++)
        {
            if (!ActivateMovementIfInBounds(x + j, y)) break;
        }
        //Izquierda
        for (int j = 1; j <= movement; j++)
        {
            if (!ActivateMovementIfInBounds(x - j, y)) break;
        }
    }

    public void ActivateAttackSelection(Cell cell, int range)
    {
        void ActivateAttackIfInBounds(int i, int j)
        {
            if (i >= 0 && i < cells.GetLength(0) && j >= 0 && j < cells.GetLength(1)) {
                if (cells[i, j].transform.childCount > 0 && cells[i, j] != cell) {
                    if (cells[i, j].transform.GetComponentInChildren<Troop>().team != cell.transform.GetComponentInChildren<Troop>().team)
                        cells[i, j].SetActiveSelection(Selection.Attack);
                }
            }
        }

        // Loop through rows and columns within the specified range
        for (int offsetX = -range; offsetX <= range; offsetX++) {
            for (int offsetY = -range; offsetY <= range; offsetY++) {
                int currentX = cell.GetGridPosition().col + offsetX;
                int currentY = cell.GetGridPosition().row + offsetY;

                ActivateAttackIfInBounds(currentY, currentX);
            }
        }
    }

    public void MoveSelectedTroop(Cell destination)
    {
        if (selectedTroopCell != null) {
            Troop selectedTroop = selectedTroopCell.transform.GetChild(0).GetComponent<Troop>();
            if (selectedTroop != null) {
                // Move the troop
                selectedTroop.transform.SetParent(destination.transform);
                selectedTroop.MoveToCell(destination);

                // Paint the path
                PaintPath(selectedTroopCell, destination, selectedTroop.team);
                gm.UseAction();
            }
            selectedTroopCell = null;
        }
    }

    private void PaintPath(Cell start, Cell destination, Team team)
    {
        // Assuming cells have coordinates (e.g., start.x, start.y)
        int startX = start.GetGridPosition().col;
        int startY = start.GetGridPosition().row;
        int endX = destination.GetGridPosition().col;
        int endY = destination.GetGridPosition().row;

        if (startX == endX) // Vertical movement
        {
            int minY = Mathf.Min(startY, endY);
            int maxY = Mathf.Max(startY, endY);
            for (int y = minY; y <= maxY; y++) {
                cells[y, startX].SetColorTeam(team);
            }
        }
        else if (startY == endY) // Horizontal movement
        {
            int minX = Mathf.Min(startX, endX);
            int maxX = Mathf.Max(startX, endX);
            for (int x = minX; x <= maxX; x++) {
                cells[startY, x].SetColorTeam(team);
            }
        }
        else {
            Debug.LogError("Troops can only move horizontally or vertically.");
        }
    }

    public void AttackWithSelectedTroop(Cell destination)
    {
        if (selectedTroopCell != null) {
            Troop selectedTroop = selectedTroopCell.transform.GetChild(0).GetComponent<Troop>();
            Troop enemy = destination.transform.GetChild(0).GetComponent<Troop>();
            if (selectedTroop != null) {
                //Attack
                selectedTroop.Attack(enemy);
                gm.UseAction();
            }
            selectedTroopCell = null;
        }
    }
}