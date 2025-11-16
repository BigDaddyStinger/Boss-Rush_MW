using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bar : MonoBehaviour
{
    [SerializeField] Image barFill;

    int maximumValue;

    public void SetMax(int max)
    {
        maximumValue = max;
    }

    public void UpdateBar(int change, int newCurrent)
    {
        barFill.fillAmount = (float)newCurrent/(float)maximumValue;
    }
}
