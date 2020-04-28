using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShapeTypes
{
    Cube,
    Sphere,
    Plane,
    Torus,
    Cylinder
}

public enum Operations
{
    Union,
    Substraction,
    Intersection
}

public class Shape : MonoBehaviour
{

    public ShapeTypes SType;

    public Vector3 Position
    {
        get
        {
            return this.transform.position;
        }
    }

    public Vector3 Size = new Vector3(1, 0, 0);

    public Color SColor;

    public Operations Operation;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
