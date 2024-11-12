using UnityEngine;

public class Troop : MonoBehaviour
{
    public Vector3 offset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MoveToCell(Cell destination)
    {
        transform.position = destination.transform.position + offset;
    }
}
