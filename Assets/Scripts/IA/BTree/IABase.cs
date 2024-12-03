using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class IABase : MonoBehaviour 
{
   public struct IAInfo
    {
        public List<Troop> enemyTeam;
        public List<Troop> allyTeam;
        public Troop selectedTroop;
        public Troop selectedEnemyTroop;

    }
}
