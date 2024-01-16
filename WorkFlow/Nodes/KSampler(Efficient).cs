using UnityEditor;
using UnityEngine;

namespace ComfyUIForUnity
{
    public class KSampler_Efficient_ : SamplerBase
    {
        public KSampler_Efficient_() { }

        public override void DrawGUI(AIEditorData aiEditorData, ServerInfo serverInfo, string nodekey)
        {
            base.DrawGUI(aiEditorData, serverInfo, nodekey);

            EditorGUILayout.BeginVertical();
            if (!isLinkSeed)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("随机种子", GUILayout.Width(80));
                var seed = aiEditorData.Load<string>(AIEditorData.ValueType.seed, nodekey);
                string newSeed = EditorGUILayout.TextField(seed);

                if (newSeed != seed)
                {
                    aiEditorData.Save(newSeed, AIEditorData.ValueType.seed, nodekey);
                }
                EditorSeed = newSeed;
                EditorGUILayout.EndHorizontal();
            }
            if (!isLinkDenoise)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("重绘幅度", GUILayout.Width(80));
                float denoise = aiEditorData.Load<float>(AIEditorData.ValueType.denoise, nodekey);
                float newDenoise = EditorGUILayout.Slider(denoise, 0.01f, 1);

                if (newDenoise != denoise)
                {
                    aiEditorData.Save(newDenoise, AIEditorData.ValueType.denoise, nodekey);
                }
                Denoise = newDenoise;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
    }
}