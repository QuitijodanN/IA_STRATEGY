using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class BoardGrid : MonoBehaviour
{
    public int rows = 7;
    public int columns = 14;
    public Cell  cellPrefab  = null;

    //Cambiar por listas de aliados y enemigos
    public List<Troop> enemies;
    public List<Troop> allies;

    public Troop troopPrefab = null;

    private Cell[,] cells;
    private Cell selectedCell;
    private TurnManager turnManager;
    private TeamsManager teamsManager;

    private void Awake()
    {
        turnManager  = GameObject.Find("GameManager").GetComponent<TurnManager>();
        teamsManager = GameObject.Find("GameManager").GetComponent<TeamsManager>();

        enemies = teamsManager.equipoEnemigo;
        allies  = teamsManager.equipoAliado;
    }
    void Start()
    {
        
        GenerateGrid();
        setTrops(allies, enemies);
        //SetTestTroop(0, 0);
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

    void setTrops(List<Troop> allies, List<Troop> enemies)
    {
        int col_for_allay = 0;

        int col_for_enemies = columns-1;
        for(int i = 0;i<teamsManager.numberOfAllies;i++) 
        {
            Troop testTroop = Instantiate(teamsManager.troopPrefab1, cells[0, col_for_allay].transform.position, Quaternion.identity);
            testTroop.transform.SetParent(cells[0, col_for_allay].transform);
            testTroop.MoveToCell(cells[0, col_for_allay]);
            testTroop.turnoActtivo = true;
            col_for_allay++;
            allies.Add(testTroop);
        }

        for (int i = 0; i < teamsManager.numberOfEnemies; i++)
        {
            Troop testTroop = Instantiate(teamsManager.troopPrefab2, cells[rows-1, col_for_enemies].transform.position, Quaternion.identity);
            testTroop.transform.SetParent(cells[rows-1, col_for_enemies].transform);
            testTroop.MoveToCell(cells[rows-1, col_for_enemies]);
            col_for_enemies--;
            enemies.Add(testTroop);
        }

        turnManager.setMaxNumberActionsPerAllyTurn(teamsManager.numberOfAllies);
        turnManager.setMaxNumberActionsPerEnemyTurn(teamsManager.numberOfEnemies);

        turnManager.numeroJugadasAliadas = teamsManager.numberOfAllies;
        turnManager.numeroJugadasEnemigas = 0;
    }

    public void ResetGridActiveSelections()
    {
        for (int row = 0; row < rows; row++) {
            for (int col = 0; col < columns; col++) {
                cells[row, col].SetActiveSelection(false);
            }
        }
    }
    public void ActivateSelection(Cell cell, int movUp, int  movDown, int moveRight, int moveLeft)
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
        for (int i = 0; i < 4; i++)
        {
            switch (i) {
                case 0:
                    //Derecha
                    for (int j = 1; j <= moveRight;  j++)
                    {
                        ActivateIfInBounds(x, y + j);
                    }
                    break;
                    //Izquierda
                case 1:
                    for (int j = 1; j <= moveLeft; j++)
                    {
                        ActivateIfInBounds(x, y - j);
                    }
                    break;
                    //Abajo
                case 2:
                    for (int j = 1; j <= movDown; j++)
                    {
                        ActivateIfInBounds(x + j, y);
                    }
                    break;
                    //Arriba
                case 3:
                    for (int j = 1; j <= movUp; j++)
                    {
                        ActivateIfInBounds(x - j, y);
                    }
                    break;
            }
        }
        /*
         * No deja usar un rango
         * 
        // Activar selección en las celdas adyacentes
        ActivateIfInBounds(x, y + movUp); // Arriba
        ActivateIfInBounds(x, y - movDown); // Abajo
        ActivateIfInBounds(x + moveRight, y); // Derecha
        ActivateIfInBounds(x - moveLeft, y); // Izquierda
        */
    }

    public void MoveSelectedTroop(Cell destination)
    {
        if (selectedCell != null)
        {
            Troop selectedTroop = selectedCell.transform.GetChild(0).GetComponent<Troop>();
            if (selectedTroop != null)
            {
                selectedTroop.transform.SetParent(destination.transform);
                selectedTroop.MoveToCell(destination);

                if (teamsManager.equipoAliado.Contains(selectedTroop))
                {
                   // Debug.Log("Está en equipo all");
                    turnManager.numeroJugadasAliadas--;
                    if (turnManager.numeroJugadasAliadas <= 0)
                    {

                        turnManager.CambiarTurno();
                    }
                }
                else if (teamsManager.equipoEnemigo.Contains(selectedTroop))
                {
                  //  Debug.Log("Está en equipo en");
                    turnManager.numeroJugadasEnemigas--;
                    if (turnManager.numeroJugadasEnemigas <= 0)
                    {
                        turnManager.CambiarTurno();
                    }

                }
                selectedCell = null;
            }
        }
    }
}