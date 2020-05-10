using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShapeType {Sphere, Cube, Torus, Prism};
public enum Operation {Union, Substract, Intersect, Blend}

public class RayShape : MonoBehaviour
{
    public ShapeType _shapeType = ShapeType.Sphere;
    public ShapeType shapeType
    {
        set
        {
            scaleWasChangedManually = false;
            _shapeType = value;
        }
        get
        {
            return _shapeType;
        }
    }

    public Operation operation;
    public Color color = Color.white;

    [Range(0,1)]
    public float blendStrength;

    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }

    bool scaleWasChangedManually;
    public Vector3 scale = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 Scale
    {
        set
        {
            scaleWasChangedManually = true;
            scale = value;
        }
        get
        {
            if(!scaleWasChangedManually) return GetScale();
            return scale;
        }
    }

    Vector3 GetScale()
    {
        switch(shapeType)
        {
            case ShapeType.Sphere:
                return new Vector3(0.5f, 0, 0);
            case ShapeType.Cube:
                return new Vector3(0.5f , 0.5f, 0.5f);
            case ShapeType.Torus:
                return new Vector3(0.7f , 0.3f, 0);
            case ShapeType.Prism:
                return new Vector3(0.5f , 0.5f, 0);
            
        }
        return new Vector3(0.5f , 0.5f, 0.5f);
    }
}
