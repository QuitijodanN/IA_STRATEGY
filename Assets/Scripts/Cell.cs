using UnityEditor;
using UnityEngine;

public class Cell : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private Color stateColor = new Color(0f, 0f, 0f, 0f); // Fully transparent by default
    private Color hoverColor = new Color(1f, 1f, 1f, 0.3f); // Fully transparent by default
    private Color teamColor = new Color(0f, 0f, 0f, 0f);  // Fully transparent by default
    private Color finalColor = new Color(0f, 0f, 0f, 0f);  // Fully transparent result initially

    private Selection activeSelection = Selection.Empty;
    private Team team = Team.None;
    private BoardGrid boardGrid = null;
    private GameManager gm;
    private int row = 0;
    private int col = 0;

    void Start()
    {
        // Initialize SpriteRenderer
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        UpdateCellColor();
        gm = GameManager.Instance;
    }

    public void SetGridPosition(BoardGrid board, int row, int col)
    {
        boardGrid = board;
        this.row = row;
        this.col = col;
    }



    public void SetActiveSelection(Selection s)
    {
        activeSelection = s;

        switch (s) {
            case Selection.Movement:
                stateColor = new Color(0f, 1f, 1f, 0.5f); // Cyan tint with transparency
                break;
            case Selection.Attack:
                stateColor = new Color(1f, 0f, 0f, 0.5f); // Red tint with transparency
                break;
            default:
                stateColor = new Color(0f, 0f, 0f, 0f); // Reset to transparent
                break;
        }

        UpdateCellColor();
    }

    public void SetColorTeam(Team t)
    {
        team = t;

        switch (t) {
            case Team.Blue:
                teamColor = new Color(0f, 0f, 1f, 0.3f); // Blue tint with lower transparency
                break;
            case Team.Red:
                teamColor = new Color(1f, 0.3f, 0f, 0.3f); // Orange tint with lower transparency
                break;
            default:
                teamColor = new Color(0f, 0f, 0f, 0f); // Reset to transparent
                break;
        }

        UpdateCellColor();
    }

    public Team GetColorTeam()
    {
        return team;
    }

    public (int row, int col) GetGridPosition()
    {
        return (row, col);
    }

    // Updates the cell's color based on the current team and state
    private void UpdateCellColor()
    {
        finalColor = BlendColors(teamColor, stateColor);
        spriteRenderer.color = finalColor;
    }

    // Blends two colors with transparency, similar to how tinted glass layers would appear
    private Color BlendColors(Color baseColor, Color overlayColor)
    {
        float alphaBase = baseColor.a;
        float alphaOverlay = overlayColor.a;

        // Compute final alpha using Porter-Duff "source-over" formula
        float finalAlpha = alphaOverlay + alphaBase * (1 - alphaOverlay);

        if (finalAlpha <= 0) return new Color(0, 0, 0, 0); // Fully transparent if alpha is zero

        // Blend RGB channels using alpha weighting
        float r = (overlayColor.r * alphaOverlay + baseColor.r * alphaBase * (1 - alphaOverlay)) / finalAlpha;
        float g = (overlayColor.g * alphaOverlay + baseColor.g * alphaBase * (1 - alphaOverlay)) / finalAlpha;
        float b = (overlayColor.b * alphaOverlay + baseColor.b * alphaBase * (1 - alphaOverlay)) / finalAlpha;

        return new Color(r, g, b, finalAlpha);
    }

    void OnMouseDown()
    {
        if (gm.attacking) return;
        // Cuando clicas en una celda sin tropas (vacía)
        if (transform.childCount == 0) {
            // Cuando clicas sobre una selección de movimiento (exclusivo de celdas vacías)
            if (activeSelection == Selection.Movement) {
                boardGrid.MoveTroop(boardGrid.selectedTroop, this);
            }
            else if (activeSelection == Selection.Attack) {
                boardGrid.AttackWithTroop(boardGrid.selectedTroop, null);
            }
        }
        // Cuando clicas sobre una tropa
        else {
            Troop clickedTroop = transform.GetChild(0).GetComponent<Troop>();
            // Cuando clicas sobre una tropa a la que atacar
            if (activeSelection == Selection.Attack) {
                boardGrid.AttackWithTroop(boardGrid.selectedTroop, clickedTroop);
            }
            // Cuando clicas sobre una tropa que quieres usar para mover o atacar
            else {
                // Solo seleccionables las tropas del turno correspondiente
                if (gm.yourTurn && clickedTroop.team == Team.Blue) {
                    boardGrid.ActivateMovementSelection(clickedTroop);
                    boardGrid.ActivateAttackSelection(clickedTroop);
                }
            }
        }
    }

    void OnMouseEnter()
    {
        spriteRenderer.color = BlendColors(finalColor, hoverColor);
    }

    void OnMouseExit()
    {
        // Reset to the pre-hover color
        spriteRenderer.color = finalColor;
    }
}