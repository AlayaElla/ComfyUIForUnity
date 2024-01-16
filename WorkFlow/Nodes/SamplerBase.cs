using UnityEngine;
using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;

namespace ComfyUIForUnity
{
    public class SamplerBase : Node
    {
        [JsonIgnore]
        public ulong Seed
        {
            get => (ulong)inputs["seed"];
            set => inputs["seed"] = value;
        }
        [JsonIgnore]
        public float Denoise
        {
            get => (float)inputs["denoise"];
            set => inputs["denoise"] = value;
        }
        [JsonIgnore]
        public string EditorSeed { get; set; }

        [JsonIgnore]
        protected bool isLinkSeed = false;
        [JsonIgnore]
        protected bool isLinkDenoise = false;

        public ulong GetSeed()
        {
            ulong seed = 0;
            if (EditorSeed == null || EditorSeed == "-1")
            {
                System.Random random = new System.Random();
                seed = ((ulong)(uint)random.Next() << 32) | (uint)random.Next();
            }
            else
            {
                try
                {
                    seed = Convert.ToUInt64(EditorSeed);
                }
                catch (Exception ex)
                {
                    Debug.LogError("种子值不合法: " + ex.Message);
                    return 0;
                }
            }
            return seed;
        }

        public override void SetDefaultValue(AIEditorData aiEditorData, ServerInfo serverInfo, string nodekey)
        {
            if (inputs["seed"].GetType() == typeof(JArray))
                isLinkSeed = true;
            if (inputs["denoise"].GetType() == typeof(JArray))
                isLinkDenoise = true;

            if (!isLinkSeed)
            {
                if (aiEditorData.Load<string>(AIEditorData.ValueType.seed, nodekey) == "")
                {
                    aiEditorData.Save("-1", AIEditorData.ValueType.seed, nodekey);
                }
            }
            if (!isLinkDenoise)
            {
                if (aiEditorData.Load<float>(AIEditorData.ValueType.denoise, nodekey) == -1)
                {
                    aiEditorData.Save(1f, AIEditorData.ValueType.denoise, nodekey);
                }
            }
        }
    }
}