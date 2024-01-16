using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.IO;
using System;

public class SaveDepthBlitPass : ScriptableRenderPass
{
    ProfilingSampler m_ProfilingSampler;
    SaveDepthRendererFeature feature;
#if UNITY_2022_1_OR_NEWER
    RTHandle m_Destination;
    RTHandle m_Source;
#else
    RenderTargetIdentifier m_Source { get; set; }
    RenderTargetIdentifier m_Destination { get; set; }
#endif

    float temp_Intensity = 1;
    Material m_Material;
    RenderTexture m_Texture;
    int frameCount;

    public SaveDepthBlitPass(SaveDepthRendererFeature _feature, Material _material, RenderTexture _texture)
    {
        frameCount = 0;
        feature = _feature;
        m_Material = _material;
        m_Texture = _texture;
        m_ProfilingSampler = new ProfilingSampler($"{feature.name}");

        renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

#if UNITY_2022_1_OR_NEWER
    public void SetTarget(RTHandle Source)
    {
        m_Source = Source;
    }
#else

    public void SetTarget(RenderTargetIdentifier Source, float intensity)
    {
        m_Source = Source;
    }
#endif

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        if (!renderingData.cameraData.camera.CompareTag("MainCamera"))
            return;
        if (m_Texture == null || m_Texture.width != Screen.width || m_Texture.height != Screen.height)
        {
            m_Texture = feature.CreateRTexure();
            Debug.Log("重新创建RT");
        }

        //根据上一帧的结果调整亮度
        if (frameCount % feature.saveFrequency == 0 && m_Texture)
        {
            if(feature.saveType == SaveDepthRendererFeature.SaveType.Depth)
            {
                if (feature.autoOptimize)
                {
                    temp_Intensity = temp_Intensity * GetOptimizeBright(m_Texture);
                    m_Material.SetFloat("_Intensity", temp_Intensity);
                }
                else
                {
                    m_Material.SetFloat("_Intensity", feature.m_Intensity);
                }
                m_Material.SetPass(0);
            }
        }

#if UNITY_2022_1_OR_NEWER
        if (m_Destination == null)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            m_Destination = RTHandles.Alloc(desc, name: "_SaveDepthBlitPassHandle");
        }
        ConfigureTarget(m_Destination);
#else
        m_Destination = new RenderTargetIdentifier(m_Texture);
        ConfigureTarget(m_Destination);
#endif
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cameraData = renderingData.cameraData;

        //if (cameraData.camera.cameraType != CameraType.Game)
        //    return;
        //if (cameraData.renderType == CameraRenderType.Overlay)
        //    return;

        if (!cameraData.camera.CompareTag("MainCamera"))
            return;

        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, m_ProfilingSampler))
        {
#if UNITY_2022_1_OR_NEWER
            if (m_Material != null)
                Blitter.BlitCameraTexture(cmd, m_Source, m_Destination, m_Material, 0);
            else
                Blitter.BlitCameraTexture(cmd, m_Source, m_Destination);
            cmd.Blit(m_Destination, m_Texture);
#else
            Blit(cmd, m_Source, m_Destination, m_Material, 0);
            cmd.Blit(m_Destination, m_Texture);
#endif
            if (frameCount % feature.saveFrequency == 0)
            {
                //SendImg(m_Texture);
                frameCount = 0;
            }
        }
        frameCount++;

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        CommandBufferPool.Release(cmd);
    }

    //void SaveImg(RenderTexture renderTexture)
    //{
    //    string savePath = "E:/Out";
    //    string depthPath = "/Depth.png";
    //    string sourcePath = "/Source.png";
    //    float size = 0;

    //    tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RFloat, false);
    //    RenderTexture.active = renderTexture;
    //    tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

    //    Color[] pixels = tex.GetPixels();
    //    for (int i = 0; i < pixels.Length; i++)
    //    {
    //        float depthValue = pixels[i].r;
    //        pixels[i] = new Color(depthValue, depthValue, depthValue);
    //        size += depthValue;
    //    }

    //    tex.SetPixels(pixels);
    //    tex.Apply();
    //    byte[] bytes = tex.EncodeToPNG();

    //    if (!Directory.Exists(savePath))
    //    {
    //        Directory.CreateDirectory(savePath);
    //    }

    //    if (saveType == SaveDepthRendererFeature.SaveType.Depth)
    //    {
    //        File.WriteAllBytes(savePath + depthPath, bytes);
    //    }
    //    else if (saveType == SaveDepthRendererFeature.SaveType.Color)
    //    {
    //        ScreenCapture.CaptureScreenshot(savePath + sourcePath, 0);
    //    }
    //    if (SaveDepthRendererFeature.isDebug)
    //        Debug.Log($"SaveDepth: {(float)size}");
    //}

    Texture2D tex;
    string imgBase64 = "";
    string imgBase64Old = "";
    void SendImg(RenderTexture renderTexture)
    {
        if (feature.saveType == SaveDepthRendererFeature.SaveType.Depth)
        {
            imgBase64 = ConvertRenderTextureToBase64(renderTexture, TextureFormat.RFloat);
        }
        else if (feature.saveType == SaveDepthRendererFeature.SaveType.Color)
        {
            imgBase64 = ConvertRenderTextureToBase64(renderTexture, TextureFormat.RGB24);
        }

        if (imgBase64 == imgBase64Old)
        {
            return;
        }

        //WebSocketClientManager.Instance.SendWebSocketMessage("data:image/png;base64," + imgBase64);
        //feature.wsClient.SendWebSocketMessage("data:image/png;base64," + imgBase64);
        imgBase64Old = imgBase64;
    }

    string ConvertRenderTextureToBase64(RenderTexture renderTexture, TextureFormat textureFormat)
    {
        tex = new Texture2D(renderTexture.width, renderTexture.height, textureFormat, false);
        RenderTexture.active = renderTexture;
        tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        RenderTexture.active = null;

        return Convert.ToBase64String(tex.EncodeToPNG());
    }

    float GetOptimizeBright(RenderTexture renderTexture)
    {
        float targetBrightness = 0.3f;
        float averageBrightness = CalculateAverageBrightnessWithGPU(renderTexture);
        if (averageBrightness == 0)
        {
            return 1;
        }
        float brightnessAdjustment = targetBrightness / averageBrightness;
        return brightnessAdjustment;
    }

    float CalculateAverageBrightnessWithGPU(RenderTexture renderTexture)
    {
        int pixelCount = renderTexture.width * renderTexture.height;
        uint[] data = new uint[1];

        int kernelIndex = feature.m_computeShader.FindKernel("SumColor");
        feature.m_computeShader.SetTexture(kernelIndex, "_InputTexture", renderTexture);
        ComputeBuffer buffer = new ComputeBuffer(1, sizeof(uint));
        buffer.SetData(new uint[1] { 0 });
        feature.m_computeShader.SetBuffer(kernelIndex, "_OutputBuffer", buffer);

        feature.m_computeShader.Dispatch(kernelIndex,
            Mathf.CeilToInt(renderTexture.width / 8),
            Mathf.CeilToInt(renderTexture.height / 8),
            1);

        buffer.GetData(data);
        buffer.Release();
        return data[0] / 1000f / pixelCount;
    }
}