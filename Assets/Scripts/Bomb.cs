using System.Collections;
using UnityEngine;

public class Bomb : Troop
{
    public float explosionSecondDelay = 2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Explode());
    }

    private IEnumerator Explode()
    {
        yield return new WaitForSeconds(explosionSecondDelay);

        if (hitClip != null) {
            GameManager.Instance.GetComponent<AudioSource>().PlayOneShot(hitClip);
        }
        Cell cell = transform.GetComponentInParent<Cell>();
        GameManager.Instance.board.AttackWithArea(this);
        Instantiate(deathPrefab, cell.transform.position - new Vector3(0f, 1.2f, 0f), Quaternion.identity);

        Destroy(gameObject);
    }

}
