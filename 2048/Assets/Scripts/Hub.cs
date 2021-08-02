using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hub : MonoBehaviour
{
    private int points;
    public Text label;

    public void Add_Points(int value)
    {
        points += value;
        label.text = points.ToString();
    }

    public void Reset()
    {
        points = 0;
        label.text = points.ToString();
    }
}
