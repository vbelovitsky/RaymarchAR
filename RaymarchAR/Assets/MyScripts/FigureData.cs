using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  enum FigTypes
{
        Cube,
        Sphere,
}

public class FigureData : MonoBehaviour
{
    

    public Collider FigCol;
    private Transform FigTr;

    private FigTypes type;
    public FigTypes FigType
    {
        get
        {
            return type;
        }
    }

    public float Side
    {
        get
        {
            return 0.5f;
        }
    }
    public Vector3 Position
    {
        get
        {
            return FigTr.position;
        }
    }
    public Vector4 PositionAndSide
    {
        get
        {
            return new Vector4(
                Position.x,
                Position.y,
                Position.z,
                Side
            );
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        FigCol = GetComponent<Collider>();
        FigTr = GetComponent<Transform>();
        type = FindType();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    FigTypes FindType()
    {
        FigTypes temp = FigTypes.Cube;
        if(FigCol is BoxCollider) temp = FigTypes.Cube;
        if(FigCol is SphereCollider) temp = FigTypes.Sphere;
        return temp;
    }
}
