
using UnityEngine;

public class Cell : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    private bool isActiveSelection = false;

    private BoardGrid boardGrid = null;
    private float row = 0;
    private float col = 0;

    public Color stateColor = new Color(1f, 1f, 1f, 0f);

    public TurnManager  turnManager;
    public TeamsManager teamsManager;

    private void Awake()
    {
        turnManager = GameObject.Find("GameManager").GetComponent<TurnManager>();
        teamsManager = GameObject.Find("GameManager").GetComponent<TeamsManager>();
    }

    void Start()
    {
        // Add SpriteRenderer and set up its properties
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.color = stateColor;
    }

    public void SetGridPosition(BoardGrid board, float row, float col)
    {
        boardGrid = board;
        this.row = row;
        this.col = col;
    }


    public void SetActiveSelection(bool isActive)
    {
        isActiveSelection = isActive;
        if (isActive ) {
            stateColor = new Color(0f, 1f, 1f, 0.3f);
            spriteRenderer.color = stateColor;
        } else {
            stateColor = new Color(1f, 1f, 1f, 0f);
            spriteRenderer.color = stateColor + new Color(0f, 0f, 0f, 0.3f);
        }
    }

    public Vector2 GetGridPosition()
    {
        return new Vector2(row, col);
    }

    void OnMouseDown()
    {
        
        // Change to hover color if the cell is available
        if (transform.childCount == 0) {
            if (isActiveSelection) {
                boardGrid.MoveSelectedTroop(this);
            }
            boardGrid.ResetGridActiveSelections();
        } else {

            /*Ahora las unidades se moverán de forma distinta en función del tipo de tropa
             * Los atributos de cuanto se puede mover una tropa está en su clase
             * Es más facil de modificar al no tener que tocar el codigo de movimiento
             */
            Troop selectedTroop = this.transform.GetChild(0).GetComponent<Troop>();

            if (selectedTroop.turnoActtivo)
                boardGrid.ActivateSelection(this, selectedTroop.movUp, selectedTroop.movDown, selectedTroop.movRight, selectedTroop.movLeft);
                
    
            
               
            
        }
    }

    void OnMouseEnter()
    {
        spriteRenderer.color = stateColor + new Color(0f, 0f, 0f, 0.3f);
    }

    void OnMouseExit()
    {
        spriteRenderer.color = stateColor;
    }
}