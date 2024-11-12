
using UnityEngine;

public class Cell : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    
    private bool isAvailable = true;
    private bool isActiveSelection = false;
    private Troop troop = null;

    private BoardGrid boardGrid = null;
    private float row = 0;
    private float col = 0;

    public Color stateColor = new Color(1f, 1f, 1f, 0f);
    public Color hoverColor = new Color(1f, 1f, 1f, 0.5f);

    void Start()
    {
        // Add SpriteRenderer and set up its properties
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.color = stateColor;
    }

    public void SetPosition(BoardGrid board, float row, float col)
    {
        boardGrid = board;
        this.row = row;
        this.col = col;
    }

    public void SetTroop(Troop troop)
    {
        this.troop = troop;
        isAvailable = false;
    }

    public void SetActiveSelection(bool isActive)
    {
        isActiveSelection = isActive;
    }

    public Vector2 GetPosition()
    {
        return new Vector2(row, col);
    }

    void OnMouseDown()
    {
        if (isActiveSelection) {
            boardGrid.ConfirmAction();
        // Change to hover color if the cell is available
        if (troop != null) {
            spriteRenderer.color = hoverColor;
        }
    }

    void OnMouseEnter()
    {
        spriteRenderer.color = hoverColor;
    }

    void OnMouseExit()
    {
        spriteRenderer.color = stateColor;
    }
}