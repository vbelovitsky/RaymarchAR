using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShapeOperationDrop : MonoBehaviour
{

    Dropdown dropdown;
    RayShape currentShape;

    void Init()
    {
        dropdown = this.gameObject.GetComponent<Dropdown>();
        dropdown.ClearOptions();
        dropdown.onValueChanged.AddListener(delegate{ OnClickHandler(dropdown); });
        string[] names = Enum.GetNames(typeof(Operation));
        dropdown.AddOptions(new List<string>(names));
    }

    public void OnClickHandler(Dropdown drop)
    {
        if(currentShape != null){
            Array values = Enum.GetValues(typeof(Operation));
            currentShape.operation = (Operation)values.GetValue(drop.value);
        }
    }

    public void SetOptions(RayShape shape)
    {
        if(dropdown == null)
        {
            Init();
        }
        
        currentShape = shape;
        dropdown.
        SetValueWithoutNotify((int)shape.operation);
    }

}
