using UnityEngine;

public class ActionCounter : MonoBehaviour
{
    [SerializeField] private GameObject actionPrefab;


    public void UpdateActionDisplay(int actions)
    {
        // Clear previous health dots
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Calculate the starting position for centering the dots
        float startOffset = -(actions - 1) * 1 / 2;

        // Create new health dots
        for (int i = 0; i < actions; i++)
        {
            Vector3 dotPosition = new Vector3(startOffset + i * 1, 0, 0);
            Instantiate(actionPrefab, transform.position + dotPosition, Quaternion.identity, transform);
        }
    }
}
