using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetButton : MonoBehaviour
{
    public GameObject targetObject;

    void Start()
    {
        targetObject = GameObject.Find("Plane");
    }

    public void OnClick() 
    {
        Paint paint = targetObject.GetComponent<Paint>();
        paint.ClearCanvas();
    }
}
