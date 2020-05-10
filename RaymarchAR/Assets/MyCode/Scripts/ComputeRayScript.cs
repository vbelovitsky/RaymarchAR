using UnityEngine;
using System.Collections.Generic;
using Vuforia;

/// <summary>
/// Класс, отвечающий за инициализацию шейдера
/// </summary>
[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
public class ComputeRayScript : MonoBehaviour
{
    /// <summary>
    /// Вычислительный шейдер
    /// </summary>
    public ComputeShader raymarchShader;

    /// <summary>
    /// Число для подсчета количества групп потоков для шейдера
    /// </summary>
    float threadDelimeter = 8.0f;

    /// <summary>
    /// Целевая текстура, отображаемая после работы шейдера
    /// </summary>
    RenderTexture target;

    /// <summary>
    /// Основная камера
    /// </summary>
    public new Camera camera;

    /// <summary>
    /// Источник света
    /// </summary>
    public Light lightSource;

    /// <summary>
    /// Ссылка для хранения и высвобождения вычислительного буфера
    /// </summary>
    /// <typeparam name="ComputeBuffer"></typeparam>
    /// <returns></returns>
    ComputeBuffer disposeBuffer;

    /// <summary>
    /// Список форм, отслеживаемых в текущем кадре
    /// </summary>
    /// <typeparam name="RayShape">Объект формы</typeparam>
    List<RayShape> shapes = new List<RayShape>();

    /// <summary>
    /// Устанавливает разрешение экрана при загрузке скрипта
    /// </summary>
    void Awake()
    {
        Screen.SetResolution(240, 135, true);
    }

    /// <summary>
    /// Отпределяет наблюдаемые формы
    /// </summary>
    void DetectTrackables()
    {
        shapes.Clear();
        IEnumerable<TrackableBehaviour> tbs = TrackerManager.Instance.GetStateManager().GetActiveTrackableBehaviours();
        foreach (TrackableBehaviour tb in tbs)
        {
            shapes.Add(tb.GetComponentInChildren<RayShape>());
        }
    }

    /// <summary>
    /// Выполняется при рендеринге изображения:
    /// если на экране присутствуют наблюдаемые формы,
    /// исходное изображение изменяется шейдером.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        DetectTrackables();
        if (shapes.Count != 0)
        {
            InitTargetTexture();
            SetCameraAndLightParameters();
            FillBufferShapes();
            raymarchShader.SetTexture(0, "Source", source);
            raymarchShader.SetTexture(0, "Destination", target);
            int threadGroupsX = Mathf.CeilToInt(camera.pixelWidth / threadDelimeter);
            int threadGroupsY = Mathf.CeilToInt(camera.pixelHeight / threadDelimeter);
            raymarchShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
            Graphics.Blit(target, destination);
            disposeBuffer.Dispose();
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    /// <summary>
    /// Наполняет буфер формами
    /// </summary>
    void FillBufferShapes()
    {
        shapes.Sort((a, b) => a.operation.CompareTo(b.operation));

        ShapeData[] shapeData = new ShapeData[shapes.Count];
        for (int i = 0; i < shapes.Count; i++)
        {
            var s = shapes[i];
            Vector3 col = new Vector3(s.color.r, s.color.g, s.color.b);
            shapeData[i] = new ShapeData()
            {
                position = s.Position,
                scale = s.Scale,
                color = col,
                shapeType = (int)s.shapeType,
                operation = (int)s.operation,
                blendStrength = s.blendStrength * 3,
            };
        }

        ComputeBuffer shapeBuffer = new ComputeBuffer(shapeData.Length, ShapeData.GetSize());
        shapeBuffer.SetData(shapeData);
        raymarchShader.SetBuffer(0, "shapes", shapeBuffer);
        raymarchShader.SetInt("shapesNumber", shapeData.Length);

        disposeBuffer = shapeBuffer;
    }

    /// <summary>
    /// Устанавливает параметры камеры и источника света в шейдер
    /// </summary>
    void SetCameraAndLightParameters()
    {
        raymarchShader.SetMatrix("_CameraToWorld", camera.cameraToWorldMatrix);
        raymarchShader.SetMatrix("_CameraInverseProjection", camera.projectionMatrix.inverse);
        raymarchShader.SetVector("_Light", lightSource.transform.forward);
    }

    /// <summary>
    /// Инициализирует целевую текстуру
    /// </summary>
    void InitTargetTexture()
    {
        if (target != null)
        {
            target.Release();
        }
        target = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        target.enableRandomWrite = true;
        target.Create();
    }

    /// <summary>
    /// Структура, хранящая данные формы для шейдера
    /// </summary>
    struct ShapeData
    {
        public Vector3 position;
        public Vector3 scale;
        public Vector3 color;
        public int shapeType;
        public int operation;
        public float blendStrength;

        /// <summary>
        /// Возвращает размер данных формы для шейдера
        /// </summary>
        public static int GetSize()
        {
            return sizeof(float) * 10 + sizeof(int) * 2;
        }
    }
}
