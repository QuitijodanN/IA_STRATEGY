using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;
using UnityEngine.UIElements;

public class IAEnemy : IABase
{
    
    enum NodeUpdateResult
    {
        Running,
        Success,
        Failure
    }

    interface IANode
    {
        void init();
        NodeUpdateResult Update();

    }

    class IASecunceNode : IANode
    {
        public IASecunceNode(IList<IANode> i_nodes)
        {
            n_subNodes = i_nodes;
        }
        public void init()
        {
            foreach (IANode n in n_subNodes)
            {
                n.init();
            }
        }

        public NodeUpdateResult Update()
        {
            foreach (IANode n in n_subNodes)
            {
              
                NodeUpdateResult subsubNodeResult =  n.Update();

                if(subsubNodeResult == NodeUpdateResult.Running)
                    return NodeUpdateResult.Running;
                else if(subsubNodeResult == NodeUpdateResult.Failure)
                    return NodeUpdateResult.Failure;

            }
            //Aqui va el timer
            return NodeUpdateResult.Success;
        }

        IList<IANode> n_subNodes;
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
        public void init()
        {
           
        }

        public NodeUpdateResult Update()
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
                return NodeUpdateResult.Failure;

            }
            else
            {
                
                Debug.Log("Nueva tropa seleccionada"+selectedTroop);
                GameManager.Instance.UseAction();
                return NodeUpdateResult.Success;
            }
        }
            
    }
    

    IANode n_root;
    [SerializeField]GameManager gm;

    private void Awake()
    {
       Debug.Assert(gm != null, "No hay GameManager Asignada a la IA");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        StartCoroutine(InitializeIA());
            
        
    }
    private IEnumerator InitializeIA()
    {
        // Wait for the end of the frame to ensure all components are loaded and rendered.
        yield return new WaitForEndOfFrame();

        List<IANode> secuenceNodeList = new List<IANode>();
        /*
         Crear Nodo para seleccionar unidad                             *Donde
         Crear Nodo para seleccionar unidad enemiga                     *Falta
         Crear Nodo para acercarse                                      *Falta A*
         Crear Nodo para Atacar                                         *Falta
         Crear Nodo para Comprar                                        
         */

        //Cambiar este NODO por otro
        IANode nodeSeleccionarUnidad = new IASelectTroopNode(gm.GetIAInfo());


        /* IANode nodeSeleccionarUnidadEnemiga = ...;
         IANode nodeAcercarse = ...;
         IANode nodeAtacar = ...;
         IANode nodeComprar = ....;
        */

        //Añadirlo a la lista
        secuenceNodeList.Add(nodeSeleccionarUnidad);


        /* secuenceNodeList.Add(nodeSeleccionarUnidadEnemiga);
         secuenceNodeList.Add(nodeAcercarse);
         secuenceNodeList.Add(nodeAtacar);
         secuenceNodeList.Add(nodeComprar);*/

         n_root = new IASecunceNode(secuenceNodeList);
         n_root.init();
    }



    private IEnumerator UpdateIA()
    {
        yield return new WaitForEndOfFrame();
        n_root.Update();
    }
    // Update is called once per frame
    void Update()
    {
        if(!gm.yourTurn) StartCoroutine(UpdateIA());
        //StartCoroutine(UpdateIA());
    }
}
