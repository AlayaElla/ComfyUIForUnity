using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ComfyUIForUnity
{
    public class UIAIGenerator : EditorWindowsBase
    {
        private Image originalUI;
        private int currentImageID = 0;
        private Dictionary<int, Sprite> originalImages;
        private Dictionary<int, List<Texture2D>> cacheRemoteImages;
        private int tabSelected = 0;
        private string[] tabs = new string[] { "配置", "结果" };

        private int hoveredImageID = 0;

        [MenuItem("Tool/AI/UI生成器", false, 2)]
        public static void ShowWindow()
        {
            GetWindow<UIAIGenerator>("UI生成器");
        }

        protected override void OnEnable()
        {
            jsonName = "UIToolConfig.json";
            toolName = "UIAIGenerator";
            buttomFixHeight = 280;
            base.OnEnable();
            originalImages = new Dictionary<int, Sprite>();
            cacheRemoteImages = new Dictionary<int, List<Texture2D>>();
            aiEditorData.SetToolName(toolName);
        }

        protected override void OnDisable()
        {
            originalImages.Clear();
            cacheRemoteImages.Clear();
            base.OnDisable();
        }

        private void OnGUI()
        {
            if (!serverInfo.IsGetInfo())
            {
                GUILayout.Label("正在获取服务器信息...");
                return;
            }
            InitGUI();

            EditorGUILayout.BeginVertical(customBoxStyle);
            DrawJsonTypeSelection();
            SetUIImage();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            tabSelected = GUILayout.Toolbar(tabSelected, tabs);

            switch (tabSelected)
            {
                case 0:
                    DrawConfiguration();
                    break;
                case 1:
                    if (cacheRemoteImages.ContainsKey(currentImageID))
                        DrawImage(cacheRemoteImages[currentImageID]);
                    else
                        DrawImage(remoteImages);
                    break;
            }
            DrawExecuteButton();
            DrawCustomProgressBar();
            EditorGUILayout.EndVertical();
        }

        protected override void DrawExecuteButton(int buttonHeight = 40)
        {
            GUI.backgroundColor = UnityEngine.Color.white;
            GUILayout.FlexibleSpace();
            //GUILayout.Label(progressText);
            if (jsonConfig.NodesForToolImage.Length == 0)
            {
                EditorGUI.BeginDisabledGroup(ButtonText == "正在生成图片..."
                    || !originalImages.ContainsKey(currentImageID));
            }
            else
            {
                EditorGUI.BeginDisabledGroup(ButtonText == "正在生成图片..."
                    || !originalImages.ContainsKey(currentImageID)
                    || originalImages[currentImageID] == null);
            }

            if (GUILayout.Button(ButtonText, GUILayout.Height(buttonHeight)))
            {
                MakeTextureReadable(AssetDatabase.GetAssetPath(originalImages[currentImageID]));
                if (jsonConfig.NodesForToolImage.Length > 0)
                    processor.SetToolImageTextures(originalImages[currentImageID].texture);

                ClickExecuteButton();
            }
            EditorGUI.EndDisabledGroup();
        }

        protected override void DisplayImage(List<Texture2D> images)
        {
            if (images != null)
            {
                remoteImages = images;
                if(jsonConfig.NodesForToolImage.Length > 0)
                    remoteImages.Add(originalImages[currentImageID].texture);

                AddCacheRemoteImages(currentImageID, remoteImages);
                ChangeUISprite(0);
                hoveredImageID = 0;
                tabSelected = 1;
            }

            ButtonText = "生成图片";
            //ChangeUISprite();
            needToUpdateUI = true;
        }

        void SetUIImage()
        {
            GUILayout.Label("拖拽UI图片到这里", EditorStyles.boldLabel);
            var newOriginalUI = EditorGUILayout.ObjectField(originalUI, typeof(Image), true) as Image;
            if (newOriginalUI != originalUI && !processor.CheckQueueing())
            {
                originalUI = newOriginalUI;
                remoteImages.Clear();
            }

            // 显示Image对应的Sprite
            if (originalUI != null)
            {
                currentImageID = originalUI.GetInstanceID();

                if (!cacheRemoteImages.ContainsKey(currentImageID))
                {
                    if (originalUI.sprite != null && AssetDatabase.GetAssetPath(originalUI.sprite.texture) != "")
                        AddOriginImage(currentImageID, originalUI.sprite);
                    else
                        AddOriginImage(currentImageID, null);
                }

                if (originalImages.ContainsKey(currentImageID))
                {
                    var sprite = originalImages[currentImageID];
                    if (sprite != null)
                    {
                        float aspectRatio = (float)sprite.texture.width / sprite.texture.height;
                        int imageWidth = 96;
                        int imageHeight = (int)(imageWidth / aspectRatio);
                        Rect rect = GUILayoutUtility.GetRect(imageWidth, imageHeight);
                        GUI.DrawTexture(rect, sprite.texture, ScaleMode.ScaleToFit);
                    }
                    else
                    {
                        GUILayout.Label("没有Spitre");
                    }
                }
            }
        }

        void AddOriginImage(int ID, Sprite sprite)
        {
            if (originalImages.Count > 20)
            {
                originalImages.Remove(originalImages.Keys.GetEnumerator().Current);
            }
            originalImages[ID] = sprite;
        }

        void AddCacheRemoteImages(int ID, List<Texture2D> images)
        {
            if (cacheRemoteImages.Count > 20)
            {
                cacheRemoteImages.Remove(cacheRemoteImages.Keys.GetEnumerator().Current);
            }
            List<Texture2D> copiedImages = images.ToList();
            cacheRemoteImages[ID] = copiedImages;
        }

        protected override void HandleImageEvents(List<Texture2D> textures, Rect imageRect, int imageIndex)
        {
            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.MouseDown:
                    if (imageRect.Contains(evt.mousePosition) && evt.button == 0)
                    {
                        hoveredImageID = imageIndex;
                        ChangeUISprite(hoveredImageID);
                        needToUpdateUI = true;
                    }
                    break;
                case EventType.ContextClick:
                    if (imageRect.Contains(evt.mousePosition))
                    {
                        ShowContextMenu(textures, imageIndex);
                        evt.Use();
                    }
                    break;
            }

            //高亮
            if (hoveredImageID == imageIndex)
            {
                EditorGUI.DrawRect(imageRect, new UnityEngine.Color(1f, 1f, 1f, 0.1f));
            }
        }

        void ChangeUISprite(int imageIndex)
        {
            if (originalImages == null)
                return;

            var _images = cacheRemoteImages[currentImageID];
            var _sprite = Sprite.Create(_images[imageIndex], new Rect(0, 0, _images[imageIndex].width, _images[imageIndex].height), Vector2.zero);
            originalUI.sprite = _sprite;
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

        public void SetOriginalUI(Image newOriginalUI)
        {
            if (processor.CheckQueueing())
                return;
            //if (newOriginalUI.sprite == null)
            //    return;
            originalUI = newOriginalUI;
        }

        public static bool IsWindowOpen()
        {
            return Resources.FindObjectsOfTypeAll<UIAIGenerator>().Length > 0;
        }

    }
}
