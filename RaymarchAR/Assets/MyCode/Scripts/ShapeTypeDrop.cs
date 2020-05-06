using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShapeTypeDrop : MonoBehaviour
{

    Dropdown dropdown;
    RayShape currentShape;

    // Start is called before the first frame update
    void Start()
    {
        dropdown = this.gameObject.GetComponent<Dropdown>();
        dropdown.ClearOptions();
        dropdown.onValueChanged.AddListener(delegate{ OnClickHandler(dropdown); });
        string[] names = Enum.GetNames(typeof(ShapeType));
        dropdown.AddOptions(new List<string>(names));
    }

    public void OnClickHandler(Dropdown drop)
    {
        if(currentShape != null){
            Array values = Enum.GetValues(typeof(ShapeType));
            currentShape.shapeType = (ShapeType)values.GetValue(drop.value);
        }
    }

    public void SetOptions(RayShape shape)
    {
        currentShape = shape;
        dropdown.SetValueWithoutNotify((int)shape.shapeType);
    }

}
