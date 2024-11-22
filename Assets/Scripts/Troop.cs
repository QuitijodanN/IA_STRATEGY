using UnityEngine;

public class Troop : MonoBehaviour
{
    //Private

    //Al finalizar el testeo hay que ponerlo el mov en private
    [Tooltip("Cuanto puede moverse hacia arriba")]
    public int movUp = 1;

    [Tooltip("Cuanto puede moverse hacia abajo")]
    public int movDown = 1;

    [Tooltip("Cuanto puede moverse hacia la izquierda")]
    public int movLeft = 1;

    [Tooltip("Cuanto puede moverse hacia la derecha")]
    public int movRight = 1;

    //Public 
    public Vector3 offset;

    [Tooltip("Define si aún puede acttuar en este turno")]
    public bool turnoActtivo = true;

    

    /// <summary>
    /// Mueve a la tropa a la celda destino
    /// </summary>
    /// <param name="destination">Objeto de tipo Cell donde la troop se posicionará</param>
    public void MoveToCell(Cell destination)
    {
        transform.position = destination.transform.position + offset;

        turnoActtivo = false;
    }


}
