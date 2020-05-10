using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShapeTypeDrop : MonoBehaviour
{

    Dropdown dropdown;
    RayShape currentShape;

    SizePanelManager sizePanel;

    void Init()
    {
        dropdown = this.gameObject.GetComponent<Dropdown>();
        dropdown.ClearOptions();
        dropdown.onValueChanged.AddListener(delegate{ OnClickHandler(dropdown); });
        string[] names = Enum.GetNames(typeof(ShapeType));
        dropdown.AddOptions(new List<string>(names));
    }

    public void OnClickHandler(Dropdown drop)
    {
        if(currentShape != null)
        {
            Array values = Enum.GetValues(typeof(ShapeType));
            currentShape.shapeType = (ShapeType)values.GetValue(drop.value);

            if(sizePanel != null)
            {
                sizePanel.SetData(currentShape);
            }
        }
    }

    public void SetOptions(RayShape shape)
    {
        if(dropdown == null)
        {
            Init();
        }
        
        currentShape = shape;
        dropdown.SetValueWithoutNotify((int)shape.shapeType);
    }

    public void SetSizePanel(SizePanelManager spm)
    {
        sizePanel = spm;
    }

}
