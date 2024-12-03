using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class EnemyPlayer : MonoBehaviour
{
    private GameManager gm;

    private AudioClip dropClip;
    private AudioClip moveClip;

    private void Start()
    {
        gm = GameManager.Instance;
    }

    private void Update()
    {
        
    }

    public void TestRandomAction()
    {

    }

    public void SkipTurn()
    {

    }
}


