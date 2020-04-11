using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vuforia;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Effects/Raymarch (Generic)")]
public class RayScript : SceneViewFilter {

    [SerializeField]
    private Shader _EffectShader;

    public Transform SunLight;

    public ARController _Controller;

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
    
    GameObject[] Targets;

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
        GL.LoadOrtho(); // Note: z value of vertices don't make a difference because we are using ortho projection

        fxMaterial.SetPass(passNr);

        GL.Begin(GL.QUADS);

        // Here, GL.MultitexCoord2(0, x, y) assigns the value (x, y) to the TEXCOORD0 slot in the shader.
        // GL.Vertex3(x,y,z) queues up a vertex at position (x, y, z) to be drawn.  Note that we are storing
        // our own custom frustum information in the z coordinate.
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
        if (!EffectMaterial)
        {
            Graphics.Blit(source, destination); // do nothing
            return;
        }
        
        List<FigureData> data = new List<FigureData>();
        IEnumerable<TrackableBehaviour> tbs = TrackerManager.Instance.GetStateManager().GetActiveTrackableBehaviours();
        foreach (TrackableBehaviour tb in tbs)
        {
            data.Add(tb.GetComponentInChildren<FigureData>());
        }
        
        EffectMaterial.SetVector("_LightDir", SunLight ? SunLight.forward : Vector3.down);
        EffectMaterial.SetMatrix("_FrustumCornersES", GetFrustumCorners(CurrentCamera));
        EffectMaterial.SetMatrix("_CameraInvViewMatrix", CurrentCamera.cameraToWorldMatrix);
        EffectMaterial.SetVector("_CameraWS", CurrentCamera.transform.position);

        SetMaterialFigureData(data);
        
        CustomGraphicsBlit(source, destination, EffectMaterial, 0);
    }

    void SetMaterialFigureData(List<FigureData> data)
    {
        Vector4 defaultCubeData = new Vector4(-1, 5, 4, 0.5f);
        Vector4 defaultSphereData = new Vector4(1, 5, 4, 0.5f);

        if(data.Count == 2)
        {
            Vector4 sphereData = data[0].FigType == FigTypes.Sphere ? data[0].PositionAndSide : data[1].PositionAndSide;
            Vector4 cubeData = data[0].FigType == FigTypes.Cube ? data[0].PositionAndSide : data[1].PositionAndSide;
            EffectMaterial.SetVector("_Object1", cubeData);
            EffectMaterial.SetVector("_Object2", sphereData);
        }
        else if(data.Count == 1)
        {
            Vector4 figData = data[0].PositionAndSide;
            if(data[0].FigType == FigTypes.Sphere)
            {
                EffectMaterial.SetVector("_Object1", figData);
                EffectMaterial.SetVector("_Object2", defaultSphereData);
            }
            else
            {
                EffectMaterial.SetVector("_Object1", defaultCubeData);
                EffectMaterial.SetVector("_Object2", figData);
            }
        }
        else
        {
            EffectMaterial.SetVector("_Object1", defaultCubeData);
            EffectMaterial.SetVector("_Object1", defaultSphereData);
        }
    }
}