using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSlider : MonoBehaviour
{
    // Slider components
    public Slider slider;
    public Image sliderHandle;
    public Image sliderBackGround;
 
    Texture2D colorTex;

    RayShape currentShape;

    void Awake()
    {
        colorTex = ColorStrip(360);

        Rect rect = new Rect(0, 0, colorTex.width, colorTex.height);

        sliderBackGround.sprite = Sprite.Create(colorTex, rect, rect.center);

        slider.onValueChanged.AddListener(OnValueChanged);
    }

    public void SetData(RayShape shape)
    {
        currentShape = shape;

        float hue;
        Color.RGBToHSV(shape.color, out hue, out _, out _);
        OnValueChanged(hue);
    }
 
    private void OnValueChanged(float value)
    {
        Color color = Color.HSVToRGB(value, 1, 1);
        sliderHandle.color = color;
        currentShape.color = color;
    }
 
    private Texture2D ColorStrip (int density)
    {
        Texture2D hueTex = new Texture2D (density, 1);
 
        Color[] colors = new Color[density];
        for (int k = 0; k < density; k++)
        {
            colors[k] = Color.HSVToRGB ((1.0f * k) / density, 1, 1);
        }

        hueTex.SetPixels (colors);
        hueTex.Apply ();

        return hueTex;
    }
 }