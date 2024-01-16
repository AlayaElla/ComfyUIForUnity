using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;

namespace ComfyUIForUnity
{
    public class CheckpointLoaderSimple : Node
    {
        [JsonIgnore]
        public string CkptName
        {
            get => (string)inputs["ckpt_name"];
            set => inputs["ckpt_name"] = value;
        }

        [JsonIgnore]
        private bool isLinkCkpt = false;

        public CheckpointLoaderSimple() { }

        public override void DrawGUI(AIEditorData aiEditorData, ServerInfo serverInfo, string nodekey)
        {
            base.DrawGUI(aiEditorData, serverInfo, nodekey);
            EditorGUILayout.BeginVertical();
            if (!isLinkCkpt)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("模型", GUILayout.Width(80));
                var ckpt_name_index = aiEditorData.Load<int>(AIEditorData.ValueType.ckpt_name_index, nodekey);
                int newCkptNameIndex = EditorGUILayout.Popup(ckpt_name_index, serverInfo.GetCkptList());
                if (newCkptNameIndex != ckpt_name_index)
                {
                    aiEditorData.Save<int>(newCkptNameIndex, AIEditorData.ValueType.ckpt_name_index, nodekey);
                }

                CkptName = serverInfo.GetCkptList()[newCkptNameIndex];
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        public override void SetDefaultValue(AIEditorData aiEditorData, ServerInfo serverInfo, string nodekey)
        {
            if (inputs["ckpt_name"].GetType() == typeof(JArray))
                isLinkCkpt = true;

            //设置默认值
            if (!isLinkCkpt)
            {
                var ckpt_name_index = aiEditorData.Load<int>(AIEditorData.ValueType.ckpt_name_index, nodekey);
                if (ckpt_name_index == -1)
                {
                    ckpt_name_index = serverInfo.GetCkptIndex(CkptName);
                    aiEditorData.Save<int>(ckpt_name_index, AIEditorData.ValueType.ckpt_name_index, nodekey);
                }
                ckpt_name_index = Math.Min(ckpt_name_index, serverInfo.GetCkptList().Length - 1);
                aiEditorData.Save<int>(ckpt_name_index, AIEditorData.ValueType.ckpt_name_index, nodekey);
            }
        }
    }
}