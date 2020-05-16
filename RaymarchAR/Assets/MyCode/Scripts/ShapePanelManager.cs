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
    GameObject typeDrop;
    GameObject operationDrop;
    GameObject colorSlider;
    GameObject blendSlider;
    GameObject sizePanel;

    void InitElements()
    {
        Panel = this.gameObject;
        typeDrop = GameObject.FindGameObjectWithTag("TypeDrop");
        operationDrop = GameObject.FindGameObjectWithTag("OperationDrop");
        colorSlider = GameObject.FindGameObjectWithTag("ColorSlider");
        blendSlider = GameObject.FindGameObjectWithTag("BlendSlider");
        sizePanel = GameObject.FindGameObjectWithTag("SizePanel");

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

        ShapeTypeDrop stp = typeDrop.GetComponent<ShapeTypeDrop>();
        stp.SetOptions(shapes[index]);

        operationDrop.GetComponent<ShapeOperationDrop>().SetOptions(shapes[index]);
        colorSlider.GetComponent<ColorSlider>().SetData(shapes[index]);
        blendSlider.GetComponent<BlendSlider>().SetData(shapes[index]);

        SizePanelManager spm = sizePanel.GetComponent<SizePanelManager>();
        spm.SetData(shapes[index]);

        stp.SetSizePanel(spm);

    }
}
