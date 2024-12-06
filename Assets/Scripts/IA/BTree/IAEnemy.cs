
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                Think(); // Llamar a la funci�n Think
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
        IADeploy deploy = new IADeploy();
        IAAttack attack = new IAAttack();

        // atamos nodos a los padres (sequence nodes)
        IAEnoughGold enoughGold = new IAEnoughGold(deploy, attack);

        n_root = enoughGold;
    }

    // -----------------------------------------------------------------------------------------------------------------------------------------
    // -- NODOS EJEMPLO
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

    class IAAttack : IANode
    {
        public override void Action()
        {
            Debug.Log("adios");
            GameManager.Instance.UseAction();
        }
    }

    class IADeploy : IANode
    {
        public override void Action()
        {
            Debug.Log("hola");
            GameManager.Instance.UseAction();
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
            

            //Si no hemos seleccionado otra unidad seleccionamos la Unidad m�s fuerte que tenemos -> success
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
                //Si ya tenemos una unidad seleccionada vemos cual es la siguiente m�s fuerte -> success
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

    //    private IEnumerator InitializeIA()
    //    {
    //        // Wait for the end of the frame to ensure all components are loaded and rendered.
    //        yield return new WaitForEndOfFrame();

    //        List<IANode> secuenceNodeList = new List<IANode>();
    //        /*
    //         Crear Nodo para seleccionar unidad
    //         Crear Nodo para seleccionar unidad enemiga
    //         Crear Nodo para acercarse
    //         Crear Nodo para Atacar
    //         Crear Nodo para Comprar
    //         */

    //        //Cambiar este NODO por otro
    //        //IANode nodeSeleccionarUnidad = new IASelectTroopNode(gm.GetIAInfo());
    //        //IANode nodeSeleccionarUnidadEnemiga = new IASelectEnemyTroopNode(gm.GetIAInfo());
    //        /* 
    //         IANode nodeAcercarse = ...;
    //         IANode nodeAtacar = ...;
    //         IANode nodeComprar = ....;
    //        */

    //        //A�adirlo a la lista
    //        //secuenceNodeList.Add(nodeSeleccionarUnidad);
    //        //secuenceNodeList.Add(nodeSeleccionarUnidadEnemiga);


    //        /* secuenceNodeList.Add(nodeSeleccionarUnidadEnemiga);
    //         secuenceNodeList.Add(nodeAcercarse);
    //         secuenceNodeList.Add(nodeAtacar);
    //         secuenceNodeList.Add(nodeComprar);*/

    //         n_root = new IASequenceNode(secuenceNodeList);
    //         n_root.Init();
    //    }
}


