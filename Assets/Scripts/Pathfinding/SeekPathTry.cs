using System.Collections;
using System.IO;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SeekPathTry : MonoBehaviour
{
    [SerializeField] Grid grid;

    Node[] path;
    int targetIndex = 0;

    public void Try()
    {
        PathRequestManager.RequestPath((1,2), (3, 7), OnPathFound, false);
    }

    public void OnPathFound(Node[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            targetIndex = 0;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator FollowPath()
    {
        if (path.Length > 0)
        {
            Node currentNode = path[0];
            while (targetIndex < path.Length)
            {
                currentNode = path[targetIndex];
                Debug.Log(currentNode.gridY + "y /" + currentNode.gridX + "x");
                targetIndex++;

                yield return null;
            }
        }
    }
}
