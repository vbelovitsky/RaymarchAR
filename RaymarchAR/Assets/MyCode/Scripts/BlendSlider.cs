using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlendSlider : MonoBehaviour
{
    public Slider slider;

    RayShape currentShape;

    void Awake()
    {
        slider.onValueChanged.AddListener(OnValueChanged);
    }

    void OnValueChanged(float value)
    {
        currentShape.blendStrength = value;
    }

    public void SetData(RayShape shape)
    {
        currentShape = shape;
        slider.value = shape.blendStrength;
    }
}
