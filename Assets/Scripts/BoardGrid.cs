
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
    [SerializeField] private AudioClip selectClip;
    [SerializeField] private AudioClip moveClip;

    void Start()
    {
        GenerateGrid();
        StartCoroutine(InitializeCells());
        gm = GameManager.Instance;
    }

    private IEnumerator InitializeCells()
    {
        // Wait for the end of the frame to ensure all components are loaded and rendered.
        yield return new WaitForEndOfFrame();

        // Color first and last column accordingly
        PaintPath(cells[0, 0], cells[rows - 1, 0], Team.Blue);
        PaintPath(cells[0, columns - 1], cells[rows - 1, columns - 1], Team.Red);

        // Spawn test troops
        int col_for_ally = 0;
        int col_for_enemies = columns - 1;
        for (int i = 0; i < gm.allyTroopPrefabs.Count; i++) {
            SpawnTroop(gm.allyTroopPrefabs[i], cells[0, col_for_ally], Team.Blue);
            col_for_ally++;
        }

        for (int i = 0; i < gm.enemyTroopPrefabs.Count; i++) {
            SpawnTroop(gm.enemyTroopPrefabs[i], cells[rows - 1, col_for_enemies], Team.Red);
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

    public void SpawnTroop(Troop troopPrefab, Cell cell, Team team)
    {
        Troop testTroop = Instantiate(troopPrefab, cell.transform.position, Quaternion.identity);
        testTroop.MoveToCell(cell);
        PaintPath(cell, cell, team);
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
        if (selectClip != null) {
            gm.GetComponent<AudioSource>().PlayOneShot(selectClip);
        }
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
                Troop selectedTroop = selectedTroopCell.transform.GetComponentInChildren<Troop>();
                if (selectedTroop is Tower) {
                    if (cells[i, j].GetColorTeam() != selectedTroop.team) {
                        cells[i, j].SetActiveSelection(Selection.Attack);
                    }
                }
                else {
                    if (cells[i, j].transform.childCount > 0 && cells[i, j] != cell) {
                        if (cells[i, j].transform.GetComponentInChildren<Troop>().team != selectedTroop.team)
                            cells[i, j].SetActiveSelection(Selection.Attack);
                    }
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
        if (moveClip != null) {
            gm.GetComponent<AudioSource>().PlayOneShot(moveClip);
        }
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
            if (selectedTroop != null) {
                //Attack
                if (selectedTroop is Tower) {
                    AttackWithArea(selectedTroopCell, selectedTroop.attackRange);
                } else {
                    Troop enemy = destination.transform.GetChild(0).GetComponent<Troop>();
                    selectedTroop.Attack(enemy);
                    
                }
                gm.UseAction();
            }
            selectedTroopCell = null;
        }
    }

    public void AttackWithArea(Cell cell, int range)
    {
        void AttackIfInBounds(int i, int j)
        {
            if (i >= 0 && i < cells.GetLength(0) && j >= 0 && j < cells.GetLength(1)) {
                Troop selectedTroop = cell.transform.GetChild(0).GetComponent<Troop>();
                if (cells[i, j].transform.childCount > 0 && cells[i, j] != cell) {
                    Troop enemy = cells[i, j].transform.GetComponentInChildren<Troop>();
                    if (enemy.team != selectedTroop.team) {
                        selectedTroop.Attack(enemy);
                    }
                }
                else {
                    PaintPath(cells[i, j], cells[i, j], selectedTroop.team);
                }
            }
        }

        // Loop through rows and columns within the specified range
        for (int offsetX = -range; offsetX <= range; offsetX++) {
            for (int offsetY = -range; offsetY <= range; offsetY++) {
                int currentX = cell.GetGridPosition().col + offsetX;
                int currentY = cell.GetGridPosition().row + offsetY;

                AttackIfInBounds(currentY, currentX);
            }
        }
    }
}