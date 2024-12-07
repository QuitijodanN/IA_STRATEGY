
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Audio;

public class BoardGrid : MonoBehaviour
{
    public int rows = 7;
    public int columns = 14;
    public Cell cellPrefab  = null;
    public Troop selectedTroop = null;

    private Cell[,] cells;
    public float[,] influenceMap;
    public float sumaMapaInfluencia;

    [SerializeField] private AudioClip selectClip;
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip dropClip;

    private GameManager gm;

    private void Awake()
    {
        GenerateGrid();
        StartCoroutine(InitializeCells());
    }
    void Start()
    {
        gm = GameManager.Instance;
    }

    // -----------------------------------------------------------------------------------------------------------------------------------------
    // -- INIT FUNCTIONS
    // -----------------------------------------------------------------------------------------------------------------------------------------
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
    public Cell getCell(int row, int col) {
        if (row < 0 || row >= rows || col < 0 || col >= columns)
        {
            return null;
        }
        return cells[row, col];
    }
    private IEnumerator InitializeCells()
    {
        // Wait for the end of the frame to ensure all components are loaded and rendered.
        yield return new WaitForEndOfFrame();

        // Color first and last column accordingly
        PaintPath(cells[0, 0], cells[rows - 1, 0], Team.Blue);
        PaintPath(cells[0, columns - 1], cells[rows - 1, columns - 1], Team.Red);

        /*
        // Spawn test troops
        int col_for_ally = 0;
        int col_for_enemies = columns - 1;
        for (int i = 0; i < gm.allyTroopPrefabs.Count; i++) {
            SpawnTroop(gm.allyTroopPrefabs[i], cells[0, col_for_ally]);
            col_for_ally++;
        }

        for (int i = 0; i < gm.enemyTroopPrefabs.Count; i++) {
            SpawnTroop(gm.enemyTroopPrefabs[i], cells[rows - 1, col_for_enemies]);
            col_for_enemies--;
        }
        ActualizeInfluence();*/
    }

    public void ActualizeInfluence()
    {
        sumaMapaInfluencia = 0;
        influenceMap = new float[rows, columns];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Team thisCellTeam = cells[row, col].GetColorTeam();
                int value = 0;
                if (thisCellTeam != Team.None)
                {
                    if (thisCellTeam == Team.Blue)
                        value = 1;
                    else
                        value = -1;

                    if (cells[row, col].transform.childCount > 0)
                        TroopInfluence(row, col, value);
                }
                influenceMap[row, col] += value;
                sumaMapaInfluencia += value;
            }
        }
        for (int row = 0; row < rows; row++)
        {
            string line = "";
            for (int col = 0; col < columns; col++)
            {
                line += " / " + influenceMap[row, col].ToString();
            }
            //Debug.Log(line);
        }
    }

    void TroopInfluence(int row, int col, int value)
    {
        for (int nRow = row - 2; nRow <= row + 2; nRow++)
            for (int nCol = col - 2; nCol <= col + 2; nCol++)
            {
                if (nRow >= 0 && nCol >= 0 && nRow < rows && nCol < columns)
                {
                    float distance = Mathf.Sqrt(Mathf.Pow(nCol - col, 2) + Mathf.Pow(nRow - row, 2));
                    float nValue = value * 100;
                    if (distance > 0)
                        nValue = value / (distance * 2);
                    influenceMap[nRow, nCol] += nValue;
                }
            }
    }

    // -----------------------------------------------------------------------------------------------------------------------------------------
    // -- SELECTION FUNCTIONS
    // -----------------------------------------------------------------------------------------------------------------------------------------
    public void ResetGridActiveSelections()
    {
        selectedTroop = null;
        for (int row = 0; row < rows; row++) {
            for (int col = 0; col < columns; col++) {
                cells[row, col].SetActiveSelection(Selection.Empty);
            }
        }
    }

    public void ActivateMovementSelection(Troop troop)
    {
        if (selectClip != null) {
            gm.GetComponent<AudioSource>().PlayOneShot(selectClip);
        }

        ResetGridActiveSelections();
        selectedTroop = troop;
        Cell center = troop.transform.GetComponentInParent<Cell>();
        (int x, int y) gridPosition = center.GetGridPosition();
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
        for (int j = 1; j <= troop.moveRange;  j++)
        {
            if (!ActivateMovementIfInBounds(x, y + j)) break;
        }
        //Abajo
        for (int j = 1; j <= troop.moveRange; j++)
        {
            if (!ActivateMovementIfInBounds(x, y - j)) break;
        }
        //Derecha
        for (int j = 1; j <= troop.moveRange; j++)
        {
            if (!ActivateMovementIfInBounds(x + j, y)) break;
        }
        //Izquierda
        for (int j = 1; j <= troop.moveRange; j++)
        {
            if (!ActivateMovementIfInBounds(x - j, y)) break;
        }
    }

    public void ActivateAttackSelection(Troop troop)
    {
        Cell center = troop.transform.GetComponentInParent<Cell>();
        void ActivateAttackIfInBounds(int i, int j)
        {
            if (i >= 0 && i < cells.GetLength(0) && j >= 0 && j < cells.GetLength(1)) {
                if (troop is Tower) {
                    if (cells[i, j].GetColorTeam() != troop.team) {
                        cells[i, j].SetActiveSelection(Selection.Attack);
                    }
                }
                else {
                    if (cells[i, j].transform.childCount > 0 && cells[i, j] != center) {
                        if (cells[i, j].transform.GetComponentInChildren<Troop>().team != troop.team)
                            cells[i, j].SetActiveSelection(Selection.Attack);
                    }
                }
            }
        }

        // Loop through rows and columns within the specified range
        for (int offsetX = -troop.attackRange; offsetX <= troop.attackRange; offsetX++) {
            for (int offsetY = -troop.attackRange; offsetY <= troop.attackRange; offsetY++) {
                int currentX = center.GetGridPosition().col + offsetX;
                int currentY = center.GetGridPosition().row + offsetY;

                ActivateAttackIfInBounds(currentY, currentX);
            }
        }
    }

    // -----------------------------------------------------------------------------------------------------------------------------------------
    // -- ACTION FUNCTIONS
    // -----------------------------------------------------------------------------------------------------------------------------------------
    public void SpawnTroop(Troop troopPrefab, Cell cell)
    {
        if (dropClip != null) {
            gm.audioSource.PlayOneShot(dropClip);
        }
        Troop troop = Instantiate(troopPrefab, cell.transform.position, Quaternion.identity);
        if (troop is not Bomb) {
            gm.AddTroop(troop);
        }
        troop.MoveToCell(cell);
        PaintPath(cell, cell, troopPrefab.team);
        gm.UseAction();
    }

    public void MoveTroop(Troop troop, Cell cell)
    {
        if (troop != null && cell != null) {
            if (moveClip != null) {
                gm.audioSource.PlayOneShot(moveClip);
            }
            Cell startCell = troop.transform.GetComponentInParent<Cell>();
            if (startCell.GetGridPosition().col < cell.GetGridPosition().col) {
                troop.transform.GetComponent<SpriteRenderer>().flipX = false;
            } else if (startCell.GetGridPosition().col > cell.GetGridPosition().col) {
                troop.transform.GetComponent<SpriteRenderer>().flipX = true;
            }
            troop.MoveToCell(cell);
            PaintPath(startCell, cell, troop.team);
            gm.UseAction();
        }
        ResetGridActiveSelections();
    }

    public void AttackWithTroop(Troop troop, Troop target)
    {
        if (troop != null) {
            //Attack
            if (troop is Tower) {
                AttackWithArea(troop);
                Tower tower = selectedTroop as Tower;
                tower.PlayEffect();
            }
            else {
                if (troop.transform.GetComponentInParent<Cell>().GetGridPosition().col < target.transform.GetComponentInParent<Cell>().GetGridPosition().col) {
                    troop.transform.GetComponent<SpriteRenderer>().flipX = false;
                }
                else if (troop.transform.GetComponentInParent<Cell>().GetGridPosition().col > target.transform.GetComponentInParent<Cell>().GetGridPosition().col) {
                    troop.transform.GetComponent<SpriteRenderer>().flipX = true;
                }
                selectedTroop.Attack(target);
            }
            gm.UseAction();
        }
        ResetGridActiveSelections();
    }

    public void AttackWithArea(Troop troop)
    {
        Cell center = troop.transform.GetComponentInParent<Cell>();
        void AttackIfInBounds(int i, int j)
        {
            if (i >= 0 && i < cells.GetLength(0) && j >= 0 && j < cells.GetLength(1)) {
                if (cells[i, j].transform.childCount > 0 && cells[i, j] != center) {
                    Troop enemy = cells[i, j].transform.GetComponentInChildren<Troop>();
                    if (enemy.team != troop.team) {
                        troop.Attack(enemy);
                    }
                }
                else {
                    PaintPath(cells[i, j], cells[i, j], troop.team);
                }
            }
        }

        // Loop through rows and columns within the specified range
        for (int offsetX = -troop.attackRange; offsetX <= troop.attackRange; offsetX++) {
            for (int offsetY = -troop.attackRange; offsetY <= troop.attackRange; offsetY++) {
                int currentX = center.GetGridPosition().col + offsetX;
                int currentY = center.GetGridPosition().row + offsetY;

                AttackIfInBounds(currentY, currentX);
            }
        }
    }

    // -----------------------------------------------------------------------------------------------------------------------------------------
    // -- COLOR FUNCTIONS
    // -----------------------------------------------------------------------------------------------------------------------------------------

    public void PaintPath(Cell start, Cell destination, Team team)
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
        gm.UpdateColorCells();
    }

    public int GetColorCellAmount(Team team)
    {
        int counter = 0;
        for (int i = 0; i < cells.GetLength(0); i++) {
            for (int j = 0; j < cells.GetLength(1); j++) {
                if (cells[i, j].GetColorTeam() == team) {
                    counter++;
                }
            }
        }
        return counter;
    }

    public BoardGrid CopiaProfunda()
    {
        using (MemoryStream ms = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, this);
            ms.Position = 0;
            return (BoardGrid)formatter.Deserialize(ms);
        }
    }
}