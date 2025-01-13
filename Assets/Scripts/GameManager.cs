using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    [SerializeField] private BudgetCounter playerCounter;
    [SerializeField] private BudgetCounter enemyCounter;
    [SerializeField] private BudgetCounter playerCellCounter;
    [SerializeField] private BudgetCounter enemyCellCounter;
    [SerializeField] private BudgetCounter playerTroopCounter;
    [SerializeField] private BudgetCounter enemyTroopCounter;
    [SerializeField] private BudgetCounter blueActions;
    [SerializeField] private BudgetCounter redActions;
    [SerializeField] private TMP_Text turnCounter;
    [SerializeField] private TMP_Text finishText;
    [SerializeField] private Animator changeTurn;

    private IAInfo aiInfo;

    //Cambiar a private
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

        turn = 1;
        turnCounter.text = turn.ToString();
    }

    public int GetCoins(Team team)
    {
        if (team == Team.Blue) return playerCoins;
        else if (team == Team.Red) return enemyCoins;
        else return 0;
    }

    public int GetTurn()
    {
        return turn;
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
        }
        else if (team == Team.Red) {
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

    public void SkipButton()
    {
        if (!yourTurn) return;

        SkipTurn();
    }

    public void SkipTurn()
    {
        int red = board.GetColorCellAmount(Team.Red);
        int blue = board.GetColorCellAmount(Team.Blue);

        if (red <= 0)
            WinGame();
        if (blue <= 0)
            LoseGame();

        //Compruebas que hay amount de ambos colores
        while (actions > 0) {
            actions--;
            board.ActualizeInfluence();
            if (yourTurn)
                blueActions.DisplayValue(actions);
            else
                redActions.DisplayValue(actions);
        }

        ChangeTurn();
    }

    public void UpdateColorCells()
    {
        playerCellCounter.DisplayValue(board.GetColorCellAmount(Team.Blue));
        enemyCellCounter.DisplayValue(board.GetColorCellAmount(Team.Red));
    }
    public void LoseGame()
    {
        finishText.gameObject.SetActive(true);
        finishText.text = "LOSE";
        
    }
    public void WinGame()
    {
        finishText.gameObject.SetActive(true);
        finishText.text = "WIN";
    }
    public void TieGame()
    {
        finishText.gameObject.SetActive(true);
        finishText.text = "TIE";
    }

    public void ChangeTurn()
    {
        yourTurn = !yourTurn;

        int red = board.GetColorCellAmount(Team.Red);
        int blue = board.GetColorCellAmount(Team.Blue);
        if (turn >= 20) {
            if (red > blue)
                LoseGame();
            else if (red < blue)
                WinGame();
            else
                TieGame();
        }
        else {

            if (yourTurn) {
                //turnText.text = "Es tu turno";
                changeTurn.SetTrigger("blueTurn");
                playerCoins += coinsRound + playerTroops.Count;
                playerCoins = Mathf.Clamp(playerCoins, 0, maxCoins);
                playerCounter.DisplayValue(playerCoins);
            }
            else {
                //turnText.text = "Es el turno enemigo";
                changeTurn.SetTrigger("redTurn");
                enemyCoins += coinsRound + enemyTroops.Count;
                enemyCoins = Mathf.Clamp(enemyCoins, 0, maxCoins);
                enemyCounter.DisplayValue(enemyCoins);

            }
            turn++;
            turnCounter.text = turn.ToString();
            board.ActualizeInfluence();
            ResetActions();
        }
    }

    public void UseAction()
    {
        int red = board.GetColorCellAmount(Team.Red);
        int blue = board.GetColorCellAmount(Team.Blue);

        if (red <= 0)
            WinGame();
        if (blue <= 0)
            LoseGame();

        //Compruebas que hay amount de ambos colores
        actions--;
        board.ActualizeInfluence();
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