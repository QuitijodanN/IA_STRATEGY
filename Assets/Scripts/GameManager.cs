using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool isGamePaused; // Estado del juego

    public TMP_Text text;
    public List<Troop> allyTroopPrefabs = new List<Troop>();
    public List<Troop> enemyTroopPrefabs = new List<Troop>();

    public bool yourTurn = true;
    public int turn = 0;
    public int maxNumActions = 5;

    [SerializeField] int coinsRound;
    [SerializeField] BudgetCounter enemyCounter;
    [SerializeField] BudgetCounter playerCounter;
    [SerializeField] int playerCount;
    [SerializeField] int enemyCount;
    private int playerCoins;
    private int enemyCoins;
    private int actions;

    private void Awake()
    {
        // Asegurarse de que solo haya una instancia del GameManager
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Mantener el GameManager al cambiar escenas
        }
        else {
            Destroy(gameObject); // Destruir duplicados
        }
    }

    private void Start()
    {
        ResetActions();
    }
    public void ChangeTurn()
    {
        yourTurn = !yourTurn;

        if (yourTurn) {
            text.text = "Es tu turno";
            playerCoins += coinsRound + playerCount;
            playerCoins = Mathf.Clamp(playerCoins, 0, 10);
            playerCounter.Change_Budget(playerCoins);
            turn++;
        }
        else {
            text.text = "Es el turno enemigo";
            enemyCoins += coinsRound + enemyCount;
            enemyCoins = Mathf.Clamp(enemyCoins, 0, 10);
            enemyCounter.Change_Budget(enemyCoins);
        }
        ResetActions();
    }

    public void UseAction()
    {
        actions--;
        if (actions <= 0) {
            ChangeTurn();
        }
    }

    public void ResetActions()
    {
        actions = maxNumActions;
    }

    public void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f; // Detiene la física y animaciones
    }

    public void ResumeGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f; // Restaura la física y animaciones
    }
}