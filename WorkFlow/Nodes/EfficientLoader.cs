using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;

namespace ComfyUIForUnity
{
    public class EfficientLoader : PromptBase
    {
        [JsonIgnore]
        public string CkptName
        {
            get => (string)inputs["ckpt_name"];
            set => inputs["ckpt_name"] = value;
        }

        [JsonIgnore]
        public string VaeName
        {
            get => (string)inputs["vae_name"];
            set => inputs["vae_name"] = value;
        }

        [JsonIgnore]
        public string Positive
        {
            get => (string)inputs["positive"];
            set => inputs["positive"] = value;
        }

        [JsonIgnore]
        public string Negative
        {
            get => (string)inputs["negative"];
            set => inputs["negative"] = value;
        }

        [JsonIgnore]
        public long Width
        {
            get => (long)inputs["empty_latent_width"];
            set => inputs["empty_latent_width"] = value;
        }

        [JsonIgnore]
        public long Height
        {
            get => (long)inputs["empty_latent_height"];
            set => inputs["empty_latent_height"] = value;
        }

        [JsonIgnore]
        public long BatchSize
        {
            get => (long)inputs["batch_size"];
            set => inputs["batch_size"] = value;
        }

        [JsonIgnore]
        public int PromptIndex
        {
            get;
            set;
        }

        [JsonIgnore]
        public int NagativePromptIndex
        {
            get;
            set;
        }
        [JsonIgnore]
        private bool isLinkSize = false;
        [JsonIgnore]
        private bool isLinkCkpt = false;
        [JsonIgnore]
        private bool isLinkVae = false;
        [JsonIgnore]
        private bool isLinkPositive = false;
        [JsonIgnore]
        private bool isLinkNegative = false;
        [JsonIgnore]
        private bool isLinkBatchSize = false;

        public EfficientLoader() { }

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

            var prompt = "";
            string newPrompt = "";
            GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
            textAreaStyle.wordWrap = true;
            if (!isLinkPositive)
            {
                PromptIndex = 0;
                GUILayout.Label("正向提示词");
                prompt = aiEditorData.Load<string>(AIEditorData.ValueType.prompt, nodekey, PromptIndex);
                newPrompt = EditorGUILayout.TextArea(prompt,
                    textAreaStyle,
                    GUILayout.Height(70),
                    GUILayout.ExpandWidth(true));
                if (newPrompt != prompt)
                {
                    aiEditorData.Save(newPrompt, AIEditorData.ValueType.prompt, nodekey, PromptIndex);
                }
                Positive = newPrompt;
            }
            if (!isLinkNegative)
            {
                PromptIndex = PromptIndex + 1;
                GUILayout.Label("反向提示词");
                prompt = aiEditorData.Load<string>(AIEditorData.ValueType.prompt, nodekey, PromptIndex);
                newPrompt = EditorGUILayout.TextArea(prompt,
                    textAreaStyle,
                    GUILayout.Height(70),
                    GUILayout.ExpandWidth(true));
                if (newPrompt != prompt)
                {
                    aiEditorData.Save(newPrompt, AIEditorData.ValueType.prompt, nodekey, PromptIndex);
                }
                Negative = newPrompt;
            }

            if(!isLinkBatchSize)
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
            if (inputs["empty_latent_width"].GetType() == typeof(JArray))
                isLinkSize = true;
            if (inputs["empty_latent_height"].GetType() == typeof(JArray))
                isLinkSize = true;
            if (inputs["ckpt_name"].GetType() == typeof(JArray))
                isLinkCkpt = true;
            if (inputs["vae_name"].GetType() == typeof(JArray))
                isLinkVae = true;
            if (inputs["positive"].GetType() == typeof(JArray))
                isLinkPositive = true;
            if (inputs["negative"].GetType() == typeof(JArray))
                isLinkNegative = true;
            if (inputs["batch_size"].GetType() == typeof(JArray))
                isLinkBatchSize = true;

            //ckpt
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
            //vae
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

            //prompt
            PromptIndex = 0;
            if (!isLinkPositive)
            {
                if (aiEditorData.Load<string>(AIEditorData.ValueType.prompt, nodekey, PromptIndex) == "")
                {
                    aiEditorData.Save(Positive, AIEditorData.ValueType.prompt, nodekey, PromptIndex);
                }
            }
            if (!isLinkNegative)
            {
                PromptIndex = isLinkPositive ? 0 : PromptIndex + 1;
                if (aiEditorData.Load<string>(AIEditorData.ValueType.prompt, nodekey, PromptIndex) == "")
                {
                    aiEditorData.Save(Negative, AIEditorData.ValueType.prompt, nodekey, PromptIndex);
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