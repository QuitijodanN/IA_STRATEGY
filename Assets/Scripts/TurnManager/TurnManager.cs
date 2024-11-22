using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public TMP_Text text;

    //Cambiar a private cuando termine el test
    //public bool esTuTurno { get; private set; }

    //public bool eselTurnoEnemigo { get; private set; }

    public bool esTuTurno;

    public bool eselTurnoEnemigo;

    private TeamsManager teamsManager;

    private int maxNumActionsPerAllyTurn;
    private int maxNumActionsPerEnemyTurn;

    //[HideInInspector]
    public int numeroJugadasAliadas;
    //[HideInInspector]
    public int numeroJugadasEnemigas;

    private void Awake()
    {
        teamsManager = GetComponent<TeamsManager>();
        

        numeroJugadasAliadas  = maxNumActionsPerAllyTurn;
        numeroJugadasEnemigas = 0;

        esTuTurno = true;
        eselTurnoEnemigo = false;
        
    }

    public void setMaxNumberActionsPerAllyTurn(int plays) { maxNumActionsPerAllyTurn = plays; }
    public void setMaxNumberActionsPerEnemyTurn(int plays) { maxNumActionsPerEnemyTurn = plays; }

    public void CambiarTurno()
    {  
        eselTurnoEnemigo = !eselTurnoEnemigo;

        esTuTurno = !esTuTurno;
       

        if (esTuTurno)
        {
            text.text = "Es tú turno";
            numeroJugadasAliadas = maxNumActionsPerAllyTurn;

            foreach (Troop tropa in teamsManager.equipoAliado)
            {
                tropa.turnoActtivo = true;
            }
        }
        else
        {
            text.text = "Es el turno enemigo";
            numeroJugadasEnemigas = maxNumActionsPerEnemyTurn;

            foreach (Troop tropa in teamsManager.equipoEnemigo)
            {
                tropa.turnoActtivo = true;
            }
        }
    }


}
