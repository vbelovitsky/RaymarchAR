using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vuforia;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Effects/Raymarch (Generic)")]
public class RayScript : SceneViewFilter
{

    [SerializeField]
    private Shader _EffectShader;

    public Transform SunLight;

    private Material _EffectMaterial;
    public Material EffectMaterial
    {
        get
        {
            if (!_EffectMaterial && _EffectShader)
            {
                _EffectMaterial = new Material(_EffectShader);
                _EffectMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            return _EffectMaterial;
        }
    }
    
    private Camera _CurrentCamera;
    public Camera CurrentCamera
    {
        get
        {
            if (!_CurrentCamera)
                _CurrentCamera = GetComponent<Camera>();
            return _CurrentCamera;
        }
    }

    List<Shape> shapes = new List<Shape>();

    Vector4[] positions = new Vector4[4];
    Vector4[] colors = new Vector4[4];
    Vector4[] sizes = new Vector4[4];
    float[] types = new float[4]; // На самом деле int, но у материала нет метода SetIntArray
    float[] operations = new float[4]; // -//-

    private Matrix4x4 GetFrustumCorners(Camera cam)
    {
        float camFov = cam.fieldOfView;
        float camAspect = cam.aspect;

        Matrix4x4 frustumCorners = Matrix4x4.identity;

        float fovWHalf = camFov * 0.5f;

        float tan_fov = Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

        Vector3 toRight = Vector3.right * tan_fov * camAspect;
        Vector3 toTop = Vector3.up * tan_fov;

        Vector3 topLeft = (-Vector3.forward - toRight + toTop);
        Vector3 topRight = (-Vector3.forward + toRight + toTop);
        Vector3 bottomRight = (-Vector3.forward + toRight - toTop);
        Vector3 bottomLeft = (-Vector3.forward - toRight - toTop);

        frustumCorners.SetRow(0, topLeft);
        frustumCorners.SetRow(1, topRight);
        frustumCorners.SetRow(2, bottomRight);
        frustumCorners.SetRow(3, bottomLeft);

        return frustumCorners;
    }

    static void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material fxMaterial, int passNr)
    {
        RenderTexture.active = dest;

        fxMaterial.SetTexture("_MainTex", source);

        GL.PushMatrix();
        GL.LoadOrtho();

        fxMaterial.SetPass(passNr);

        GL.Begin(GL.QUADS);

       
        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f); // BL

        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f); // BR

        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f); // TR

        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f); // TL
    
        GL.End();
        GL.PopMatrix();
    }


    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // Определяем наблюдаемые цели AR
        DetectTrackables();

        // Если материал не задан или цели не найдены, возвращаем обычное изображение
        if (!EffectMaterial || shapes.Count == 0)
        {
            Graphics.Blit(source, destination);
            return;
        }

        // Detect trackables
        // Collect trackable data to buffer
        // Set buffer to shader
        
        CreateAndSetShapeData();
        SetMaterialSettings();

        CustomGraphicsBlit(source, destination, EffectMaterial, 0);
    }

    void CreateAndSetShapeData()
    {
        for(int i = 0; i < shapes.Count; i++)
        {
            Shape shape = shapes[i];
            positions[i] = shape.Position;
            colors[i] = new Vector3(shape.SColor.r, shape.SColor.g, shape.SColor.b);
            sizes[i] = shape.Size;
            types[i] = (int)shape.SType;
            operations[i] = (int)shape.Operation;

            // Debug.Log($"Type: {types[i]}, Operation: {operations[i]}, Position: {positions[i].ToString()}");
        }

        EffectMaterial.SetVectorArray("_positions", positions);
        EffectMaterial.SetVectorArray("_colors", colors);
        EffectMaterial.SetVectorArray("_sizes", sizes);
        EffectMaterial.SetFloatArray("_types", types);
        EffectMaterial.SetFloatArray("_operations", operations);

        EffectMaterial.SetInt("_numShapes", shapes.Count);
        
        // ShapeData[] shapeData = new ShapeData[shapes.Count];
        // for (int i = 0; i < shapes.Count; i++) {
        //     Shape s = shapes[i];
        //     Vector3 col = new Vector3 (s.SColor.r, s.SColor.g, s.SColor.b);
        //     shapeData[i] = new ShapeData () {
        //         position = s.Position,
        //         size = s.Size,
        //         color = col,
        //         sType = (int) s.SType,
        //         operation = (int) s.Operation
        //     };
        //     Debug.Log(shapeData[i].ToString());
        // }
        // ComputeBuffer shapeBuffer = new ComputeBuffer (shapeData.Length, ShapeData.GetSize());
        // shapeBuffer.SetData (shapeData);
        // EffectMaterial.SetBuffer ("shapes", shapeBuffer);
        // EffectMaterial.SetInt ("numShapes", shapeData.Length);
        //buffersToDispose.Add (shapeBuffer);
    }


    void DetectTrackables()
    {
        shapes.Clear();
        IEnumerable<TrackableBehaviour> tbs = TrackerManager.Instance.GetStateManager().GetActiveTrackableBehaviours();
        foreach (TrackableBehaviour tb in tbs)
        {
            shapes.Add(tb.GetComponentInChildren<Shape>());
        }
    }

    void SetMaterialSettings()
    {
        EffectMaterial.SetVector("_LightDir", SunLight ? SunLight.forward : Vector3.down);
        EffectMaterial.SetMatrix("_FrustumCornersES", GetFrustumCorners(CurrentCamera));
        EffectMaterial.SetMatrix("_CameraInvViewMatrix", CurrentCamera.cameraToWorldMatrix);
        EffectMaterial.SetVector("_CameraWS", CurrentCamera.transform.position);
    }

}