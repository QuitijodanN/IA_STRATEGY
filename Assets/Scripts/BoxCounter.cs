using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class BoxCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI txt;
    //private Slider slider;


    private void Start()
    {
        //slider = GetComponent<Slider>();
    }

    public void DisplayValue(int boxValue)
    {
        //slider.value = coinValue;
        txt.text = boxValue.ToString();
    }
}
