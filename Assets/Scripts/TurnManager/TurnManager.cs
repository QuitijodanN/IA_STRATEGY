//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class TurnManager : MonoBehaviour
//{
//    public TMP_Text text;

//    public bool yourTurn = true;
//    public int turn = 0;

//    private TeamsManager teamsManager;

//    public int maxNumActions = 5;

//    private int coins;
//    private int enemyCoins;
//    private int actions;

//    private void Awake()
//    {
//        teamsManager = GetComponent<TeamsManager>();
        
//    }

//    public void CambiarTurno()
//    {  
//        yourTurn = !yourTurn;

//        if (yourTurn)
//        {
//            text.text = "Es tú turno";
//            coins += 10;
//            turn++;
//        }
//        else
//        {
//            text.text = "Es el turno enemigo";
//            enemyCoins += 10;
//        }
//        actions = maxNumActions;
//    }
//}
