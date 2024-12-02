using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class BudgetCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI txt;
    private Slider slider;


    private void Start()
    {
        slider = GetComponent<Slider>();
    }

    public void DisplayValue(int coinValue)
    {
        slider.value = coinValue;
        txt.text = coinValue.ToString();
    }


    /*
    public void Change_Budget (int budget)
    {

        txt.text = budget.ToString();

        StartCoroutine(Increase(budget));
    }
    IEnumerator Increase(int budget)
    {
        if (transform.localScale.x < 0.1f * budget) {
            while (transform.localScale.x < 0.1f * budget)
            {
                float actualScale = gameObject.transform.localScale.x;
                float actualPos = gameObject.transform.localPosition.x;
                gameObject.transform.localScale = new Vector3(actualScale + 0.01f, 1, 1);
                gameObject.transform.localPosition = new Vector3(actualPos + maxLenght / 200, 0, 0);

                yield return new WaitForSeconds(0.05f);
            }                             
        }
        else {
            while (transform.localScale.x > 0.1f * budget)
            {
                float actualScale = gameObject.transform.localScale.x;
                float actualPos = gameObject.transform.localPosition.x;
                gameObject.transform.localScale = new Vector3(actualScale - 0.01f, 1, 1);
                gameObject.transform.localPosition = new Vector3(actualPos - maxLenght / 200, 0, 0);

                yield return new WaitForSeconds(0.05f);
            }         
        }
        gameObject.transform.localScale = new Vector3(0.1f * budget, 1, 1);
        gameObject.transform.localPosition = new Vector3(budget * maxLenght / 20 - maxLenght / 2, 0, 0);
    }*/
}
