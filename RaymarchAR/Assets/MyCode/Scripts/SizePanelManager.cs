using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SizePanelManager : MonoBehaviour
{
    GameObject Panel;

    RayShape currentShape;

    void Init()
    {
        Panel = this.gameObject;
    }

    public void SetData(RayShape shape)
    {
        if(Panel == null)
        {
            Init();
        }
        currentShape = shape;

        SizeInput[] inputs = Panel.GetComponentsInChildren<SizeInput>();
        foreach(SizeInput input in inputs)
        {
            input.SetValue(currentShape);
        }
    }
}
