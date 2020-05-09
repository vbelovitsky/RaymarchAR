using UnityEngine;
using System.Collections.Generic;
using Vuforia;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
public class ComputeRayScript : MonoBehaviour
{
    public ComputeShader raymarching;

    RenderTexture target;
    public Camera cam;
    public Light lightSource;
    List<ComputeBuffer> buffersToDispose = new List<ComputeBuffer>();

    List<RayShape> shapes = new List<RayShape>();

    void Awake()
    {
        Screen.SetResolution(240, 135, true);
    }
    
    // void Init ()
    // {
    //     cam = Camera.current;
    //     lightSource = FindObjectOfType<Light> ();
    // }

    void DetectTrackables()
    {
        shapes.Clear();
        IEnumerable<TrackableBehaviour> tbs = TrackerManager.Instance.GetStateManager().GetActiveTrackableBehaviours();
        foreach (TrackableBehaviour tb in tbs)
        {
            shapes.Add(tb.GetComponentInChildren<RayShape>());
        }
    }

    void OnRenderImage (RenderTexture source, RenderTexture destination)
    {
        DetectTrackables();
        if(shapes.Count != 0)
        {
            //Init();
            buffersToDispose.Clear();
            InitRenderTexture ();
            SetParameters ();
            CreateScene();
            raymarching.SetTexture (0, "Source", source);
            raymarching.SetTexture (0, "Destination", target);
            int threadGroupsX = Mathf.CeilToInt(cam.pixelWidth / 32.0f);
            int threadGroupsY = Mathf.CeilToInt(cam.pixelHeight / 32.0f);
            raymarching.Dispatch(0, threadGroupsX, threadGroupsY, 1);
            Graphics.Blit(target, destination);
            DisposeBuffers();
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    void DisposeBuffers()
    {
        foreach (var buffer in buffersToDispose) {
            buffer.Dispose ();
        }
    }

    void CreateScene ()
    {
        shapes.Sort ((a, b) => a.operation.CompareTo(b.operation));

        ShapeData[] shapeData = new ShapeData[shapes.Count];
        for (int i = 0; i < shapes.Count; i++) {
            var s = shapes[i];
            Vector3 col = new Vector3 (s.color.r, s.color.g, s.color.b);
            shapeData[i] = new ShapeData () {
                position = s.Position,
                scale = s.Scale, color = col,
                shapeType = (int) s.shapeType,
                operation = (int) s.operation,
                blendStrength = s.blendStrength * 3,
                numChildren = s.numChildren
            };
        }

        ComputeBuffer shapeBuffer = new ComputeBuffer (shapeData.Length, ShapeData.GetSize());
        shapeBuffer.SetData (shapeData);
        raymarching.SetBuffer(0, "shapes", shapeBuffer);
        raymarching.SetInt ("numShapes", shapeData.Length);

        buffersToDispose.Add (shapeBuffer);
    }

    void SetParameters ()
    {
        bool lightIsDirectional = lightSource.type == LightType.Directional;
        raymarching.SetMatrix ("_CameraToWorld", cam.cameraToWorldMatrix);
        raymarching.SetMatrix ("_CameraInverseProjection", cam.projectionMatrix.inverse);
        raymarching.SetVector ("_Light", (lightIsDirectional) ? lightSource.transform.forward : lightSource.transform.position);
        raymarching.SetBool ("positionLight", !lightIsDirectional);
    }

    void InitRenderTexture ()
    {
        if (target == null || target.width != cam.pixelWidth || target.height != cam.pixelHeight) {
            if (target != null) {
                target.Release ();
            }
            target = new RenderTexture (cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create ();
        }
    }

    struct ShapeData
    {
        public Vector3 position;
        public Vector3 scale;
        public Vector3 color;
        public int shapeType;
        public int operation;
        public float blendStrength;
        public int numChildren;

        public static int GetSize ()
        {
            return sizeof (float) * 10 + sizeof (int) * 3;
        }
    }
}
