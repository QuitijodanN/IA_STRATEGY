using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class BudgetCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI txt;

    public void Change_Budget (int budget)
    {

        txt.text = budget.ToString();

        StartCoroutine(Increase(budget));
    }
    IEnumerator Increase(int budget)
    {
        if (transform.localScale.y < 0.1f * budget) {
            while (transform.localScale.y < 0.1f * budget)
            {
                float actualScale = gameObject.transform.localScale.y;
                float actualPos = gameObject.transform.localPosition.y;
                gameObject.transform.localScale = new Vector3(1, actualScale + 0.01f, 1);
                gameObject.transform.localPosition = new Vector3(0, actualPos + 2.425f, 0);

                yield return new WaitForSeconds(0.05f);
            }                             
        }
        else {
            while (transform.localScale.y > 0.1f * budget)
            {
                float actualScale = gameObject.transform.localScale.y;
                float actualPos = gameObject.transform.localPosition.y;
                gameObject.transform.localScale = new Vector3(1, actualScale - 0.01f, 1);
                gameObject.transform.localPosition = new Vector3(0, actualPos - 2.425f, 0);

                yield return new WaitForSeconds(0.05f);
            }         
        }
        gameObject.transform.localScale = new Vector3(1, 0.1f * budget, 1);
        gameObject.transform.localPosition = new Vector3(0, budget * 24.25f -242.5f, 0);
    }
}
