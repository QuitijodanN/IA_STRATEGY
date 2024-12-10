
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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

            if (thinkTimer >= 1.5f) {
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
        IAAttack attack = new IAAttack();
        IASkipTurn skip = new IASkipTurn();
        IAMove move = new IAMove();
        IAPaint paint = new IAPaint();


        // atamos nodos a los padres (sequence nodes)
        IAMoveBehavior moveBehavior = new IAMoveBehavior(paint, move);
        IACanAttack canAttack = new IACanAttack(attack, moveBehavior);
        IAUseTroops useTroops = new IAUseTroops(canAttack, skip);
        IASaveMoney saveMoney = new IASaveMoney(deploy, useTroops);
        IAEnoughGold enoughGold = new IAEnoughGold(saveMoney, useTroops);

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
            // Si tenemos oro para desplegar tropas
            if (GameManager.Instance.GetCoins(Team.Red) > 2) {
                n_true.Action();
            }
            else {
                n_false.Action();
            }
        }
    }

    class IASaveMoney : IASequenceNode
    {
        public IASaveMoney(IANode nodeTrue, IANode nodeFalse) : base(nodeTrue, nodeFalse) { }

        public override void Action()
        {
            // Si tenemos mas tropas pero menos casillas pintadas, priorizamos movernos para pintar

            if (GameManager.Instance.GetTurn() > 18 ||
                GameManager.Instance.GetCoins(Team.Red) + 3 + GameManager.Instance.enemyTroops.Count > 20 ||
                GameManager.Instance.enemyTroops.Count <= 1) {

                if (GameManager.Instance.enemyTroops.Count > GameManager.Instance.playerTroops.Count &&
                GameManager.Instance.board.GetColorCellAmount(Team.Red) < GameManager.Instance.board.GetColorCellAmount(Team.Blue)) {
                    n_false.Action();
                } else {
                    n_true.Action();
                }
            }
            else
            {
                n_false.Action();
            }
        }
    }

    class IAUseTroops : IASequenceNode
    {
        public IAUseTroops(IANode nodeTrue, IANode nodeFalse) : base(nodeTrue, nodeFalse) { }

        public override void Action()
        {
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
            BoardGrid board = GameManager.Instance.board; // Referencia al tablero
            Cell[,] grid = board.GetBoard();
            List<(Troop attacker, Troop target)> attackPairs = new List<(Troop, Troop)>(); // Lista de ataques posibles
            /*
             Si puede atacar nodo hoja hit
             else Nodo elección pintar
             */
            foreach (Troop t in GameManager.Instance.enemyTroops)
            {

                Cell currentCell = t.GetComponentInParent<Cell>();
                (int, int) pos = currentCell.GetGridPosition(); // Usamos GetGridPosition del Cell

                for (int x = -t.attackRange; x <= t.attackRange; x++)
                {
                    for (int y = -t.attackRange; y <= t.attackRange; y++)
                    {
                        if (x == 0 && y == 0) continue; // Ignoramos la celda de origen

                        int targetX = pos.Item1 + x;
                        int targetY = pos.Item2 + y;

                        if (IsValidPosition(targetX, targetY, grid)) // Validamos coordenadas con la matriz
                        {
                            Cell targetCell = grid[targetX, targetY];
                            if (targetCell.transform.childCount != 0)
                            {
                                Troop target = targetCell.transform.GetChild(0).GetComponent<Troop>();
                                if (target.team == Team.Blue) // Encontramos una tropa enemiga
                                {
                                    attackPairs.Add((t, target));
                                }
                            }
                        }
                    }
                }
            }
            if (attackPairs.Count > 0)
            {
                IAAttack attackNode = n_true as IAAttack;
                attackNode.SetValues(attackPairs);
                n_true.Action();
            }                
            else 
                n_false.Action();

        }
        private bool IsValidPosition(int x, int y, Cell[,] grid)
        {
            // Comprueba si las coordenadas están dentro de los límites del tablero
            return x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1);
        }
    }

    class IAMoveBehavior : IASequenceNode
    {
        public IAMoveBehavior(IANode nodeTrue, IANode nodeFalse) : base(nodeTrue, nodeFalse) { }

        public override void Action()
        {
            if (GameManager.Instance.playerTroops.Count == 0 || GameManager.Instance.enemyTroops.Count > GameManager.Instance.playerTroops.Count) {
                Debug.Log("Paint");
                n_true.Action();
            }
            else {
                Debug.Log("Move Path");
                n_false.Action();
            }
                
        }
    }

    // -----------------------------------------------------------------------------------------------------------------------------------------
    // -- NODOS HOJA
    // -----------------------------------------------------------------------------------------------------------------------------------------
    class IAAttack : IANode
    {
        List<(Troop, Troop)> attackPairs; // Lista de ataques posibles        

        public void SetValues(List<(Troop, Troop)> _attackPairs)
        {
            attackPairs = _attackPairs;

        }
        public override void Action()
        {
            GameManager.Instance.enemyTroops = GameManager.Instance.GetTroops(Team.Red); // Todas las tropas enemigas     

            // Lógica para seleccionar el mejor ataque
            (Troop, Troop) atacantes = SelectBestAttack(attackPairs);
            Troop attacker = atacantes.Item1;
            Troop target = atacantes.Item2;
            Debug.Log($"{attacker.name} ataca a {target.name}");

            // Realizamos el ataque
            if (attacker is Tower)
            {
                GameManager.Instance.board.AttackWithTroop(attacker, target);
            }
            else
            {
                attacker.Attack(target);
            }

            GameManager.Instance.UseAction();//ESTO CUANDO SE META EN EL ARBOL BIEN HAY QUE QUITARLO DE AQUI
            Debug.Log("Ataque realizado");
        }

        public (Troop attacker, Troop target) SelectBestAttack(List<(Troop attacker, Troop target)> attackPairs)
        {
            (Troop bestAttacker, Troop bestTarget) bestAttack = (null, null);
            float bestDamage = float.MinValue;

            foreach (var attackPair in attackPairs) {
                Troop attacker = attackPair.attacker;
                Troop target = attackPair.target;

                // Check if the attack will kill the target
                if (target.health - target.damage <= 0) {
                    // If this attack kills the target, choose it
                    return attackPair;
                }

                // Otherwise, check if this attack deals more damage than the previous best
                if (target.damage > bestDamage) {
                    bestDamage = target.damage;
                    bestAttack.bestAttacker = attacker;
                    bestAttack.bestTarget = target;
                }
            }

            return bestAttack;
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
                        if (tablero.GetCell(i, j).GetColorTeam() == Team.Red || tablero.GetCell(i, j).GetColorTeam() == Team.None)
                        {
                            if(tablero.GetCell(i, j).transform.childCount == 0)
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
                        if (tablero.GetCell(i, j).GetColorTeam() == Team.Red && tablero.GetCell(i, j).transform.childCount == 0)
                            posibleDeploypos.Add((i, j));
                    }
                }
            }
            return posibleDeploypos;
        }

        private ((float,Troop),(int,int)) ObtenerPosicionPorInfluencia(Troop tropa, List<(int,int)> posicionesParaComp, BoardGrid tablero)
        {
            float influence = 0f;
            float bestInfluence = -1f;
            (int, int) bestPos = (-1, -1);

            //Caballero
            if (tropa == GameManager.Instance.enemyTroopPrefabs[0])
            {
                //Recorremos todas las pos validas
                foreach ((int,int) pos in posicionesParaComp)
                {
                    //Comprobar si esta a mele
                    for (int x = -1;x < 2; x++)
                    {
                        for(int y = -1;y < 2; y++)
                        {
                            if (pos.Item1 + x >= 0 && pos.Item1 + x < tablero.rows && pos.Item2 + y >= 0 && pos.Item2 + y < tablero.columns) {
                                if (x == 0 && y == 0) continue;

                                float t = tablero.GetCellInfluence(pos.Item1 + x, pos.Item2 + y);
                                if (t >= 0) influence += t;
                            }
                        }
                    }

                    influence /= 8f;

                    if(influence > bestInfluence)
                    {
                        bestInfluence = influence;
                        bestPos = pos;
                    }
                }

                Debug.Log("Caballero: " + bestInfluence);

            }

            //Arquero
            if (tropa == GameManager.Instance.enemyTroopPrefabs[1])
            {
                //Recorremos todas las pos validas
                foreach ((int, int) pos in posicionesParaComp)
                {
                    int closeEnemies = 0;
                    //Comprobar si puede atacar enemigos
                    for (int x = -2; x < 3; x++)
                    {
                        for (int y = -2; y < 3; y++)
                        {
                            if (pos.Item1 + x >= 0 && pos.Item1 + x < tablero.rows && pos.Item2 + y >= 0 && pos.Item2 + y < tablero.columns) {
                                if (x <= 1 && x >= -1 && y <= 1 && y >= -1) {
                                    if (tablero.GetCell(pos.Item1 + x, pos.Item2 + y).transform.childCount > 0) {
                                        closeEnemies++;
                                    }
                                    continue;
                                }

                                float t = tablero.GetCellInfluence(pos.Item1 + x, pos.Item2 + y);
                                if (t >= 0) influence += t;
                            }
                        }

                        influence = (influence - closeEnemies) / 12f;

                        if (influence > bestInfluence) {
                            bestInfluence = influence;
                            bestPos = pos;
                        }
                    }
                }
                Debug.Log("Arquero: " + bestInfluence);
            }
            //Torre
            //Solo si tenemos 2 o mas tropas
            //Se tiene que colocar en zonas seguras
            if (tropa is Tower && GameManager.Instance.enemyTroops.Count >= 2)
            {
                //Recorremos todas las pos validas
                foreach ((int, int) pos in posicionesParaComp)
                {
                    //Comprobar si puede pintar o si tiene enemigos cerca
                    for (int x = -2; x < 3; x++)
                    {
                        for (int y = -2; y < 3; y++)
                        {
                            if (pos.Item1 + x >= 0 && pos.Item1 + x < tablero.rows && pos.Item2 + y >= 0 && pos.Item2 + y < tablero.columns) {
                                if (x == 0 && y == 0) continue;

                                float t = tablero.GetCellInfluence(pos.Item1 + x, pos.Item2 + y);
                                influence += t;
                            }
                        }
                    }
                    
                    influence /= -16f;

                    if (influence > bestInfluence) {
                        bestInfluence = influence;
                        bestPos = pos;
                    }
                }
                Debug.Log("Torre: " + bestInfluence);
            }

            //Pawn solo si vamos mejor en casillas
            if (tropa == GameManager.Instance.enemyTroopPrefabs[3] && GameManager.Instance.enemyTroops.Count >= 2)
            {

                //Recorremos todas las pos validas
                foreach ((int, int) pos in posicionesParaComp) {
                    int closeEnemies = 0;
                    //Comprobar si puede atacar enemigos
                    for (int x = -3; x < 4; x++) {
                        for (int y = -3; y < 4; y++) {
                            if (pos.Item1 + x >= 0 && pos.Item1 + x < tablero.rows && pos.Item2 + y >= 0 && pos.Item2 + y < tablero.columns) {
                                if (x <= 1 && x >= -1 && y <= 1 && y >= -1) {
                                    if (tablero.GetCell(pos.Item1 + x, pos.Item2 + y).transform.childCount > 0) {
                                        closeEnemies++;
                                    }
                                    continue;
                                }

                                float t = tablero.GetCellInfluence(pos.Item1 + x, pos.Item2 + y);
                                if (t >= 0) influence += t;
                            }
                        }

                        influence = (influence - closeEnemies) / 16f;

                        if (influence > bestInfluence) {
                            bestInfluence = influence;
                            bestPos = pos;
                        }
                    }
                }
                Debug.Log("Mago: " + bestInfluence);
            }
            
            //Barrel
            //Tiene que usarse matando a la mayor cantidad de tropas posibles
            //Hay que desplegarlo donde se tenga más influencia enemiga
            //Solo lo usaremos si tenemos alguna tropa
            if (tropa is Bomb && GameManager.Instance.enemyTroops.Count >= 1)
            {

                //Recorremos todas las pos validas
                foreach ((int, int) pos in posicionesParaComp)
                {
                    //Comprobar si esta a mele
                    for (int x = -1; x < 2; x++) {
                        for (int y = -1; y < 2; y++) {
                            if (pos.Item1 + x >= 0 && pos.Item1 + x < tablero.rows && pos.Item2 + y >= 0 && pos.Item2 + y < tablero.columns) {
                                if (x == 0 && y == 0) continue;

                                if (tablero.GetCell(pos.Item1 + x, pos.Item2 + y).transform.childCount > 0) 
                                    influence += 2.5f;
                                if (tablero.GetCell(pos.Item1 + x, pos.Item2 + y).GetColorTeam() == Team.Blue)
                                    influence += 1f;
                                else if (tablero.GetCell(pos.Item1 + x, pos.Item2 + y).GetColorTeam() == Team.None)
                                    influence += 0.5f;
                            }
                        }
                    }

                    influence /= 8f;

                    if (influence > bestInfluence) {
                        bestInfluence = influence;
                        bestPos = pos;
                    }
                }
                Debug.Log("Bomba: " + bestInfluence);
            }
            return ((bestInfluence, tropa), bestPos);
        }

        public (Troop,(int,int)) ObtenerMejorJugada()
        {
            Troop tropaSeleccionada = null;

            List<(int, int)> posibleDeploypos = new List<(int, int)>();
            (int, int) deployPos = (0, 0);
            List<((float,Troop), (int, int))> List_score_pos = new List <((float, Troop), (int, int))>();
            float previousBestPlay = 0;
           

            for (int i = 0; i < GameManager.Instance.enemyTroopPrefabs.Count; i++)
            {
                //Si tenemos suficiente dinero para comprar la tropa simulamos la mejor jugada
                if(GameManager.Instance.GetCoins(Team.Red) >= GameManager.Instance.enemyTroopPrefabs[i].cost)
                {
                    //Funciona
                    posibleDeploypos = ObtenerPosicionesValidas(GameManager.Instance.enemyTroopPrefabs[i],GameManager.Instance.board);

                    ((float, Troop), (int, int)) bestDeploypos = ObtenerPosicionPorInfluencia(GameManager.Instance.enemyTroopPrefabs[i], posibleDeploypos, GameManager.Instance.board);

                    //Guardamos la jugada
                    List_score_pos.Add(bestDeploypos);
                }
                
            }

            previousBestPlay = List_score_pos[0].Item1.Item1;
            tropaSeleccionada = List_score_pos[0].Item1.Item2;
            deployPos = List_score_pos[0].Item2;
            foreach (((float,Troop), (int, int)) jugada in List_score_pos)
            {
                //Debug.Log("Tropa en el array "+jugada.Item1.Item2);
                if(jugada.Item1.Item1 > previousBestPlay)
                {
                    Debug.Log("Tropa seleccionada " + jugada.Item1.Item2);
                    previousBestPlay = jugada.Item1.Item1;
                    tropaSeleccionada = jugada.Item1.Item2;
                    deployPos = jugada.Item2;
                }
            }

            // Si previousBestPlay sigue siendo 0, selecciona una tropa y posición aleatorias
            if (previousBestPlay == 0) {
                // Filtrar tropas disponibles según el coste
                List<Troop> tropasDisponibles = GameManager.Instance.enemyTroopPrefabs
                    .Where(t => GameManager.Instance.GetCoins(Team.Red) >= t.cost)
                    .ToList();

                if (tropasDisponibles.Count > 0) {
                    // Seleccionar tropa aleatoria
                    tropaSeleccionada = tropasDisponibles[UnityEngine.Random.Range(0, tropasDisponibles.Count)];

                    // Obtener posiciones válidas para esa tropa
                    posibleDeploypos = ObtenerPosicionesValidas(tropaSeleccionada, GameManager.Instance.board);

                    // Seleccionar posición aleatoria
                    if (posibleDeploypos.Count > 0) {
                        deployPos = posibleDeploypos[UnityEngine.Random.Range(0, posibleDeploypos.Count)];
                    }
                }
            }

            return (tropaSeleccionada, deployPos);
        }
       
        public override void Action()
        {
            Debug.Log("Desplegar");

            (Troop, (int, int)) jugada = ObtenerMejorJugada();

            if (jugada.Item2.Item1 > -1) {
                GameManager.Instance.board.SpawnTroop(jugada.Item1, GameManager.Instance.board.GetCell(jugada.Item2.Item1, jugada.Item2.Item2));
                GameManager.Instance.SpendCoins(jugada.Item1.cost, Team.Red);
            }
        }
    }

    //class IAMove : IANode
    //{
    //    List<Troop> playerTroops;
    //    List<Troop> enemyTroops;

    //    Node[] path;
    //    int steps;

    //    Troop actualTroop;

    //    int index;

    //    public override void Action()
    //    {
    //        Debug.Log("Mover");
    //        playerTroops = GameManager.Instance.playerTroops;
    //        enemyTroops = GameManager.Instance.enemyTroops;

    //        path = null;
    //        steps = 1000;
    //        index = 0;

    //        actualTroop = null;

    //        if (path == null && playerTroops.Count > 0 && enemyTroops.Count > 0)
    //        {
    //            foreach (Troop ETroop in enemyTroops)
    //            {
    //                foreach (Troop PTroop in playerTroops)
    //                {
    //                    Cell ECell = ETroop.transform.parent.GetComponent<Cell>();
    //                    Cell PCell = PTroop.transform.parent.GetComponent<Cell>();

    //                    (int, int) EPos = ECell.GetGridPosition();
    //                    (int, int) PPos = PCell.GetGridPosition();

    //                    // Calcula el vector de dirección desde PPos hacia EPos
    //                    int dirX = EPos.Item1 - PPos.Item1;
    //                    int dirY = EPos.Item2 - PPos.Item2;

    //                    // Normaliza el vector (reduce a una dirección de paso único)
    //                    int stepX = dirX != 0 ? dirX / Math.Abs(dirX) : 0;
    //                    int stepY = dirY != 0 ? dirY / Math.Abs(dirY) : 0;

    //                    // Calcula la posición ajustada (por ejemplo, a 1 celda de distancia)
    //                    int adjustedX = PPos.Item1 + stepX * ETroop.attackRange; // Máximo 1 paso
    //                    int adjustedY = PPos.Item2 + stepY * ETroop.attackRange;

    //                    (int, int) adjustedPos = (adjustedX, adjustedY);

    //                    if (ETroop.moveRange > 1)
    //                        PathRequestManager.RequestPath(EPos, adjustedPos, OnPathFound, true);
    //                    else if (ETroop.moveRange == 1)
    //                        PathRequestManager.RequestPath(EPos, adjustedPos, OnPathFound, false);
    //                    else
    //                        AllTowers(enemyTroops);
    //                }
    //            }

    //        }
    //        else if (path == null)
    //        {
    //            int index = 0;
    //            actualTroop = enemyTroops[index];
    //            while (index < enemyTroops.Count && actualTroop.moveRange < 1)
    //            {
    //                index ++;
    //                actualTroop = enemyTroops[index];
    //            }
    //            if (index <= enemyTroops.Count && actualTroop == null)
    //                AllTowers(enemyTroops);
    //            else
    //            {
    //                Cell ECell = actualTroop.transform.parent.GetComponent<Cell>();
    //                (int, int) EPos = ECell.GetGridPosition();

    //                float maxInfluence = 0;
    //                (int, int) actualPos = (0, 0);

    //                for (int i = 0; i < GameManager.Instance.board.rows; i++)
    //                {
    //                    for (int j = 0; j < GameManager.Instance.board.columns; j++)
    //                    {
    //                        if (GameManager.Instance.board.GetCellInfluence(i, j) >= maxInfluence)
    //                            if (IsCloser(EPos, actualPos, (i, j)))
    //                            {
    //                                actualPos = (i, j);
    //                                maxInfluence = GameManager.Instance.board.GetCellInfluence(i, j);
    //                            }
    //                    }
    //                }

    //                if (actualTroop.moveRange > 1)
    //                    PathRequestManager.RequestPath(EPos, actualPos, OnDiferentPath, true);
    //                else
    //                    PathRequestManager.RequestPath(EPos, actualPos, OnDiferentPath, false);
    //            }               

    //        }

    //    }

    //    public static bool IsCloser((int x, int y) point1, (int x, int y) point2, (int x, int y) point3)
    //    {
    //        // Calcular la distancia al cuadrado (más eficiente que usar raíz cuadrada)
    //        float distance1 = MathF.Pow(point3.x - point1.x, 2) + MathF.Pow(point3.y - point1.y, 2);
    //        float distance2 = MathF.Pow(point3.x - point2.x, 2) + MathF.Pow(point3.y - point2.y, 2);

    //        // Comparar distancias
    //        return distance1 < distance2;
    //    }

    //    public void AllTowers(List<Troop> enemyTroops)
    //    {
    //        bool towers = true;
    //        foreach (Troop ETroop in enemyTroops)
    //            if (ETroop.moveRange != 0)
    //                towers = false;

    //        if (towers)
    //            GameManager.Instance.UseAction();
    //    }

    //    public void OnPathFound(Node[] newPath, bool pathSuccessful)
    //    {
    //        int actual = index;
    //        index++;
    //        if (pathSuccessful)
    //        {
    //            if (path == null)
    //            {
    //                path = newPath;
    //                steps = path.Length;
    //                actualTroop = enemyTroops[actual / playerTroops.Count];

    //            }
    //            else if (steps > newPath.Length)
    //            {
    //                path = newPath;
    //                steps = path.Length;
    //                actualTroop = enemyTroops[actual / playerTroops.Count];
    //            }
    //        }
    //        if (path.Length > 0 && index >= playerTroops.Count * enemyTroops.Count)
    //        {
    //            Cell destination = GameManager.Instance.board.GetCell(path[0].gridY, path[0].gridX);

    //            GameManager.Instance.board.MoveTroop(actualTroop, destination);
    //        }

    //    }

    //    public void OnDiferentPath(Node[] newPath, bool pathSuccessful)
    //    {
    //        if (newPath.Length > 0)
    //        {
    //            Cell destination = GameManager.Instance.board.GetCell(newPath[0].gridY, newPath[0].gridX);

    //            GameManager.Instance.board.MoveTroop(actualTroop, destination);
    //        }
    //    }

    //}

    class IAMove : IANode
    {
        List<Troop> playerTroops;
        List<Troop> enemyTroops;

        Node[] path;
        int steps;

        Troop actualTroop;

        int index;

        public override void Action()
        {
            Debug.Log("Mover");
            playerTroops = GameManager.Instance.playerTroops;
            enemyTroops = GameManager.Instance.enemyTroops;

            path = null;
            steps = 1000;
            index = 0;

            actualTroop = null;

            if (path == null && playerTroops.Count > 0 && enemyTroops.Count > 0) {
                foreach (Troop ETroop in enemyTroops) {
                    foreach (Troop PTroop in playerTroops) {
                        if (ETroop == null || PTroop == null) continue;

                        Cell ECell = ETroop.transform.parent.GetComponent<Cell>();
                        Cell PCell = PTroop.transform.parent.GetComponent<Cell>();

                        if (ECell == null || PCell == null) continue;

                        (int, int) EPos = ECell.GetGridPosition();
                        (int, int) PPos = PCell.GetGridPosition();

                        // Calcula el vector de dirección desde PPos hacia EPos
                        int dirX = EPos.Item1 - PPos.Item1;
                        int dirY = EPos.Item2 - PPos.Item2;

                        // Normaliza el vector (reduce a una dirección de paso único)
                        int stepX = dirX != 0 ? dirX / Math.Abs(dirX) : 0;
                        int stepY = dirY != 0 ? dirY / Math.Abs(dirY) : 0;

                        // Calcula la posición ajustada (por ejemplo, a 1 celda de distancia)
                        int adjustedX = PPos.Item1 + stepX * ETroop.attackRange; // Máximo 1 paso
                        int adjustedY = PPos.Item2 + stepY * ETroop.attackRange;

                        (int, int) adjustedPos = (adjustedX, adjustedY);

                        if (ETroop.moveRange < 1)
                            AllTowers(enemyTroops);

                        else
                            PathRequestManager.RequestPath(EPos, adjustedPos, OnPathFound, ETroop.moveRange);

                    }
                }
            }
            else if (path == null) {
                if (enemyTroops.Count == 0) {
                    Debug.LogError("No hay tropas enemigas disponibles.");
                    return;
                }

                index = 0;
                actualTroop = enemyTroops[index];
                while (index < enemyTroops.Count && actualTroop != null && actualTroop.moveRange < 1) {
                    index++;
                    if (index < enemyTroops.Count) {
                        actualTroop = enemyTroops[index];
                    }
                }

                if (index >= enemyTroops.Count || actualTroop == null) {
                    AllTowers(enemyTroops);
                }
                else {
                    Cell ECell = actualTroop.transform.parent.GetComponent<Cell>();
                    if (ECell == null) {
                        Debug.LogError("La celda de la tropa actual es nula.");
                        return;
                    }

                    (int, int) EPos = ECell.GetGridPosition();

                    float maxInfluence = 0;
                    (int, int) actualPos = (0, 0);

                    for (int i = 0; i < GameManager.Instance.board.rows; i++) {
                        for (int j = 0; j < GameManager.Instance.board.columns; j++) {
                            if (GameManager.Instance.board.GetCellInfluence(i, j) >= maxInfluence)
                                if (IsCloser(EPos, actualPos, (i, j))) {
                                    actualPos = (i, j);
                                    maxInfluence = GameManager.Instance.board.GetCellInfluence(i, j);
                                }
                        }
                    }

                    PathRequestManager.RequestPath(EPos, actualPos, OnDiferentPath, actualTroop.moveRange);
                }
            }
        }

        public static bool IsCloser((int x, int y) point1, (int x, int y) point2, (int x, int y) point3)
        {
            float distance1 = MathF.Pow(point3.x - point1.x, 2) + MathF.Pow(point3.y - point1.y, 2);
            float distance2 = MathF.Pow(point3.x - point2.x, 2) + MathF.Pow(point3.y - point2.y, 2);

            return distance1 < distance2;
        }

        public void AllTowers(List<Troop> enemyTroops)
        {
            bool towers = true;
            foreach (Troop ETroop in enemyTroops)
                if (ETroop != null && ETroop.moveRange != 0)
                    towers = false;

            if (towers)
                GameManager.Instance.UseAction();
        }

        public void OnPathFound(Node[] newPath, bool pathSuccessful)
        {
            if (newPath == null) {
                Debug.LogError("El camino encontrado es nulo.");
                return;
            }

            int actual = index;
            index++;
            if (pathSuccessful) {
                if (path == null || steps > newPath.Length) {
                    path = newPath;
                    steps = path.Length;
                    actualTroop = enemyTroops[Math.Min(actual / playerTroops.Count, enemyTroops.Count - 1)];
                }
            }
            if (path != null && path.Length > 0 && index >= playerTroops.Count * enemyTroops.Count) {
                Cell destination = GameManager.Instance.board.GetCell(path[0].gridY, path[0].gridX);

                GameManager.Instance.board.MoveTroop(actualTroop, destination);
            }
        }

        public void OnDiferentPath(Node[] newPath, bool pathSuccessful)
        {
            if (newPath == null || newPath.Length == 0) {
                Debug.LogError("El camino diferente es inválido.");
                return;
            }

            Cell destination = GameManager.Instance.board.GetCell(newPath[0].gridY, newPath[0].gridX);

            GameManager.Instance.board.MoveTroop(actualTroop, destination);
        }
    }


    class IAPaint : IANode
    {

        (Troop, Cell) GetBestMoveForAllTroops(List<Troop> enemyTroops)
        {
            BoardGrid board = GameManager.Instance.board;

            // Direcciones: (dx, dy) = {Arriba, Abajo, Derecha, Izquierda}
            int[,] directions = new int[,] { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 } };

            // Función local para comprobar si una posición está dentro de los límites
            bool CheckMovementIfInBounds(int i, int j)
            {
                if (i >= 0 && i < board.rows && j >= 0 && j < board.columns) {
                    if (board.GetCell(i, j).transform.childCount > 0) {
                        return false; // La celda está ocupada
                    }
                    return true;
                }
                return false;
            }

            // Función auxiliar para calcular el puntaje de movimiento en una dirección y devolver la celda
            (float, Cell) CalculateMoveScore(int startX, int startY, int deltaX, int deltaY, Troop troop)
            {
                float moveScore = 0;
                Cell bestCell = null;

                for (int j = 1; j <= troop.moveRange; j++) {
                    int newX = startX + deltaX * j;
                    int newY = startY + deltaY * j;

                    if (!CheckMovementIfInBounds(newX, newY)) break;

                    Cell currentCell = board.GetCell(newX, newY);
                    Team color = currentCell.GetColorTeam();

                    if (color == Team.Blue) {
                        moveScore += 2;
                    }
                    else if (color == Team.None) {
                        moveScore += 1;
                    }

                    bestCell = currentCell; // Actualiza la mejor celda válida
                }

                return (moveScore, bestCell);
            }

            // Variables para rastrear el mejor movimiento global
            float bestGlobalScore = float.MinValue;
            Cell bestGlobalCell = null;
            Troop bestTroop = null;

            // Iterar sobre todas las tropas enemigas
            foreach (Troop troop in enemyTroops) {
                Cell center = troop.transform.GetComponentInParent<Cell>();
                (int x, int y) gridPosition = center.GetGridPosition();
                int x = gridPosition.x;
                int y = gridPosition.y;

                if (troop is Tower) {
                    float score = 0f;
                    // Loop through rows and columns within the specified range
                    for (int offsetX = -troop.attackRange; offsetX <= troop.attackRange; offsetX++) {
                        for (int offsetY = -troop.attackRange; offsetY <= troop.attackRange; offsetY++) {
                            int currentX = center.GetGridPosition().col + offsetX;
                            int currentY = center.GetGridPosition().row + offsetY;

                            if (currentY >= 0 && currentY < GameManager.Instance.board.rows && currentX >= 0 && currentX < GameManager.Instance.board.columns) {
                                if (GameManager.Instance.board.GetCell(currentY, currentX).transform.childCount == 0 && GameManager.Instance.board.GetCell(currentY, currentX) != center) {
                                    if (GameManager.Instance.board.GetCell(currentY, currentX).GetColorTeam() == Team.None) {
                                        score += 1;
                                    }
                                    else if (GameManager.Instance.board.GetCell(currentY, currentX).GetColorTeam() == Team.Blue) {
                                        score += 2;
                                    }
                                }
                            }
                        }
                    }
                    if (score > bestGlobalScore) {
                        bestGlobalScore = score;
                        bestGlobalCell = troop.transform.GetComponentInParent<Cell>();
                        bestTroop = troop; // Actualiza la tropa que encontró el mejor movimiento
                    }
                } else {
                    // Iterar sobre todas las direcciones y calcular el mejor puntaje
                    for (int i = 0; i < directions.GetLength(0); i++) {
                        int dx = directions[i, 0];
                        int dy = directions[i, 1];

                        var (score, cell) = CalculateMoveScore(x, y, dx, dy, troop);

                        if (score > bestGlobalScore && cell != null) {
                            bestGlobalScore = score;
                            bestGlobalCell = cell;
                            bestTroop = troop; // Actualiza la tropa que encontró el mejor movimiento
                        }
                    }
                }
                
            }

            // Retornar la tropa y la mejor celda global
            return (bestTroop, bestGlobalCell);
        }


        public override void Action()
        {
            (Troop bestTroopForAll, Cell bestCellForAll) destination = GetBestMoveForAllTroops(GameManager.Instance.enemyTroops);

            if (destination.bestTroopForAll is Tower) {
                GameManager.Instance.board.AttackWithTroop(destination.bestTroopForAll, null);
            }
            GameManager.Instance.board.MoveTroop(destination.bestTroopForAll, destination.bestCellForAll);
        }

    }


    class IASkipTurn : IANode
    {
        public override void Action()
        {
            Debug.Log("Pasar turno");
            GameManager.Instance.SkipTurn();
        }
    }

   
}


