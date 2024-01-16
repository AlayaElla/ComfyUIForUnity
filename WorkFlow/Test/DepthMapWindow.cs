using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthMapWindow : EditorWindow
{
    [MenuItem("Tool/AI/测试工具", false, 100)]
    public static void ShowWindow()
    {
        GetWindow<DepthMapWindow>("测试工具");
    }

    List<SaveDepthRendererFeature> savefeatures;
    List<Texture2D> capturedTextures;
    private Vector2 scrollPosition;

    void OnGUI()
    {
        if (savefeatures == null)
        {
            savefeatures = new List<SaveDepthRendererFeature>();
            var renderer = (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset).GetRenderer(0);
            var property = typeof(ScriptableRenderer).GetProperty("rendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);

            List<ScriptableRendererFeature> features = property.GetValue(renderer) as List<ScriptableRendererFeature>;

            foreach (var feature in features)
            {
                if (feature is SaveDepthRendererFeature && feature.isActive)
                {
                    savefeatures.Add(feature as SaveDepthRendererFeature);
                }
            }
        }

        if (GUILayout.Button("Capture Depth and Color"))
        {
            capturedTextures = new List<Texture2D>(); // 初始化列表

            foreach (var feature in savefeatures)
            {
                if (feature.isActive)
                {
                    Texture2D texture = feature.CaptureTexture();
                    if (texture != null)
                    {
                        capturedTextures.Add(texture);
                    }
                }
            }
        }
        //点击按钮删除全部EditorPrefs
        if (GUILayout.Button("Delete All EditorPrefs"))
        {
            EditorPrefs.DeleteAll();
            Debug.Log("删除数据完成");
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        // 显示捕获的图像
        if (capturedTextures != null)
        {
            foreach (var texture in capturedTextures)
            {
                if (texture != null)
                {
                    GUILayout.Label("Captured Image:");

                    // 计算缩放比例
                    float ratio = Mathf.Min(position.width / texture.width, position.height / texture.height);
                    int width = (int)(texture.width * ratio);
                    int height = (int)(texture.height * ratio);

                    // 显示缩放后的图像
                    GUILayout.Label(new GUIContent(texture), GUILayout.Width(width), GUILayout.Height(height));
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }
}