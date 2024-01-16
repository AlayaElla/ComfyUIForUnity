using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace ComfyUIForUnity
{
    public class LoadImage : Node
    {
        [JsonIgnore]
        public string Image
        {
            get => (string)inputs["image"];
            set => inputs["image"] = value;
        }

        [JsonIgnore]
        public Texture2D NeedUploadImage
        {
            get;
            set;
        }

        [JsonIgnore]
        public Texture2D DefaultImage = new Texture2D(8, 8);

        [JsonIgnore]
        public int ImageIndex
        {
            get;
            set;
        }

        public LoadImage() { }

        public override void DrawGUI(AIEditorData aiEditorData, ServerInfo serverInfo, string nodekey)
        {
            base.DrawGUI(aiEditorData, serverInfo, nodekey);

            // 尝试加载历史图片
            var path = aiEditorData.Load<string>(AIEditorData.ValueType.imagePath, nodekey);
            if (NeedUploadImage == null && !string.IsNullOrEmpty(path))
            {
                NeedUploadImage = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }

            GUILayout.BeginVertical();
            GUILayout.Label("参考图");
            GUILayoutOption[] options = {
            GUILayout.Width(128),
            GUILayout.Height(128)};

            Texture2D newImage = EditorGUILayout.ObjectField(NeedUploadImage, typeof(Texture2D), false, options) as Texture2D;
            if (newImage != NeedUploadImage)
            {
                NeedUploadImage = newImage;
                path = AssetDatabase.GetAssetPath(NeedUploadImage);
                aiEditorData.Save(path,AIEditorData.ValueType.imagePath, nodekey);

                // 确保图片是可读的
                MakeTextureReadable(path);
            }
            EditorGUILayout.EndVertical();
        }

        private void MakeTextureReadable(string assetPath)
        {
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (textureImporter != null && !textureImporter.isReadable)
            {
                textureImporter.isReadable = true;
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }
        }
    }
}