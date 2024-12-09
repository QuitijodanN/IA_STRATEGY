
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;
using static UnityEngine.UI.Image;

public struct IAInfo
{
    public List<Troop> enemyTeam;
    public List<Troop> allyTeam;
    public Troop selectedTroop;
    public Troop selectedEnemyTroop;
}

public class IAEnemy : MonoBehaviour
{

    IANode n_root;
    GameManager gm;
    private float thinkTimer = 0f; // Temporizador

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gm = GameManager.Instance;
        InitializeIA();
        Think();
    }

    private void Update()
    {
        if (!gm.yourTurn) {
            thinkTimer += Time.deltaTime;

            if (thinkTimer >= 0.5f) {
                Think(); // Llamar a la función Think
                thinkTimer = 0f; // Reiniciar el temporizador
            }
        } else {
            thinkTimer = 0f;
        }
    }

    void Think()
    {
        if (!gm.yourTurn) {
            n_root.Action();
        }
    }


    private void InitializeIA()
    {
        // iniciamos nodos hoja (acciones)
        IADeploy    deploy  = new IADeploy();
        IAAttack    attack  = new IAAttack();
        IASkipTurn  skip    = new IASkipTurn();
        IAMove      move    = new IAMove();


        // atamos nodos a los padres (sequence nodes)
        IACanAttack     canAttack   = new IACanAttack(attack,move);
        IAHaveTroops    haveTroops  = new IAHaveTroops(canAttack,skip);
        IAEnoughGold    enoughGold  = new IAEnoughGold(deploy, haveTroops);

        n_root = enoughGold;
    }

    // -----------------------------------------------------------------------------------------------------------------------------------------
    // -- NODOS DECISIONES
    // -----------------------------------------------------------------------------------------------------------------------------------------
    class IAEnoughGold : IASequenceNode
    {
        public IAEnoughGold(IANode nodeTrue, IANode nodeFalse) : base(nodeTrue, nodeFalse) { }

        public override void Action()
        {
            if (GameManager.Instance.GetCoins(Team.Red) > 2) {
                n_true.Action();
            }
            else {
                n_false.Action();
            }
        }
    }

    class IAHaveTroops : IASequenceNode
    {
        public IAHaveTroops(IANode nodeTrue, IANode nodeFalse) : base(nodeTrue, nodeFalse) { }

        public override void Action()
        {
            /*
             Si tenemos tropas pregumntar si podemos atacar
             else SKIP
             */

            if(GameManager.Instance.enemyTroops.Count > 0)
                n_true.Action();
            else 
                n_false.Action(); 
        }
    }

    class IACanAttack : IASequenceNode
    {
        public IACanAttack(IANode nodeTrue, IANode nodeFalse) : base(nodeTrue, nodeFalse) { }

        public override void Action()
        {
            /*
             Si puede atacar nodo hoja hit
             else Nodo elección pintar
             */
            foreach (Troop t in GameManager.Instance.enemyTroops)
            {
                //Debug.Log(t.transform.parent.name);
            }
            
        }
    }




    // -----------------------------------------------------------------------------------------------------------------------------------------
    // -- NODOS HOJA
    // -----------------------------------------------------------------------------------------------------------------------------------------
    class IAAttack : IANode
    {
        public override void Action()
        {
            Debug.Log("Atacar");
            //GameManager.Instance.UseAction();
        }
    }

    class IADeploy : IANode
    {
        float valorInfluencia;
        float previuosInfluence;
        (int, int) posicionDeploy;
        private List<(int, int)> ObtenerPosicionesValidas(Troop tropa, BoardGrid tablero)
        {
            List<(int, int)> posibleDeploypos = new List<(int, int)>();

            //Si es una barril
            if (tropa is Bomb)
            {
                for(int i = 0; i < tablero.rows; i++)
                {
                    for(int j = 0; j < tablero.columns; j++)
                    {
                        if (tablero.getCell(i, j).GetColorTeam() == Team.Red || tablero.getCell(i, j).GetColorTeam() == Team.None)
                        {
                            if(tablero.getCell(i, j).transform.childCount == 0)
                            {
                                posibleDeploypos.Add((i, j));
                            }
                        } 
                           
                    }
                }
            }
            //El resto
            else
            {
                for (int i = 0; i < tablero.rows; i++)
                {
                    for (int j = 0; j < tablero.columns; j++)
                    {
                        if (tablero.getCell(i, j).GetColorTeam() == Team.Red && tablero.getCell(i, j).transform.childCount == 0)
                            posibleDeploypos.Add((i, j));
                    }
                }
            }
            return posibleDeploypos;
        }
        
        private float[,] DeepCopyMapaInf(float[,] mapaOr)
        {
            int rows = mapaOr.GetLength(0);
            int cols = mapaOr.GetLength(1);
            float[,] copy = new float[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    copy[i, j] = mapaOr[i, j];
                }
            }

            return copy;
        }

        private Cell[,] DeepCopyBoard(Cell[,] ogBoard)
        {
            int rows = ogBoard.GetLength(0);
            int cols = ogBoard.GetLength(1);
            Cell[,] copy = new Cell[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    copy[i, j] = ogBoard[i, j];
                }
            }

            return copy;
        }
       

        public void ActualizeInfluence(float[,] influenceMap, float sumaMapaInfluencia, Cell[,] cells)
        {
            int rows = cells.GetLength(0);
            int columns = cells.GetLength(1);


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
                            TroopInfluence(row, col, value,influenceMap);
                    }
                    influenceMap[row, col] += value;
                    sumaMapaInfluencia += value;
                }
            }
        }
        void TroopInfluence(int row, int col, int value, float[,] influenceMap)
        {
            int rows = influenceMap.GetLength(0);
            int columns = influenceMap.GetLength(1);

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
        private ((float,Troop),(int,int)) ObtenerPosicionPorInfluencia(Troop tropa, List<(int,int)> posicionesParaComp, BoardGrid tablero)
        {

            valorInfluencia = tablero.sumaMapaInfluencia;
            float copySumaInfluencia = valorInfluencia;
            previuosInfluence = copySumaInfluencia;


            //Caballero
            /*
             Ataque Mele(1 pos)
             Se mueve dos pos
             
             */
            if (tropa == GameManager.Instance.enemyTroopPrefabs[0])
            {
                //Creamos un tablero de pruebas
                //Y un mapa de pruebas
                float[,] mapaCopy = DeepCopyMapaInf(tablero.influenceMap);
                Cell[,] tableroCopy = DeepCopyBoard(tablero.GetBoard());
               


                //Recorremos todas las pos validas
                foreach ((int,int) pos in posicionesParaComp)
                {
                    //Comprobar si esta a mele
                    for (int x = -1;x < 2; x++)
                    {
                        for(int y = -1;y < 2; y++)
                        {
                            if (x ==  0 && y == 0 ) continue;

                            if(pos.Item1 + x >= 0 && pos.Item1 + x < tableroCopy.GetLength(0) && pos.Item2 + y >= 0 && pos.Item2 + y < tableroCopy.GetLength(1))
                                if (tableroCopy[pos.Item1+x,pos.Item2+y].transform.childCount != 0)
                                {
                                    if (tableroCopy[pos.Item1 + x, pos.Item2 + y].transform.GetChild(0).GetComponent<Troop>().team == Team.Blue)
                                    {
                                       
                                        copySumaInfluencia -= 10f;
                                    }
                                       
                                }
                                else
                                {
                                    
                                    copySumaInfluencia -= 3f;
                                }
                                  
                                
                        }
                    }

                    if(copySumaInfluencia < previuosInfluence)
                    {
                        previuosInfluence = copySumaInfluencia;
                        posicionDeploy = pos;
                    }

                    copySumaInfluencia = valorInfluencia;


                }
              
            }

            //Arquero
            if (tropa == GameManager.Instance.enemyTroopPrefabs[1])
            {
                //Creamos un tablero de pruebas
                //BoardGrid copiaTablero = tablero.CopiaProfunda();
                // Crear una nueva instancia del prefab o del objeto original
                float[,] mapaCopy = DeepCopyMapaInf(tablero.influenceMap);
                Cell[,] tableroCopy = DeepCopyBoard(tablero.GetBoard());
                

                //Recorremos todas las pos validas
                foreach ((int, int) pos in posicionesParaComp)
                {


                    //Comprobar si puede atacar enemigos
                    for (int x = -1; x < 2; x++)
                    {
                        for (int y = -1; y < 2; y++)
                        {
                            if (x == 0 && y == 0) continue;



                            if (pos.Item1 + x*2 >= 0 && pos.Item1 + x*2 < tableroCopy.GetLength(0) && pos.Item2 + y*2 >= 0 && pos.Item2 + y * 2 < tableroCopy.GetLength(1))
                            {
                               
                                if (tableroCopy[pos.Item1 + x * 2, pos.Item2 + y * 2].transform.childCount != 0)
                                {
                                    if (tableroCopy[pos.Item1 + x * 2, pos.Item2 + y * 2].transform.GetChild(0).GetComponent<Troop>().team == Team.Blue)
                                    {

                                       
                                        copySumaInfluencia -= 10f;
                                            
                                        
                                    }

                                }
                               
                                else
                                {
                                   
                                    copySumaInfluencia -= 4f;
                                }       
                            }
                        }
                    }

                    if (copySumaInfluencia <= previuosInfluence)
                    {
                        previuosInfluence = copySumaInfluencia;
                        posicionDeploy = pos;
                    }

                    copySumaInfluencia = valorInfluencia;

                }

            }
            //Torre
            //Solo si tenemos 2 o mas tropas
            //Se tiene que colocar en zonas seguras
            if (tropa is Tower && GameManager.Instance.enemyTroops.Count >= 3)
            {
                float seguridad = 0;
                //Creamos un tablero de pruebas
                //BoardGrid copiaTablero = tablero.CopiaProfunda();
                // Crear una nueva instancia del prefab o del objeto original
                float[,] mapaCopy = DeepCopyMapaInf(tablero.influenceMap);
                Cell[,] tableroCopy = DeepCopyBoard(tablero.GetBoard());

                //Recorremos todas las pos validas
                foreach ((int, int) pos in posicionesParaComp)
                {
                  

                    //Comprobar si puede pintar o si tiene enemigos cerca
                    for (int x = -2; x < 3; x++)
                    {
                        for (int y = -2; y < 3; y++)
                        {
                            if (x == 0 && y == 0) continue;

                            //Cuanto más negativo más seguro
                            //Cuanto más cercano al cero más para pintar
                            if (pos.Item1 + x >= 0 && pos.Item1 + x < tableroCopy.GetLength(0) && pos.Item2 + y >= 0 && pos.Item2 + y < tableroCopy.GetLength(1))
                            {
                              
                                seguridad += mapaCopy[pos.Item1 + x, pos.Item2 + y];
                            }
                              
                            


                        }
                    }
                    //Las torres son defensivas por lo que no las queremos muy alejadas de nuestra influencia
                    //Posición ideal sitios para pintar y seguro
                   if(-1 < seguridad && seguridad <= 0)
                    {
                        copySumaInfluencia -= 50 + seguridad*3;
                    }
                    //Segunda mejor pos no se puede pintar mucho o nada pero es seguro
                    else if(seguridad < -1)
                    {
                       copySumaInfluencia -= 5;
                    }
                   //Peor posición mucha influencia enemiga
                    else
                    {
                        copySumaInfluencia += 50;
                    }


                    if (copySumaInfluencia <= previuosInfluence)
                    {
                        previuosInfluence = copySumaInfluencia;
                        posicionDeploy = pos;
                    }

                    copySumaInfluencia = valorInfluencia;
                    seguridad = 0;


                }

            }

            //Pawn solo si vamos mejor en casillas
            if (tropa == GameManager.Instance.enemyTroopPrefabs[3] && GameManager.Instance.enemyTroops.Count >= 2)
            {
                
                //Creamos un tablero de pruebas
                //BoardGrid copiaTablero = tablero.CopiaProfunda();
                // Crear una nueva instancia del prefab o del objeto original
                float[,] mapaCopy = DeepCopyMapaInf(tablero.influenceMap);
                Cell[,] tableroCopy = DeepCopyBoard(tablero.GetBoard());


                //Recorremos todas las pos validas
                foreach ((int, int) pos in posicionesParaComp)
                {


                    //Comprobar si puede atacar enemigos
                    for (int x = -3; x < 4; x++)
                    {
                        for (int y = -3; y < 4; y++)
                        {
                            if (x == 0 && y == 0) continue;



                            if (pos.Item1 + x * 3 >= 0 && pos.Item1 + x * 3 < tableroCopy.GetLength(0) && pos.Item2 + y * 3 >= 0 && pos.Item2 + y * 3 < tableroCopy.GetLength(1))
                            {

                                if (tableroCopy[pos.Item1 + x * 3, pos.Item2 + y * 3].transform.childCount != 0)
                                {
                                    if (tableroCopy[pos.Item1 + x * 3, pos.Item2 + y * 3].transform.GetChild(0).GetComponent<Troop>().team == Team.Blue)
                                    {

                                       
                                        copySumaInfluencia -= 200f;


                                    }

                                }

                                else
                                {

                                    copySumaInfluencia -= 1f;
                                }
                            }
                        }
                    }

                    if (copySumaInfluencia <= previuosInfluence)
                    {
                        previuosInfluence = copySumaInfluencia;
                        posicionDeploy = pos;
                    }

                    copySumaInfluencia = valorInfluencia;
                }
            }
            
            //Barrel
            //Tiene que usarse matando a la mayor cantidad de tropas posibles
            //Hay que desplegarlo donde se tenga más influencia enemiga
            //Solo lo usaremos si tenemos alguna tropa
            if (tropa is Bomb && GameManager.Instance.enemyTroops.Count >= 1)
            {
                Debug.Log("Comprobando Barril");
                float efectividad = 0;
                float preEfectividad = 0;
                int kills = 0;
                int preKills = 0;

                //Creamos un tablero de pruebas
                //BoardGrid copiaTablero = tablero.CopiaProfunda();
                // Crear una nueva instancia del prefab o del objeto original
                float[,] mapaCopy = DeepCopyMapaInf(tablero.influenceMap);
                Cell[,] tableroCopy = DeepCopyBoard(tablero.GetBoard());

                //Recorremos todas las pos validas
                foreach ((int, int) pos in posicionesParaComp)
                {
                    Debug.Log("Mirando posiciones barril");

                    //Comprobar si puede pintar o si tiene enemigos cerca
                    for (int x = -1; x < 2; x++)
                    {
                        for (int y = -1; y < 2; y++)
                        {
                            if (x == 0 && y == 0) continue;

                            //Vemos si matamos
                            if (pos.Item1 + x >= 0 && pos.Item1 + x < tableroCopy.GetLength(0) && pos.Item2 + y >= 0 && pos.Item2 + y < tableroCopy.GetLength(1))
                            {
                                if (tableroCopy[pos.Item1 + x, pos.Item2 + y].transform.childCount != 0)
                                {
                                    if (tableroCopy[pos.Item1 + x, pos.Item2 + y].transform.GetChild(0).GetComponent<Troop>().team == Team.Blue)
                                    {
                                        Debug.Log("Una KILL");
                                        preKills++;
                                    }
                                        

                                    //Factor de desempate: vemos cuanto altera el mapa de influencia
                                    preEfectividad += mapaCopy[pos.Item1 + x, pos.Item2 + y];
                                }
                                   
                            }
                               
                        }
                    }
                 
                    if(preKills > kills)
                    {
                        Debug.Log("Comprobando jugada con más kills");
                        kills = preKills;
                        copySumaInfluencia -= 30*kills + efectividad*2f;
                        posicionDeploy = pos;
                        previuosInfluence = copySumaInfluencia;
                        posicionDeploy = pos;

                    }
                    else if(preKills == kills)
                    {
                        if(preEfectividad > efectividad)
                        {
                            Debug.Log("Comprobando jugada con más kills");
                            efectividad = preEfectividad;
                            copySumaInfluencia -= 30 * kills + efectividad * 2f;
                            posicionDeploy = pos;
                            previuosInfluence = copySumaInfluencia;
                            posicionDeploy = pos;
                        }
                    }

                    preKills = 0;
                    copySumaInfluencia = valorInfluencia;
                }
            }

           

            return ((previuosInfluence, tropa),posicionDeploy);
        }
        public (Troop,(int,int)) ObtenerMejorJugada()
        {
           

            Troop tropaSeleccionada = null;

            List<(int, int)> posibleDeploypos = new List<(int, int)>();
            (int, int) deployPos = (0, 0);
            List<((float,Troop), (int, int))> List_score_pos = new List <((float, Troop), (int, int))>();
            float previusBestPlay = 0;
           

            for (int i = 0; i < GameManager.Instance.enemyTroopPrefabs.Count; i++)
            {
                //Si tenemos suficiente dinero para comprar la tropa simulamos la mejor jugada
                if(GameManager.Instance.GetCoins(Team.Red) >= GameManager.Instance.enemyTroopPrefabs[i].cost)
                {
                
                    //Funciona
                    posibleDeploypos = ObtenerPosicionesValidas(GameManager.Instance.enemyTroopPrefabs[i],GameManager.Instance.board);
                    
                    //Guardamos la jugada
                    List_score_pos.Add(ObtenerPosicionPorInfluencia(GameManager.Instance.enemyTroopPrefabs[i], posibleDeploypos, GameManager.Instance.board));
                    

                }
                
            }

            previusBestPlay = List_score_pos[0].Item1.Item1;
            tropaSeleccionada = List_score_pos[0].Item1.Item2;
            deployPos = List_score_pos[0].Item2;
            foreach (((float,Troop), (int, int)) jugada in List_score_pos)
            {
                //Debug.Log("Tropa en el array "+jugada.Item1.Item2);
                if(jugada.Item1.Item1 < previusBestPlay)
                {
                    Debug.Log("Tropa seleccionada " + jugada.Item1.Item2);
                    tropaSeleccionada = jugada.Item1.Item2;
                    deployPos = jugada.Item2;
                }
            }

            return (tropaSeleccionada, deployPos);
        }
       
        public override void Action()
        {
            Debug.Log("Desplegar");

            (Troop, (int, int)) jugada = ObtenerMejorJugada();

            GameManager.Instance.board.SpawnTroop(jugada.Item1,GameManager.Instance.board.getCell(jugada.Item2.Item1,jugada.Item2.Item2));
            GameManager.Instance.SpendCoins(jugada.Item1.cost, Team.Red);
        }
    }

    class IAMove : IANode
    {

        Node[] path;
        int steps;
        Troop actualTroop;
        bool change;

        public override void Action()
        {
            Debug.Log("Mover");
            List<Troop> playerTroops = GameManager.Instance.playerTroops;
            List<Troop> enemyTroops = GameManager.Instance.enemyTroops;

            path = null;
            steps = 1000;

            actualTroop = null;
            change = false;
            
            if (path == null && playerTroops.Count > 0 && enemyTroops.Count > 0)
            {
                foreach (Troop ETroop in enemyTroops)
                {
                    foreach (Troop PTroop in playerTroops)
                    {
                        Cell ECell = ETroop.transform.parent.GetComponent<Cell>();
                        Cell PCell = PTroop.transform.parent.GetComponent<Cell>();

                        (int, int) EPos = ECell.GetGridPosition();
                        (int, int) PPos = PCell.GetGridPosition();

                        if (ETroop.moveRange > 1)
                            PathRequestManager.RequestPath(EPos, PPos, OnPathFound, true);
                        else
                            PathRequestManager.RequestPath(EPos, PPos, OnPathFound, false);

                        if (change)
                            actualTroop = ETroop;
                    }
                }
                Cell destination = GameManager.Instance.board.getCell(path[0].gridY, path[0].gridX);
                actualTroop.MoveToCell(destination);

            }

        }
        public void OnPathFound(Node[] newPath, bool pathSuccessful)
        {
            if (pathSuccessful)
            {
                if (path == null)
                {
                    path = newPath;
                    steps = path.Length;

                }
                else if (steps > newPath.Length)
                {
                    path = newPath;
                    steps = path.Length;
                }   
            }
        }

    }

    class IASkipTurn : IANode
    {
        public override void Action()
        {
            Debug.Log("Pasar turno");
        }
    }

   
}


