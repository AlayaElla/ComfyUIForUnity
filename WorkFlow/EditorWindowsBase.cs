using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.Experimental.GraphView;

namespace ComfyUIForUnity
{
    public class EditorWindowsBase : EditorWindow
    {
        protected ImageProcessor processor;
        protected ServerInfo serverInfo;
        protected JsonTool jsonTool;
        protected AIToolConfig aIToolConfig;
        protected AIEditorData aiEditorData;

        protected string serverAddress = "127.0.0.1:8188";
        protected string ButtonText = "生成图片";
        protected float currentProgress = 0;
        protected List<Texture2D> remoteImages;
        protected Texture2D previewImage;

        protected JsonConfig jsonConfig;

        protected int promptCount = 0;
        protected int uploadImageCount = 0;
        protected int loaraNodeCount = 0;

        protected bool init = false;
        protected bool needToUpdateUI = false;
        protected byte[] previewImageData = null;
        protected int imagesPerRow = 2;
        protected int buttomFixHeight = 145;//显示图片滚动区域距离底部高度

        protected GUIStyle customBoxStyle;
        protected Vector2 scrollPosition;

        protected string jsonName = "";
        protected string toolName = "";

        protected virtual void OnEnable()
        {
            init = false;

            aIToolConfig = AssetDatabase.LoadAssetAtPath<AIToolConfig>("Assets/ComfyUIForUnity/AIToolConfig.asset");
            jsonTool = new JsonTool(aIToolConfig, jsonName);
            serverAddress = aIToolConfig.Address;
            processor = new ImageProcessor(serverAddress);
            serverInfo = new ServerInfo(serverAddress);
            aiEditorData = new AIEditorData(jsonTool, serverInfo);
            remoteImages = new List<Texture2D>();

            processor.OnProgressUpdate += UpdateProgress;
            processor.OnImageProcessed += DisplayImage;
            processor.OnImagePreview += DisplayPreview;
            serverInfo.GetServerInfo();

            EditorApplication.update += UpdateUI;
        }

        protected virtual void OnDisable()
        {
            processor.OnProgressUpdate -= UpdateProgress;
            processor.OnImageProcessed -= DisplayImage;
            processor.OnImagePreview -= DisplayPreview;

            EditorApplication.update -= UpdateUI;
        }

        protected void InitGUI()
        {
            if (!init)
            {
                if (customBoxStyle == null)
                {
                    customBoxStyle = new GUIStyle("box");
                    customBoxStyle.normal.background = MakeTex(2, 2, new UnityEngine.Color(0.3f, 0.3f, 0.3f));
                }

                jsonConfig = jsonTool.GetTypeConfig(aiEditorData.json_name_index);
                aiEditorData.json_name_index = Math.Min(aiEditorData.json_name_index, jsonTool.GetJsonNameList().Length - 1);
                init = true;
            }
        }

        protected void DrawConfiguration()
        {
            var nodes = jsonTool.GetNodes();
            if (nodes == null)
                return;
            promptCount = 0;
            uploadImageCount = 0;
            loaraNodeCount = 0;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (var nodeKey in jsonConfig.NodesToModify)
            {
                if (nodes.ContainsKey(nodeKey))
                {
                    //DrawNodeUI(nodes[nodeKey]);
                    nodes[nodeKey].DrawGUI(aiEditorData, serverInfo, nodeKey);
                }
            }
            GUILayout.Space(10);
            EditorGUILayout.EndScrollView();
        }

        //protected void DrawNodeUI(Node node, string nodeKey)
        //{
        //    switch (node)
        //    {
        //        case PromptBase prompt:
        //            promptCount++;
        //            node.DrawGUI(aiEditorData, serverInfo, promptCount);
        //            break;
        //        case LoadImage loadImage:
        //            uploadImageCount++;
        //            node.DrawGUI(aiEditorData, serverInfo, uploadImageCount);
        //            break;
        //        case LoRAStacker stacker:
        //            loaraNodeCount++;
        //            node.DrawGUI(aiEditorData, serverInfo, loaraNodeCount);
        //            break;
        //        default:
        //            node.DrawGUI(aiEditorData, serverInfo);
        //            break;
        //    }
        //}

        protected virtual void DrawJsonTypeSelection()
        {
            GUILayout.Label("使用功能", EditorStyles.boldLabel);
            int newJsonNameIndex = EditorGUILayout.Popup(aiEditorData.json_name_index, jsonTool.GetJsonNameList());
            if (newJsonNameIndex != aiEditorData.json_name_index)
            {
                aiEditorData.json_name_index = newJsonNameIndex;
                jsonConfig = jsonTool.GetTypeConfig(aiEditorData.json_name_index); // 重新加载配置
                needToUpdateUI = true;
            }
            aiEditorData.SetJsonData(toolName + ":" +
                jsonTool.GetJsonNameList()[aiEditorData.json_name_index]);
            GUILayout.Space(5);
        }

        protected void ClickExecuteButton()
        {
            ButtonText = "正在生成图片...";
            UpdateProgress(0);

            processor.StartImageProcessingOperation(jsonTool, jsonConfig);
        }

        protected virtual void DrawExecuteButton(int buttonHeight = 40)
        {
            GUI.backgroundColor = UnityEngine.Color.white;
            GUILayout.FlexibleSpace();
            //GUILayout.Label(progressText);
            EditorGUI.BeginDisabledGroup(ButtonText == "正在生成图片...");
            if (GUILayout.Button(ButtonText, GUILayout.Height(buttonHeight)))
            {
                ClickExecuteButton();
            }
            EditorGUI.EndDisabledGroup();
        }

        protected virtual void DisplayImage(List<Texture2D> images)
        {
            if (images != null)
                remoteImages = images;
            ButtonText = "生成图片";
            needToUpdateUI = true;
        }

        protected virtual void DisplayPreview(byte[] binaryData)
        {
            previewImageData = binaryData;
            //needToUpdateUI = true;
        }

        protected void DrawCustomProgressBar()
        {
            Rect rect = new Rect(0, position.height - 5, position.width, 5);
            EditorGUI.DrawRect(rect, UnityEngine.Color.gray);

            float progressWidth = rect.width * (currentProgress / 100.0f);
            Rect progressRect = new Rect(rect.x, position.height - 5, progressWidth, rect.height);
            EditorGUI.DrawRect(progressRect, new UnityEngine.Color(40 / 255f, 100 / 255f, 85 / 255f));
        }

        protected void UpdateProgress(float percent)
        {
            currentProgress = percent;
            needToUpdateUI = true;
        }


        protected void UpdateUI()
        {
            if (needToUpdateUI)
            {
                needToUpdateUI = false;
                Repaint();
            }
            if (previewImageData != null)
            {
                Texture2D texture = ConvertBytesToTexture2D(previewImageData);
                if (texture != null)
                {
                    previewImage = texture;
                }
                previewImageData = null;
            }
        }

        protected virtual void DrawImage(List<Texture2D> textures)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("每行图片数量:", GUILayout.Width(80));
            if (textures.Count == 1)
                imagesPerRow = 1;
            imagesPerRow = (int)GUILayout.HorizontalSlider(imagesPerRow, 1, 6);
            GUILayout.EndHorizontal();

            if (textures.Count == 0)
            {
                GUILayout.Label("没有图片");
                return;
            }

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width - 5), GUILayout.Height(position.height - buttomFixHeight));

            float spacing = 5f; // 图片间隔
            float maxImageWidth = (position.width - (spacing * (imagesPerRow + 1))) / imagesPerRow;
            int rowCount = (int)Mathf.Ceil(textures.Count / (float)imagesPerRow);

            for (int row = 0; row < rowCount; row++)
            {
                GUILayout.BeginHorizontal();
                for (int col = 0; col < imagesPerRow; col++)
                {
                    int imageIndex = row * imagesPerRow + col;
                    if (imageIndex < textures.Count)
                    {
                        Texture2D image = textures[imageIndex];
                        float aspectRatio = (float)image.width / image.height;
                        float imageWidth = Mathf.Min(image.width, maxImageWidth);
                        float imageHeight = imageWidth / aspectRatio;

                        GUILayout.Space(spacing);
                        Rect imageRect = GUILayoutUtility.GetRect(imageWidth, imageHeight);
                        GUI.DrawTexture(imageRect, image, ScaleMode.ScaleToFit);

                        //图片菜单
                        HandleImageEvents(textures, imageRect, imageIndex);
                    }
                    else
                        GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                if (row < rowCount - 1)
                {
                    GUILayout.Space(spacing);
                }
            }

            GUILayout.EndScrollView();
        }

        protected virtual void HandleImageEvents(List<Texture2D> textures, Rect imageRect, int imageIndex)
        {
            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.ContextClick:
                    if (imageRect.Contains(evt.mousePosition))
                    {
                        ShowContextMenu(textures, imageIndex);
                        evt.Use();
                    }
                    break;
            }
        }

        protected virtual void ShowContextMenu(List<Texture2D> textures, int imageIndex)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("保存图片"), false, () => SaveImageDialog(textures, imageIndex));
            menu.AddItem(new GUIContent("生成图片"), false, ClickExecuteButton);
            menu.ShowAsContext();
        }

        protected virtual void SaveImageDialog(List<Texture2D> textures, int imageIndex)
        {
            string defaultFileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string path = EditorUtility.SaveFilePanel("保存图片", "", defaultFileName, "png");
            if (!string.IsNullOrEmpty(path))
            {
                byte[] imageBytes = textures[imageIndex].EncodeToPNG();
                File.WriteAllBytes(path, imageBytes);
            }
        }

        protected Texture2D ConvertBytesToTexture2D(byte[] imageData)
        {
            byte[] imageBytes = imageData.Skip(4).ToArray();
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(imageBytes))
            {
                return texture;
            }
            else return null;
        }

        Texture2D MakeTex(int width, int height, UnityEngine.Color col)
        {
            UnityEngine.Color[] pix = new UnityEngine.Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
