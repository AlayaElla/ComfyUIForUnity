using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ComfyUIForUnity
{
    [System.Serializable]
    public class Node
    {
        public Dictionary<string, object> inputs;
        public string class_type;
        public Meta _meta;

        [JsonIgnore]
        public Texture2D line;

        public Node()
        {
            line = new Texture2D(1, 1);
            line.SetPixel(0, 0, new UnityEngine.Color(0.3f, 0.3f, 0.3f));
            line.Apply();
        }

        public virtual void DrawGUI(AIEditorData aiEditorData, ServerInfo serverInfo, string nodeKey)
        {
            DrawLine();
            SetDefaultValue(aiEditorData, serverInfo, nodeKey);
        }

        public virtual void SetDefaultValue(AIEditorData aiEditorData, ServerInfo serverInfo, string nodeKey) { }

        public void DrawLine()
        {
            GUILayout.Space(5);
            GUIStyle whiteStyle = new GUIStyle();
            whiteStyle.normal.background = line;
            GUILayout.Box("", whiteStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Space(5);
        }
    }

    [System.Serializable]
    public class Meta
    {
        public string title;
    }

    public class ImageInfo
    {
        public string filename;
        public string subfolder;
        public string type;
    }

    public class NodeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Node));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            string classType = "ComfyUIForUnity." + jsonObject["class_type"].Value<string>()
                    .Replace(" ", "")
                    .Replace("(", "_")
                    .Replace(")", "_");
            Node node;
            try
            {
                Type nodeType = Type.GetType(classType);
                if (nodeType != null && nodeType.IsSubclassOf(typeof(Node)))
                {
                    node = (Node)Activator.CreateInstance(nodeType);
                }
                else
                {
                    throw new InvalidOperationException("未找到匹配的子类: " + classType);
                }
            }
            catch //(Exception ex)
            {
                //Debug.Log(ex.Message);
                node = new Node();
            }

            serializer.Populate(jsonObject.CreateReader(), node);
            return node;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}

