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
        saveDepthRendererFeature.saveType = (SaveDepthRendererFeature.SaveType)EditorGUILayout.EnumPopup("图片类型", saveDepthRendererFeature.saveType);
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
            saveDepthRendererFeature.m_Intensity = EditorGUILayout.Slider("深度图强度", saveDepthRendererFeature.m_Intensity, 1f, 200.0f);
        }

        //创建空行
        EditorGUILayout.Space();
        saveDepthRendererFeature.saveFrequency = EditorGUILayout.IntSlider("同步频率", saveDepthRendererFeature.saveFrequency, 1, 60);
        saveDepthRendererFeature.autoOptimize = EditorGUILayout.Toggle("优化深度图", saveDepthRendererFeature.autoOptimize);
        saveDepthRendererFeature._isDebug = EditorGUILayout.Toggle("调试", saveDepthRendererFeature._isDebug);
        SaveDepthRendererFeature.isDebug = saveDepthRendererFeature._isDebug;

        serializedObject.ApplyModifiedProperties();

        //如果是修改了saveDepthRendererFeature.saveType，则需要重新启动SaveDepthRendererFeature
        if (lastSaveType != saveDepthRendererFeature.saveType)
        {
            lastSaveType = saveDepthRendererFeature.saveType;
            saveDepthRendererFeature.Create();
        }
    }
}
