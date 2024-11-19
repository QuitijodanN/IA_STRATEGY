using System.Collections.Generic;
using UnityEngine;

public class TeamsManager : MonoBehaviour
{
    public Troop troopPrefab1 = null;
    public Troop troopPrefab2 = null;

    public int numberOfEnemies = 2;
    public int numberOfAllies  = 2;

    public List<Troop> equipoAliado;
    public List <Troop> equipoEnemigo;
   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        for (int i = 0; i < numberOfAllies; i++)
            equipoAliado.Add(troopPrefab1);

        for (int i = 0; i < numberOfEnemies; i++)
            equipoEnemigo.Add(troopPrefab2);
    }

    public void eliminarEnemigo(Troop enemigo)
    {
        equipoEnemigo.Remove(enemigo);
    }

    public void eliminarAliado(Troop aliado)
    {
        equipoAliado.Remove(aliado);
    }
   
}
