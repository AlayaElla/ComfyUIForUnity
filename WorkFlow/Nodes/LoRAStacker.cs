using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace ComfyUIForUnity
{
    public class LoRAStacker : Node
    {
        [JsonIgnore]
        public long LoraCount
        {
            get => (long)inputs["lora_count"];
            set => inputs["lora_count"] = value;
        }

        public class LoraInfo
        {
            public int NameIndex = 0;
            public float Weight = 1;
        }
        [JsonIgnore]
        public List<LoraInfo> LoraInfos = new List<LoraInfo>();

        [JsonIgnore]
        public int LoraNodeIndex
        {
            get;
            set;
        }

        public override void DrawGUI(AIEditorData aiEditorData, ServerInfo serverInfo, string nodekey)
        {
            base.DrawGUI(aiEditorData, serverInfo, nodekey);

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("LoRA数量", GUILayout.Width(80));
            var lora_count = aiEditorData.Load<int>(AIEditorData.ValueType.lora_count, nodekey);
            int newLoraCount = EditorGUILayout.IntSlider("", lora_count, 0, 49);
            if (newLoraCount != lora_count)
            {
                aiEditorData.Save<int>(newLoraCount, AIEditorData.ValueType.lora_count, nodekey);
            }
            LoraCount = newLoraCount;
            EditorGUILayout.EndHorizontal();

            // 根据LoraCount的值调整LoraInfos的大小
            while (LoraInfos.Count < LoraCount)
                LoraInfos.Add(new LoraInfo());
            while (LoraInfos.Count > LoraCount)
                LoraInfos.RemoveAt(LoraInfos.Count - 1);

            int lora_name_index = 0;
            float lora_weight = 0f;
            for (int i = 0; i < LoraInfos.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"  |-LoRA模型 {i + 1}", GUILayout.Width(90));
                lora_name_index = aiEditorData.Load<int>(AIEditorData.ValueType.lora_name_index, nodekey, i + 1);
                LoraInfos[i].NameIndex = EditorGUILayout.Popup(lora_name_index, serverInfo.GetLoraList(), GUILayout.MinWidth(100));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"  |-权重 {i + 1}", GUILayout.Width(90));
                lora_weight = aiEditorData.Load<float>(AIEditorData.ValueType.lora_weight, nodekey, i + 1);
                LoraInfos[i].Weight = EditorGUILayout.Slider("", lora_weight, 0, 1f, GUILayout.MinWidth(100));
                EditorGUILayout.EndHorizontal();

                if (LoraInfos[i].NameIndex != lora_name_index)
                {
                    aiEditorData.Save<int>(LoraInfos[i].NameIndex, AIEditorData.ValueType.lora_name_index, nodekey, i + 1);
                }
                if (LoraInfos[i].Weight != lora_weight)
                {
                    aiEditorData.Save<float>(LoraInfos[i].Weight, AIEditorData.ValueType.lora_weight, nodekey, i + 1);
                }

                if (LoraInfos[i].NameIndex < 0)
                    LoraInfos[i].NameIndex = 0;
                if (LoraInfos[i].Weight < 0)
                    LoraInfos[i].Weight = 1;
                inputs[$"lora_name_{i + 1}"] = serverInfo.GetLoraList()[Math.Min(LoraInfos[i].NameIndex, serverInfo.GetLoraList().Length - 1)];
                inputs[$"lora_wt_{i + 1}"] = LoraInfos[i].Weight;
            }

            EditorGUILayout.EndVertical();
        }

        public override void SetDefaultValue(AIEditorData aiEditorData, ServerInfo serverInfo, string nodekey)
        {
            //设置默认值
            if (aiEditorData.Load<int>(AIEditorData.ValueType.lora_count, nodekey) == -1)
            {
                aiEditorData.Save<int>((int)LoraCount, AIEditorData.ValueType.lora_count, nodekey);
            }

            // 根据LoraCount的值调整LoraInfos的大小
            while (LoraInfos.Count < LoraCount)
                LoraInfos.Add(new LoraInfo());
            while (LoraInfos.Count > LoraCount)
                LoraInfos.RemoveAt(LoraInfos.Count - 1);

            var lora_name_index = 0;
            for (int i = 0; i < LoraInfos.Count; i++)
            {
                lora_name_index = aiEditorData.Load<int>(AIEditorData.ValueType.lora_name_index, nodekey, i + 1);
                if (lora_name_index == -1)
                {
                    lora_name_index = serverInfo.GetLoraIndex((string)inputs[$"lora_name_{i + 1}"]);
                    aiEditorData.Save<int>(lora_name_index, AIEditorData.ValueType.lora_name_index, nodekey, i + 1);
                }
                lora_name_index = Math.Min(lora_name_index, serverInfo.GetLoraList().Length - 1);
                aiEditorData.Save<int>(lora_name_index, AIEditorData.ValueType.ckpt_name_index, nodekey);

                if (aiEditorData.Load<float>(AIEditorData.ValueType.lora_weight, nodekey, i + 1) == -1)
                {
                    float weight = 1f;
                    if (inputs[$"lora_wt_{i + 1}"].GetType() == typeof(float))
                        weight = (float)inputs[$"lora_wt_{i + 1}"];
                    else if (inputs[$"lora_wt_{i + 1}"].GetType() == typeof(double))
                        weight = (float)(double)inputs[$"lora_wt_{i + 1}"];

                    aiEditorData.Save<float>((float)weight, AIEditorData.ValueType.lora_weight, nodekey, i + 1);
                }
            }
        }
    }
}