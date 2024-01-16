using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SaveDepthRendererFeature : ScriptableRendererFeature
{
    public float m_Intensity = 10.0f;
    public int saveFrequency = 15;

    public SaveType saveType = SaveType.Depth;

    public ComputeShader m_computeShader;
    public bool autoOptimize = true;

    public static bool isDebug = false;
    public bool _isDebug = false;

    public enum SaveType
    {
        Depth,
        Color
    }

    Material m_Material;
    RenderTexture m_Texture;

    SaveDepthBlitPass m_RenderPass = null;

#if UNITY_2022_1_OR_NEWER
    RTHandle m_TargetHandle;
#else
    RenderTargetIdentifier m_TargetHandle { get; set; }
#endif

    public override void Create()
    {
        m_computeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/ComfyUIForUnity/Render/Shader/AverageBrightness.compute");
        if (saveType == SaveType.Depth)
        {
#if UNITY_2022_1_OR_NEWER
            m_Material = CoreUtils.CreateEngineMaterial(Shader.Find("Shader Graphs/getdepthShader"));
#else
            Shader shader = Shader.Find("Shader Graphs/getdepthShaderURP12.1");
            m_Material = CoreUtils.CreateEngineMaterial(shader);
#endif
        }
        else if(saveType == SaveType.Color)
        {
            Shader shader = Shader.Find("Custom/URPLinearToSRGB");
            m_Material = CoreUtils.CreateEngineMaterial(shader);
            //m_Material = null;
        }
        if (m_Texture == null)
            m_Texture = CreateRTexure();
        m_RenderPass = new SaveDepthBlitPass(this, m_Material, m_Texture);
    }

    public RenderTexture CreateRTexure()
    {
        m_Texture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat);
        return m_Texture;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {

#if UNITY_2022_1_OR_NEWER
            renderer.EnqueuePass(m_RenderPass);
#else
            renderer.EnqueuePass(m_RenderPass);

            if (saveType == SaveType.Depth)
            {
                m_RenderPass.ConfigureInput(ScriptableRenderPassInput.Depth);
                m_RenderPass.SetTarget(renderer.cameraColorTarget, 1f);
            }
            else if (saveType == SaveType.Color)
            {
                m_RenderPass.ConfigureInput(ScriptableRenderPassInput.Color);
                m_RenderPass.SetTarget(renderer.cameraColorTarget, 1f);
            }

#endif
        }
    }

#if UNITY_2022_1_OR_NEWER
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {

            if (saveType == SaveType.Depth)
            {
                m_RenderPass.ConfigureInput(ScriptableRenderPassInput.Depth);
                m_RenderPass.SetTarget(renderer.cameraDepthTargetHandle);
            }
            else if (saveType == SaveType.Color)
            {
                m_RenderPass.ConfigureInput(ScriptableRenderPassInput.Color);
                m_RenderPass.SetTarget(renderer.cameraColorTargetHandle);
            }
        }
    }
#else
#endif
    Texture2D texture2D;
    public Texture2D CaptureTexture()
    {
        if(m_Texture == null)
        {
            Debug.LogError("没有获取到渲染的图片");
            return null;
        }
        //Debug.Log("开始读取图片");      
        texture2D = new Texture2D(m_Texture.width, m_Texture.height, TextureFormat.RGBA32, false, false);
        RenderTexture.active = m_Texture;
        texture2D.ReadPixels(new Rect(0, 0, m_Texture.width, m_Texture.height), 0, 0);
        texture2D.Apply();
        RenderTexture.active = null;

        return texture2D;    
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(m_Material);
    }
}