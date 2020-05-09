using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ShapeDataHolder : MonoBehaviour
{

    List<RayShape> shapes;

    void Start()
    {
        shapes = new List<RayShape>();
        //GetAllTrackables();
    }

    
}
