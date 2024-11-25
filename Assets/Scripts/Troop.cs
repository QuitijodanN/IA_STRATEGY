using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class Troop : MonoBehaviour
{
    [Tooltip("Cuanto puede moverse")]
    public int moveRange = 1;
    public int attackRange = 1;
    public int health = 3;
    public int damage = 3;
    public Team team = Team.None;

    [SerializeField] private Vector3 offset;
    [SerializeField] private GameObject healthDotPrefab; // Prefab for the health dot
    [SerializeField] private float dotSpacing = 0.2f;    // Spacing between dots
    private Transform healthBarParent;                  // Parent for health dots

    private void Start()
    {
        // Create a parent GameObject for health dots
        healthBarParent = new GameObject("HealthBar").transform;
        healthBarParent.SetParent(transform);
        healthBarParent.position = transform.parent.position - new Vector3(0, 0.4f, 0); // Position below the character

        UpdateHealthDisplay();
    }

    public void MoveToCell(Cell destination)
    {
        transform.position = destination.transform.position + offset;
        transform.SetParent(destination.transform);
    }

    public void Attack(Troop troop)
    {
        if (troop.team != team)
        {
            troop.TakeDamage(damage);
            if (troop.health <= 0) {
                Destroy(troop.gameObject);
            }
        }
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