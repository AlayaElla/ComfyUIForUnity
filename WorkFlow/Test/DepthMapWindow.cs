using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthMapWindow : EditorWindow
{
    [MenuItem("Tool/AI/���Թ���", false, 100)]
    public static void ShowWindow()
    {
        GetWindow<DepthMapWindow>("���Թ���");
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
            capturedTextures = new List<Texture2D>(); // ��ʼ���б�

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
        //�����ťɾ��ȫ��EditorPrefs
        if (GUILayout.Button("Delete All EditorPrefs"))
        {
            EditorPrefs.DeleteAll();
            Debug.Log("ɾ���������");
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        // ��ʾ�����ͼ��
        if (capturedTextures != null)
        {
            foreach (var texture in capturedTextures)
            {
                if (texture != null)
                {
                    GUILayout.Label("Captured Image:");

                    // �������ű���
                    float ratio = Mathf.Min(position.width / texture.width, position.height / texture.height);
                    int width = (int)(texture.width * ratio);
                    int height = (int)(texture.height * ratio);

                    // ��ʾ���ź��ͼ��
                    GUILayout.Label(new GUIContent(texture), GUILayout.Width(width), GUILayout.Height(height));
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }
}