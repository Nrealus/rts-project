using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITestField : MonoBehaviour
{
    private bool orderWheelOpened;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!orderWheelOpened)
            {
                orderWheelOpened = true;
                Debug.Log("Order Wheel Opened");
            }
            else
            {
                orderWheelOpened = false;
                Debug.Log("Order Wheel Closed");
            }
        }
    }


}
