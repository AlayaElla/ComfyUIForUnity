using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEditor.Rendering.Universal;

[CustomEditor(typeof(SaveDepthRendererFeature))]
public class SaveDepthRendererFeatureEditor : ScriptableRendererFeatureEditor
{
    float pre_Intensity;
    bool restore_Intensity = false;
    SaveDepthRendererFeature.SaveType lastSaveType;

    void OnEnable()
    {
    }

    public override void OnInspectorGUI()
    {
        SaveDepthRendererFeature saveDepthRendererFeature = (SaveDepthRendererFeature)target;
        serializedObject.Update();

        saveDepthRendererFeature.m_computeShader = (ComputeShader)EditorGUILayout.ObjectField("ComputeShader", saveDepthRendererFeature.m_computeShader, typeof(ComputeShader), false);
        saveDepthRendererFeature.saveType = (SaveDepthRendererFeature.SaveType)EditorGUILayout.EnumPopup("ͼƬ����", saveDepthRendererFeature.saveType);
        if (saveDepthRendererFeature.autoOptimize)
        {
            if (saveDepthRendererFeature.m_Intensity != 1)
                pre_Intensity = saveDepthRendererFeature.m_Intensity;

            saveDepthRendererFeature.m_Intensity = 1;
            restore_Intensity = true;
        }
        else
        {
            if (restore_Intensity)
            {
                saveDepthRendererFeature.m_Intensity = pre_Intensity;
                restore_Intensity = false;
            }
            saveDepthRendererFeature.m_Intensity = EditorGUILayout.Slider("���ͼǿ��", saveDepthRendererFeature.m_Intensity, 1f, 200.0f);
        }

        //��������
        EditorGUILayout.Space();
        saveDepthRendererFeature.saveFrequency = EditorGUILayout.IntSlider("ͬ��Ƶ��", saveDepthRendererFeature.saveFrequency, 1, 60);
        saveDepthRendererFeature.autoOptimize = EditorGUILayout.Toggle("�Ż����ͼ", saveDepthRendererFeature.autoOptimize);
        saveDepthRendererFeature._isDebug = EditorGUILayout.Toggle("����", saveDepthRendererFeature._isDebug);
        SaveDepthRendererFeature.isDebug = saveDepthRendererFeature._isDebug;

        serializedObject.ApplyModifiedProperties();

        //������޸���saveDepthRendererFeature.saveType������Ҫ��������SaveDepthRendererFeature
        if (lastSaveType != saveDepthRendererFeature.saveType)
        {
            lastSaveType = saveDepthRendererFeature.saveType;
            saveDepthRendererFeature.Create();
        }
    }
}
