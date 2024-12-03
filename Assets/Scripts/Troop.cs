using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class Troop : MonoBehaviour
{
    public int moveRange = 1;
    public int attackRange = 1;
    public int health = 3;
    public int damage = 3;
    public int cost = 2;
    public Team team = Team.None;
    public Effect deathPrefab;
    public AudioClip hitClip;
    public int TroopPower { get
        {
            return health + moveRange;
        }
    }

    [SerializeField] private Vector3 offset;
    [SerializeField] private GameObject healthDotPrefab;
    [SerializeField] private float dotSpacing = 0.2f;

    private GameManager gm;
    private Animator animator;
    private Transform healthBarParent;
    private Troop target;

    private void Start()
    {
        gm = GameManager.Instance;
        // Create a parent GameObject for health dots
        healthBarParent = new GameObject("HealthBar").transform;
        healthBarParent.SetParent(transform);
        healthBarParent.position = transform.parent.position - new Vector3(0, 0.4f, 0); // Position below the character

        UpdateHealthDisplay();

        animator = GetComponent<Animator>();
    }

    public void MoveToCell(Cell destination)
    {
        transform.position = destination.transform.position + offset;
        transform.SetParent(destination.transform);
    }

    public virtual void Attack(Troop troop)
    {
        target = troop;
        if (animator != null) {
            gm.attacking = true;
            animator.SetTrigger("Attack");
        } else {
            OnFrame_Attack();
        }
    }

    public void OnFrame_Attack()
    {
        gm = GameManager.Instance;
        if (target != null && target.team != team) {
            target.TakeDamage(damage);
            if (target.health <= 0) {
                Cell targetCell = target.transform.GetComponentInParent<Cell>();
                gm.board.PaintPath(targetCell, targetCell, team);
                if (target.deathPrefab != null) {
                    Instantiate(target.deathPrefab, target.transform.parent.position - new Vector3(0f, 0.8f, 0f), Quaternion.identity);
                }
                gm.RemoveTroop(target);
                Destroy(target.gameObject);
            }

            if (hitClip != null) {
                gm.audioSource.PlayOneShot(hitClip);
            }
        }
        gm.attacking = false;
    }

    public void UpdateHealthDisplay()
    {
        // Clear previous health dots
        foreach (Transform child in healthBarParent) {
            Destroy(child.gameObject);
        }

        // Calculate the starting position for centering the dots
        float startOffset = -(health - 1) * dotSpacing / 2;

        // Create new health dots
        for (int i = 0; i < health; i++) {
            Vector3 dotPosition = new Vector3(startOffset + i * dotSpacing, 0, 0);
            Instantiate(healthDotPrefab, healthBarParent.position + dotPosition, Quaternion.identity, healthBarParent);
        }
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        health = Mathf.Max(health, 0); // Ensure health doesn't go negative
        UpdateHealthDisplay();
    }

    public void Heal(int amount)
    {
        health += amount;
        UpdateHealthDisplay();
    }
}