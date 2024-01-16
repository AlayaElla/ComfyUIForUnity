using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ComfyUIForUnity
{
    public class Color : Node
    {
        [JsonIgnore]
        public JObject ColorHex
        {
            get => (JObject)inputs["color"];
            set => inputs["color"] = value;
        }

        [JsonIgnore]
        private UnityEngine.Color _color = UnityEngine.Color.white;

        public Color() { }

        public override void DrawGUI(AIEditorData aiEditorData, ServerInfo serverInfo, string nodekey)
        {
            base.DrawGUI(aiEditorData, serverInfo, nodekey);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("颜色", GUILayout.Width(80));
            var color_hex = aiEditorData.Load<string>(AIEditorData.ValueType.color, nodekey);
            _color = EditorGUILayout.ColorField(HexToColor(color_hex));
            var hex = ColorToHex(_color);
            if(hex != color_hex)
            {
                aiEditorData.Save(hex, AIEditorData.ValueType.color, nodekey);
            }
            ColorHex["hex"] = hex;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        public override void SetDefaultValue(AIEditorData aiEditorData, ServerInfo serverInfo, string nodekey)
        {
            //设置默认值
            if (aiEditorData.Load<string>(AIEditorData.ValueType.color, nodekey) == "")
            {
                aiEditorData.Save(ColorHex["hex"].ToString(), AIEditorData.ValueType.color, nodekey);
            }
        }

        string ColorToHex(UnityEngine.Color color)
        {
            return "#" + ColorUtility.ToHtmlStringRGB(color);
        }

        UnityEngine.Color HexToColor(string hex)
        {
            UnityEngine.Color color = UnityEngine.Color.white;
            ColorUtility.TryParseHtmlString(hex, out color);
            return color;
        }
    }
}