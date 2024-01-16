using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;

namespace ComfyUIForUnity
{
    public class VAELoader : Node
    {
        [JsonIgnore]
        public string VaeName
        {
            get => (string)inputs["vae_name"];
            set => inputs["vae_name"] = value;
        }

        [JsonIgnore]
        private bool isLinkVae = false;

        public VAELoader() { }

        public override void DrawGUI(AIEditorData aiEditorData, ServerInfo serverInfo, string nodekey)
        {
            base.DrawGUI(aiEditorData, serverInfo, nodekey);

            EditorGUILayout.BeginVertical();
            if (!isLinkVae)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("VAE", GUILayout.Width(80));
                int vae_name_index = aiEditorData.Load<int>(AIEditorData.ValueType.vae_name_index, nodekey);
                int newVaeNameIndex = EditorGUILayout.Popup(vae_name_index, serverInfo.GetVaeList());
                if (newVaeNameIndex != vae_name_index)
                {
                    aiEditorData.Save<int>(newVaeNameIndex, AIEditorData.ValueType.vae_name_index, nodekey);
                }
                VaeName = serverInfo.GetVaeList()[Math.Min(newVaeNameIndex, serverInfo.GetVaeList().Length - 1)];
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        public override void SetDefaultValue(AIEditorData aiEditorData, ServerInfo serverInfo, string nodekey)
        {
            if (inputs["vae_name"].GetType() == typeof(JArray))
                isLinkVae = true;

            //…Ë÷√ƒ¨»œ÷µ
            if (!isLinkVae)
            {
                var vae_name_index = aiEditorData.Load<int>(AIEditorData.ValueType.vae_name_index, nodekey);
                if (vae_name_index == -1)
                {
                    vae_name_index = serverInfo.GetVaeIndex(VaeName);
                    aiEditorData.Save<int>(vae_name_index, AIEditorData.ValueType.vae_name_index, nodekey);
                }
                vae_name_index = Math.Min(vae_name_index, serverInfo.GetVaeList().Length - 1);
                aiEditorData.Save<int>(vae_name_index, AIEditorData.ValueType.vae_name_index, nodekey);
            }
        }
    }
}