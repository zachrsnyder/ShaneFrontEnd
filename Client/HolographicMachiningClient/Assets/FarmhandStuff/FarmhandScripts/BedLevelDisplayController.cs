using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BedLevelDisplayController : MonoBehaviour
{
    [SerializeField] private TextMeshPro front_left;
    [SerializeField] private TextMeshPro front_right;
    [SerializeField] private TextMeshPro back_left;
    [SerializeField] private TextMeshPro back_right;

    public float front_left_val
    {

        set
        {
            //front_left.text = "No need to adjust!";
            if (value < 0)
            {
                front_left.text = "Front Left:\nCounter Clockwise\n" + Math.Abs(value).ToString("0.00") + " Turns";
            }
            else if (value > 0)
            {
                front_left.text = "Front Left:\nClockwise\n" + Math.Abs(value).ToString("0.00") + " Turns";
            }
            else //TODO: Add a "range" to this else, so that if the adjustment is less than 10 min or so, we don't tell the user to mess with it.
            {
                front_left.text = "No need to adjust!";
            }

        }
    }

    public float front_right_val
    {

        set
        {
            //front_right.text = "No need to adjust!";
            if (value < 0)
            {
                front_right.text = "Front Right:\nCounter Clockwise\n" + Math.Abs(value).ToString("0.00") + " Turns";
            }
            else if (value > 0)
            {
                front_right.text = "Front Right:\nClockwise\n" + Math.Abs(value).ToString("0.00") + " Turns";
            }
            else
            {
                front_right.text = "No need to adjust!";
            }

        }
    }

    public float back_left_val
    {
        set
        {
            //back_left.text = "No need to adjust!";
            if (value < 0)
            {
                back_left.text = "Back Left:\nCounter Clockwise\n" + Math.Abs(value).ToString("0.00") + " Turns";
            }
            else if (value > 0)
            {
                back_left.text = "Back Left:\nClockwise\n" + Math.Abs(value).ToString("0.00") + " Turns";
            }
            else
            {
                back_left.text = "No need to adjust!";
            }

        }
    }

    public float back_right_val
    {

        set
        {
            //back_right.text = "No need to adjust!";
            if (value < 0)
            {
                back_right.text = "Back Right:\nCounter Clockwise\n" + Math.Abs(value).ToString("0.00") + " Turns";
            }
            else if (value > 0)
            {
                back_right.text = "Back Right:\nClockwise\n" + Math.Abs(value).ToString("0.00") + " Turns";
            }
            else
            {
                back_right.text = "No need to adjust!";
            }

        }
    }
}
