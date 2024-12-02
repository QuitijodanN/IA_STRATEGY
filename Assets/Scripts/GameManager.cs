using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool isGamePaused; // Estado del juego
    public bool attacking = false;

    public TMP_Text text;
    public BoardGrid board;
    public List<Troop> allyTroopPrefabs = new List<Troop>();
    public List<Troop> enemyTroopPrefabs = new List<Troop>();

    public bool yourTurn = true;
    public int turn = 0;
    public int maxNumActions = 5;
    public int maxCoins = 20;

    [SerializeField] int coinsRound;
    [SerializeField] BudgetCounter playerCounter;
    [SerializeField] BudgetCounter enemyCounter;
    [SerializeField] BoxCounter playerBoxCounter;
    [SerializeField] BoxCounter enemyBoxCounter;
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
        playerCoins = maxCoins;
        playerCounter.DisplayValue(playerCoins);
    }

    public int GetCoins(Team team)
    {
        if (team == Team.Blue) return playerCoins;
        else if (team == Team.Red) return enemyCoins;
        else return 0;
    }

    public void SpendCoins(int coins, Team team)
    {
        if (team == Team.Blue) {
            playerCoins -= coins;
            playerCounter.DisplayValue(playerCoins);
        }
        else if (team == Team.Red) {
            enemyCoins -= coins;
            enemyCounter.DisplayValue(enemyCoins);
        }
    }

    public void ChangeTurn()
    {
        yourTurn = !yourTurn;

        if (yourTurn) {
            text.text = "Es tu turno";
            playerCoins += coinsRound + playerCount;
            playerCoins = Mathf.Clamp(playerCoins, 0, maxCoins);
            //playerCounter.Change_Budget(playerCoins);
            playerCounter.DisplayValue(playerCoins);
            turn++;
        }
        else {
            text.text = "Es el turno enemigo";
            enemyCoins += coinsRound + enemyCount;
            enemyCoins = Mathf.Clamp(enemyCoins, 0, maxCoins);
            //enemyCounter.Change_Budget(enemyCoins);
            enemyCounter.DisplayValue(enemyCoins);

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