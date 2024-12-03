using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class TroopDeployer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject frameOutline;
    public GameObject troopImage;
    public Troop troopPrefab; // Reference to the 3D asset prefab
    [SerializeField] private AudioClip grabClip;
    [SerializeField] private AudioClip notAllowedClip;

    private Vector2 offset;
    private Vector2 initialAnchoredPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

    private GameManager gm;

    void Start()
    {
        gm = GameManager.Instance;

        // Store the initial RectTransform state
        RectTransform rectTransform = troopImage.GetComponent<RectTransform>();
        initialAnchoredPosition = rectTransform.anchoredPosition;
        initialRotation = rectTransform.localRotation;
        initialScale = rectTransform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (frameOutline != null) {
            frameOutline.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (frameOutline != null) {
            frameOutline.SetActive(false);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        gm.audioSource.PlayOneShot(grabClip);
        // Calculate offset from pointer position to the center of the card
        RectTransform rectTransform = troopImage.GetComponent<RectTransform>();
        offset = (Vector2)rectTransform.position - eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateTroopPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        HandleDropOrReset();
    }

    private void UpdateTroopPosition(Vector2 position)
    {
        Image image = troopImage.GetComponent<Image>();
        if (position.y > 100f) {
            image.color = new Color(1f, 1f, 1f, 0.2f);
        }
        else {
            image.color = new Color(1f, 1f, 1f, 1f);
        }

        RectTransform rectTransform = troopImage.GetComponent<RectTransform>();
        rectTransform.position = position + offset;
    }

    private void HandleDropOrReset()
    {
        if (gm.yourTurn) {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null) {
                Cell cell = hit.collider.GetComponent<Cell>();
                float currentGold = gm.GetCoins(Team.Blue);

                if (cell != null && currentGold >= troopPrefab.cost && cell.transform.childCount == 0 &&
                    (cell.GetColorTeam() == Team.Blue || (cell.GetColorTeam() != Team.Red && troopPrefab is Bomb))) {
                    gm.SpendCoins(troopPrefab.cost, troopPrefab.team);
                    gm.board.SpawnTroop(troopPrefab, cell);
                }
                else {
                    gm.audioSource.PlayOneShot(notAllowedClip);
                }
            }
        }

        ResetTroopPosition();
    }

    private void ResetTroopPosition()
    {
        RectTransform rectTransform = troopImage.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = initialAnchoredPosition;
        rectTransform.localRotation = initialRotation;
        rectTransform.localScale = initialScale;

        Image image = troopImage.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 1f);
    }
}