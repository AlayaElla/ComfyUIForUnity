using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ComfyUIForUnity
{
    public class CLIPTextEncode : PromptBase
    {
        [JsonIgnore]
        public string Text
        {
            get => (string)inputs["text"];
            set => inputs["text"] = value;
        }
        [JsonIgnore]
        public int PromptIndex
        {
            get;
            set;
        }

        [JsonIgnore]
        private bool isLinkPrompt = false;

        public CLIPTextEncode() { }

        public override void DrawGUI(AIEditorData aiEditorData, ServerInfo serverInfo, string nodekey)
        {
            base.DrawGUI(aiEditorData, serverInfo, nodekey);
            EditorGUILayout.BeginVertical();
            if (!isLinkPrompt)
            {
                PromptIndex = 0;
                EditorGUILayout.BeginVertical();
                GUILayout.Label("提示词");
                GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
                textAreaStyle.wordWrap = true;

                var prompt = aiEditorData.Load<string>(AIEditorData.ValueType.prompt, nodekey, PromptIndex);
                string newPrompt = EditorGUILayout.TextArea(prompt,
                    textAreaStyle,
                    GUILayout.Height(70),
                    GUILayout.ExpandWidth(true));
                if (newPrompt != prompt)
                {
                    aiEditorData.Save(newPrompt, AIEditorData.ValueType.prompt, nodekey, PromptIndex);
                }

                Text = newPrompt;
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        public override void SetDefaultValue(AIEditorData aiEditorData, ServerInfo serverInfo, string nodekey)
        {
            if (inputs["text"].GetType() == typeof(JArray))
                isLinkPrompt = true;

            if (!isLinkPrompt)
            {
                PromptIndex = 0;
                //设置默认值
                if (aiEditorData.Load<string>(AIEditorData.ValueType.prompt, nodekey, PromptIndex) == "")
                {
                    aiEditorData.Save(Text, AIEditorData.ValueType.prompt, nodekey, PromptIndex);
                }
            }
        }
    }
}