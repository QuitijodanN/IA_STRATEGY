using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static IABase;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool isGamePaused = false; // Estado del juego
    public bool attacking = false;
    public bool yourTurn = true;

    public List<Troop> allyTroopPrefabs = new List<Troop>();
    public List<Troop> enemyTroopPrefabs = new List<Troop>();
    public BoardGrid board;
    [HideInInspector] public AudioSource audioSource;

    [SerializeField] private int maxNumActions = 5;
    [SerializeField] private int maxCoins = 20;
    [SerializeField] private int coinsRound = 5;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private BudgetCounter playerCounter;
    [SerializeField] private BudgetCounter enemyCounter;
    [SerializeField] private BudgetCounter playerCellCounter;
    [SerializeField] private BudgetCounter enemyCellCounter;
    [SerializeField] private BudgetCounter playerTroopCounter;
    [SerializeField] private BudgetCounter enemyTroopCounter;
    [SerializeField] private BudgetCounter blueActions;
    [SerializeField] private BudgetCounter redActions;


    private IAInfo aiInfo;
    private int playerCoins;
    private int enemyCoins;
    private int actions;
    private int turn = 0;

    //Private al terminar
    public List<Troop> playerTroops;
    public List<Troop> enemyTroops;

    private void Awake()
    {
        aiInfo = new IAInfo();
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
       

        audioSource = GetComponent<AudioSource>();

        ResetActions();
        playerCoins = coinsRound;
        playerCounter.DisplayValue(playerCoins);

        playerTroops = new List<Troop>();
        enemyTroops = new List<Troop>();
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

    public List<Troop> GetTroops(Team team)
    {
        if (team == Team.Blue) {
            return playerTroops;
        } else if (team == Team.Red) {
            return enemyTroops;
        }
        return null;
    }

    public void AddTroop(Troop troop)
    {
        if (troop.team == Team.Blue) {
            playerTroops.Add(troop);
            playerTroopCounter.DisplayValue(playerTroops.Count);
        }
        else if (troop.team == Team.Red) {
            enemyTroops.Add(troop);
            enemyTroopCounter.DisplayValue(enemyTroops.Count);
        }
        UpdateIAInfo();
    }

    public void RemoveTroop(Troop troop)
    {
        if (troop.team == Team.Blue) {
            playerTroops.Remove(troop);
            playerTroopCounter.DisplayValue(playerTroops.Count);
        }
        else if (troop.team == Team.Red) {
            enemyTroops.Remove(troop);
            enemyTroopCounter.DisplayValue(enemyTroops.Count);
        }
        UpdateIAInfo();
    }

    public void UpdateColorCells()
    {
        playerCellCounter.DisplayValue(board.GetColorCellAmount(Team.Blue));
        enemyCellCounter.DisplayValue(board.GetColorCellAmount(Team.Red));
    }

    public void ChangeTurn()
    {
        yourTurn = !yourTurn;

        if (yourTurn) {
            turnText.text = "Es tu turno";
            playerCoins += coinsRound + playerTroops.Count;
            playerCoins = Mathf.Clamp(playerCoins, 0, maxCoins);
            playerCounter.DisplayValue(playerCoins);
            turn++;
        }
        else {
            turnText.text = "Es el turno enemigo";
            enemyCoins += coinsRound + enemyTroops.Count;
            enemyCoins = Mathf.Clamp(enemyCoins, 0, maxCoins);
            enemyCounter.DisplayValue(enemyCoins);

        }
        ResetActions();
    }

    public void UseAction()
    {
        actions--;

        if (yourTurn)
            blueActions.DisplayValue(actions);
        else
            redActions.DisplayValue(actions);

        if (actions <= 0) {
            ChangeTurn();
        }
    }

    public void ResetActions()
    {
        actions = maxNumActions;

        if (yourTurn)
            blueActions.DisplayValue(actions);
        else
            redActions.DisplayValue(actions);
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

    public IAInfo GetIAInfo()
    {
        //Debug.Log(aiInfo.allyTeam[0].name);
        return aiInfo;
    }

    public void UpdateIAInfo()
    {
        aiInfo.enemyTeam = enemyTroops;
        aiInfo.allyTeam = playerTroops;

        //Debug.Log(aiInfo.allyTeam[0].name);


    }
}