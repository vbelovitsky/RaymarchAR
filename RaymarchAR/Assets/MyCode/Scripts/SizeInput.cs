using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SizeInput : MonoBehaviour
{
    public int Index = 0;

    InputField input;

    RayShape currentShape;

    void Init()
    {
        input = this.gameObject.GetComponent<InputField>();
        input.onValueChanged.AddListener(ChangeScale);
    }

    void ChangeScale(string value)
    {
        if(currentShape != null)
        {
            Vector3 scale = currentShape.Scale;
            float num = 0.5f;
            if(!float.TryParse(value, out num))
            {
                input.text = currentShape.Scale[Index].ToString("g4");
            }
            scale[Index] = num;
            currentShape.Scale = scale;
        }
    }

    public void SetValue(RayShape shape)
    {
        if(input == null)
        {
            Init();
        }

        currentShape = shape;
        input.text = shape.Scale[Index].ToString("g4");
    }
}
