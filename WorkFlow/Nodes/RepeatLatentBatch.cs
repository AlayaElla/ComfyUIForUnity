using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ComfyUIForUnity
{
    public class RepeatLatentBatch : Node
    {
        [JsonIgnore]
        public long BatchSize
        {
            get => (long)inputs["amount"];
            set => inputs["amount"] = value;
        }

        [JsonIgnore]
        private bool isLinkBatchSize = false;

        public RepeatLatentBatch()
        {
        }

        public override void DrawGUI(AIEditorData aiEditorData, ServerInfo serverInfo, string nodekey)
        {
            base.DrawGUI(aiEditorData, serverInfo, nodekey);

            EditorGUILayout.BeginVertical();
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
            if (inputs["amount"].GetType() == typeof(JArray))
                isLinkBatchSize = true;

            //设置默认值
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