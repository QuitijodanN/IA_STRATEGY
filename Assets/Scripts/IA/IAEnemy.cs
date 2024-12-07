
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;

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
                Debug.Log(t.transform.parent.name);
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
                            posibleDeploypos.Add((i, j));
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
                        if (tablero.getCell(i, j).GetColorTeam() == Team.Red)
                            posibleDeploypos.Add((i, j));
                    }
                }
            }
            return posibleDeploypos;
        }
        
        private (int,int) ObtenerPosicionPorInfluencia(Troop tropa, List<(int,int)> posicionesParaComp, BoardGrid tablero)
        {
            (int, int) posicionDeploy = (0,0);
            float valorInfluencia = tablero.sumaMapaInfluencia;

            //Caballero
            /*
             Ataque Mele(1 pos)
             Se mueve dos pos
             
             */
            if (tropa == GameManager.Instance.enemyTroopPrefabs[0])
            {
                //Creamos un tablero de pruebas
                BoardGrid copiaTablero = tablero.CopiaProfunda();
                //Recorremos todas las pos validas
                foreach ((int,int) pos in posicionesParaComp)
                {
                    copiaTablero.SpawnTroop(GameManager.Instance.enemyTroopPrefabs[0], copiaTablero.getCell(pos.Item1, pos.Item2));
                   
                    //Comprobar si esta a mele
                    for(int x = -1;x < 2; x++)
                    {
                        for(int y = -1;y < 2; y++)
                        {
                            if (x ==  0 && y == 0  ||
                                x ==  1 && y == 1  ||
                                x == -1 && y == -1 ||
                                x == -1 && y == 1  ||
                                x ==  1 && y == -1) continue;

                            if (copiaTablero.getCell(pos.Item1+x,pos.Item2+y).transform.childCount != 0)
                                if (copiaTablero.getCell(x, y).transform.GetChild(0).GetComponent<Troop>().team == Team.Blue)
                                    copiaTablero.sumaMapaInfluencia -= 5;
                                
                        }
                    }

                    if(copiaTablero.sumaMapaInfluencia < valorInfluencia)
                    {
                        valorInfluencia = copiaTablero.sumaMapaInfluencia;
                        posicionDeploy = pos;
                    }

                    Destroy(copiaTablero.getCell(pos.Item1, pos.Item2).transform.GetChild(0).gameObject);
                    copiaTablero.ActualizeInfluence();
                }

            }

            //Arquero
            if (tropa == GameManager.Instance.enemyTroopPrefabs[1])
            {
                //Creamos un tablero de pruebas
                BoardGrid copiaTablero = tablero.CopiaProfunda();
                //Recorremos todas las pos validas
                foreach ((int, int) pos in posicionesParaComp)
                {
                    copiaTablero.SpawnTroop(GameManager.Instance.enemyTroopPrefabs[0], copiaTablero.getCell(pos.Item1, pos.Item2));

                    //Comprobar si puede atacar enemigos
                    for (int x = -2; x < 3; x++)
                    {
                        for (int y = -2; y < 3; y++)
                        {
                            if (x ==  0 && y == 0) continue;

                          
                            if (copiaTablero.getCell(pos.Item1 + x, pos.Item2 + y).transform.childCount != 0)
                                if (copiaTablero.getCell(x, y).transform.GetChild(0).GetComponent<Troop>().team == Team.Blue)
                                    copiaTablero.sumaMapaInfluencia -= 10;

                           
                        }
                    }

                    if (copiaTablero.sumaMapaInfluencia < valorInfluencia)
                    {
                        valorInfluencia = copiaTablero.sumaMapaInfluencia;
                        posicionDeploy = pos;
                    }

                    Destroy(copiaTablero.getCell(pos.Item1, pos.Item2).transform.GetChild(0).gameObject);
                    copiaTablero.ActualizeInfluence();
                }

            }
            //Torre

            //Pawn solo si vamos mejor en casillas
            if (tropa == GameManager.Instance.enemyTroopPrefabs[3] && tablero.GetColorCellAmount(Team.Red) >= tablero.GetColorCellAmount(Team.Blue))
            {
                //Creamos un tablero de pruebas
                BoardGrid copiaTablero = tablero.CopiaProfunda();
                //Recorremos todas las pos validas
                foreach ((int, int) pos in posicionesParaComp)
                {
                    copiaTablero.SpawnTroop(GameManager.Instance.enemyTroopPrefabs[0], copiaTablero.getCell(pos.Item1, pos.Item2));

                    //Comprobar si puede atacar enemigos
                    for (int x = -3; x < 4; x++)
                    {
                        for (int y = -3; y < 4; y++)
                        {
                            if (x == 0 && y == 0) continue;


                            if (copiaTablero.getCell(pos.Item1 + x, pos.Item2 + y).transform.childCount != 0)
                                if (copiaTablero.getCell(x, y).transform.GetChild(0).GetComponent<Troop>().team == Team.Blue)
                                    copiaTablero.sumaMapaInfluencia -= 15;


                        }
                    }
                    
                    if (copiaTablero.sumaMapaInfluencia < valorInfluencia)
                    {
                        valorInfluencia = copiaTablero.sumaMapaInfluencia;
                        posicionDeploy = pos;
                    }

                    Destroy(copiaTablero.getCell(pos.Item1, pos.Item2).transform.GetChild(0).gameObject);
                    copiaTablero.ActualizeInfluence();
                }

            }

            //Barrel


            return posicionDeploy;
        }
        public (Troop,(int,int)) ObtenerMejorJugada(float[,] mapaInfluencia)
        {
            float[,] mapaInf = GameManager.Instance.board.influenceMap;
            int rows = GameManager.Instance.board.rows;
            int cols = GameManager.Instance.board.columns;

            Troop tropaSeleccionada = GameManager.Instance.enemyTroopPrefabs[0];

            List<(int, int)> posibleDeploypos = new List<(int, int)>();
            (int, int) deployPos = (0, 0);
            int filaDeploy = 0;
            float valueDeploy = 0;
            float greaterLastRow = 0;
            float sumaCurrentRow = 0;

            for (int i = 0; i < GameManager.Instance.enemyTroopPrefabs.Count; i++)
            {
                //Si tenemos suficiente dinero para comprar la tropa simulamos la mejor jugada
                if(GameManager.Instance.GetCoins(Team.Red) >= GameManager.Instance.enemyTroopPrefabs[i].cost)
                {
                
                    posibleDeploypos = ObtenerPosicionesValidas(GameManager.Instance.enemyTroopPrefabs[i],GameManager.Instance.board);
                    deployPos = ObtenerPosicionPorInfluencia(GameManager.Instance.enemyTroopPrefabs[i], posibleDeploypos, GameManager.Instance.board);
                    //Guardar jugada y comparar con la anterior mejor

                }
                
            }
            return (tropaSeleccionada, deployPos);
        }
       
        public override void Action()
        {
            Debug.Log("Desplegar");


            //Cambiar a bucle que compre las unidades
          /*  if (GameManager.Instance.GetCoins(Team.Red) >= GameManager.Instance.enemyTroopPrefabs[3].cost)
            {
                GameManager.Instance.board.SpawnTroop(GameManager.Instance.enemyTroopPrefabs[3], GameManager.Instance.board.getCell(deployPos.Item1, deployPos.Item2));
                GameManager.Instance.SpendCoins(5, Team.Red);
            }
            else if(GameManager.Instance.enemyCoins >= 3)
            {
                GameManager.Instance.board.SpawnTroop(GameManager.Instance.enemyTroopPrefabs[1], GameManager.Instance.board.getCell(deployPos.Item1, deployPos.Item2));
                GameManager.Instance.SpendCoins(3, Team.Red);
            }
            else
            {
                GameManager.Instance.board.SpawnTroop(GameManager.Instance.enemyTroopPrefabs[0], GameManager.Instance.board.getCell(deployPos.Item1, deployPos.Item2));
                GameManager.Instance.SpendCoins(2, Team.Red);
            }*/

            GameManager.Instance.UseAction();
           
           
        }
    }

    class IAMove : IANode
    {
        public override void Action()
        {
            Debug.Log("Mover");
            //GameManager.Instance.UseAction();
        }
    }

    class IASkipTurn : IANode
    {
        public override void Action()
        {
            Debug.Log("Pasar turno");
            GameManager.Instance.yourTurn = true;
        }
    }

    /*
    private IEnumerator UpdateIA()
    {
        yield return new WaitForEndOfFrame();
        n_root.Action();
    }


    interface IANode
    {
        void Init();
        NodeActionResult Action();
    }

    class IASequenceNode : IANode
    {
        public IASequenceNode(IList<IANode> i_nodes)
        {
            n_subNodes = i_nodes;
        }

        public void Init()
        {
            foreach (IANode n in n_subNodes)
            {
                n.Init();
            }
        }

        public NodeActionResult Action()
        {
            foreach (IANode n in n_subNodes)
            {
                NodeActionResult subsubNodeResult =  n.Action();

                if(subsubNodeResult == NodeActionResult.Running)
                    return NodeActionResult.Running;
                else if(subsubNodeResult == NodeActionResult.Failure)
                    return NodeActionResult.Failure;

            }
            return NodeActionResult.Success;
        }

        IList<IANode> n_subNodes;
    }

    /*
    class IASelectEnemyTroopNode : IANode
    {
        IAInfo aiInfo;
        Troop selectedEnemyTroop;

        public IASelectEnemyTroopNode(IAInfo IAinfo)
        {
            aiInfo = IAinfo;
            selectedEnemyTroop = aiInfo.selectedEnemyTroop;
        }

        public void Init()
        {
           
        }

        public NodeActionResult Action()
        {
           if (selectedEnemyTroop == null)
           {
                Debug.Log("No hay tropa aliada seleccionada como target");
                selectedEnemyTroop = aiInfo.allyTeam[0];

            }
            else
            {
                foreach (Troop enemy in aiInfo.allyTeam)
                {
                    if (enemy != selectedEnemyTroop) selectedEnemyTroop = enemy;
                }
            }


           if(selectedEnemyTroop == null) return NodeActionResult.Failure;

           Debug.Log("Tropa ALIADA seleccionada" + selectedEnemyTroop);
           return NodeActionResult.Success;
        }
    }
    class IASelectTroopNode : IANode
    {
       
        IAInfo iaInfo;
        Troop selectedTroop;
        public IASelectTroopNode(IAInfo iAInfo)
        {
            iaInfo = iAInfo;
            selectedTroop = iaInfo.selectedTroop;

        }
        public void Init()
        {
           
        }

        public NodeActionResult Action()
        {
       
            int most_powerfull = 0;
            

            //Si no hemos seleccionado otra unidad seleccionamos la Unidad más fuerte que tenemos -> success
            if (selectedTroop == null)
            {
                Debug.Log("No hay tropa previa seleccionada");
                if(iaInfo.enemyTeam.Count != 0)
                {
                    foreach (Troop enemy in iaInfo.enemyTeam)
                    {

                        if (enemy.TroopPower > most_powerfull)
                        {
                            most_powerfull = enemy.TroopPower;
                            selectedTroop = enemy;

                        }
                    }
                }


            }
            else
            {
                Debug.Log("Nombre de la tropa previamente seleccionada" + iaInfo.selectedTroop.name);
                //Si ya tenemos una unidad seleccionada vemos cual es la siguiente más fuerte -> success
                foreach (Troop enemy in iaInfo.enemyTeam)
                {
                    if (enemy.TroopPower > most_powerfull && !enemy.Equals(selectedTroop))
                    {
                        most_powerfull = enemy.TroopPower;
                        selectedTroop = enemy;
                    }
                }
            }

            //Actualizamos la selected troop
            iaInfo.selectedTroop = selectedTroop;
            //Si no tenemos tropas terminamos -> failure
            if (selectedTroop == null)
            {

                Debug.Log(selectedTroop);
                GameManager.Instance.UseAction();
                return NodeActionResult.Failure;

            }
            else
            {
                
                Debug.Log("Nueva tropa seleccionada"+selectedTroop);
                GameManager.Instance.UseAction();
                return NodeActionResult.Success;
            }
        }
            
    }
    */
}


