using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShapeOperationDrop : MonoBehaviour
{

    Dropdown dropdown;
    RayShape currentShape;

    // Start is called before the first frame update
    void Start()
    {
        dropdown = this.gameObject.GetComponent<Dropdown>();
        dropdown.ClearOptions();
        dropdown.onValueChanged.AddListener(delegate{ OnClickHandler(dropdown); });
        
        dropdown.AddOptions(new List<string>(Enum.GetNames(typeof(Operation))));
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
        currentShape = shape;
        dropdown.SetValueWithoutNotify((int)shape.operation);
    }

}
