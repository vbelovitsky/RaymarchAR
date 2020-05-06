using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class ShapePanelManager : MonoBehaviour
{

    GameObject Panel;
    public int Index { get; set; }

    List<RayShape> shapes;
    GameObject TypeDrop;
    GameObject OperationDrop;
    GameObject ColorSlider;
    GameObject BlendSlider;

    void InitElements()
    {
        Panel = this.gameObject;
        TypeDrop = GameObject.FindGameObjectWithTag("TypeDrop");
        OperationDrop = GameObject.FindGameObjectWithTag("OperationDrop");
        ColorSlider = GameObject.FindGameObjectWithTag("ColorSlider");
        BlendSlider = GameObject.FindGameObjectWithTag("BlendSlider");
        GetAllTrackables();
    }

    public void GetAllTrackables()
    {
        shapes = new List<RayShape>();
        
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Shape"))
        {
            shapes.Add(obj.GetComponent<RayShape>());
        }
    }


    public void SetData(int index)
    {
        if(Panel == null)
        {
            InitElements();
        }

        Index = index;

        TypeDrop.GetComponent<ShapeTypeDrop>().SetOptions(shapes[index]);
        OperationDrop.GetComponent<ShapeOperationDrop>().SetOptions(shapes[index]);
        
        // Устанавливаем параметры формы

    }

    
}
