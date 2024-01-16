using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ComfyUIForUnity
{
    public class EmptyLatentImage : Node
    {
        [JsonIgnore]
        public long Width
        {
            get => (long)inputs["width"];
            set => inputs["width"] = value;
        }

        [JsonIgnore]
        public long Height
        {
            get => (long)inputs["height"];
            set => inputs["height"] = value;
        }

        [JsonIgnore]
        public long BatchSize
        {
            get => (long)inputs["batch_size"];
            set => inputs["batch_size"] = value;
        }

        [JsonIgnore]
        private bool isLinkSize = false;
        [JsonIgnore]
        private bool isLinkBatchSize = false;
        
        public EmptyLatentImage()
        {
        }

        public override void DrawGUI(AIEditorData aiEditorData, ServerInfo serverInfo, string nodekey)
        {
            base.DrawGUI(aiEditorData, serverInfo, nodekey);
            EditorGUILayout.BeginVertical();

            if (!isLinkSize)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("长度 × 高度", GUILayout.Width(80));
                var width = aiEditorData.Load<long>(AIEditorData.ValueType.width, nodekey);
                long newWidth = EditorGUILayout.LongField(width);
                if (newWidth != width)
                {
                    aiEditorData.Save<long>(newWidth, AIEditorData.ValueType.width, nodekey);
                }
                var height = aiEditorData.Load<long>(AIEditorData.ValueType.height, nodekey);
                long newHeight = EditorGUILayout.LongField(height);
                if (newHeight != height)
                {
                    aiEditorData.Save<long>(newHeight, AIEditorData.ValueType.height, nodekey);
                }
                EditorGUILayout.EndHorizontal();

                Width = newWidth;
                Height = newHeight;
            }

            if (!isLinkBatchSize)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("批次", GUILayout.Width(80));
                var batch = aiEditorData.Load<long>(AIEditorData.ValueType.batch, nodekey);
                long newAmount = EditorGUILayout.LongField(batch);
                if (newAmount != batch)
                {
                    aiEditorData.Save(newAmount, AIEditorData.ValueType.batch, nodekey);
                }

                BatchSize = newAmount;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        public override void SetDefaultValue(AIEditorData aiEditorData, ServerInfo serverInfo, string nodekey)
        {
            if (inputs["width"].GetType() == typeof(JArray))
                isLinkSize = true;
            if (inputs["height"].GetType() == typeof(JArray))
                isLinkSize = true;
            if (inputs["batch_size"].GetType() == typeof(JArray))
                isLinkBatchSize = true;

            //设置默认值
            //size
            if (!isLinkSize)
            {
                if (aiEditorData.Load<long>(AIEditorData.ValueType.width, nodekey) == -1)
                {
                    aiEditorData.Save<long>(Width, AIEditorData.ValueType.width, nodekey);
                }

                if (aiEditorData.Load<long>(AIEditorData.ValueType.height, nodekey) == -1)
                {
                    aiEditorData.Save<long>(Height, AIEditorData.ValueType.height, nodekey);
                }
            }

            //batch
            if (!isLinkBatchSize)
            {
                if (aiEditorData.Load<long>(AIEditorData.ValueType.batch, nodekey) == -1)
                {
                    aiEditorData.Save(BatchSize, AIEditorData.ValueType.batch, nodekey);
                }
            }
        }
    }
}