using System.Collections;
using UnityEngine;

public class Tower : Troop
{
    [SerializeField] private Effect areaEffect;
    [SerializeField] private AudioClip towerClip;
    public void PlayEffect()
    {
        if (areaEffect != null) {
            Instantiate(areaEffect, transform.parent.position, Quaternion.identity);
        }
        if (towerClip != null) {
            GameManager.Instance.GetComponent<AudioSource>().PlayOneShot(towerClip);
        }
    }
}
