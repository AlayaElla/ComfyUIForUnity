using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ComfyUIForUnity
{
    public class ScreenAIGenerator : EditorWindowsBase
    {
        private int tabSelected = 0;
        private string[] tabs = new string[] { "����", "���" };
        protected bool isImageOnlyMode = false;

        [MenuItem("Tool/AI/ͼƬ����", false, 1)]
        public static void ShowWindow()
        {
            GetWindow<ScreenAIGenerator>("ͼƬ������");
        }

        protected override void OnEnable()
        {
            jsonName = "ScreenToolConfig.json";
            toolName = "ScreenAIGenerator";
            base.OnEnable();
            aiEditorData.SetToolName(toolName);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        private void OnGUI()
        {
            if (!serverInfo.IsGetInfo())
            {
                GUILayout.Label("���ڻ�ȡ��������Ϣ...");
                return;
            }
            InitGUI();
            if (!isImageOnlyMode)
            {
                EditorGUILayout.BeginVertical(customBoxStyle);
                DrawJsonTypeSelection();
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical("box");
                tabSelected = GUILayout.Toolbar(tabSelected, tabs);
            }
            else
                EditorGUILayout.BeginVertical("box");


            switch (tabSelected)
            {
                case 0:
                    DrawConfiguration();
                    break;
                case 1:
                    DrawImage(remoteImages);
                    break;
            }

            DrawExecuteButton(isImageOnlyMode ? 25 : 40);
            DrawCustomProgressBar();
            EditorGUILayout.EndVertical();
        }

        protected override void DrawJsonTypeSelection()
        {
            GUILayout.Label("ʹ�ù���", EditorStyles.boldLabel);
            int newJsonNameIndex = EditorGUILayout.Popup(aiEditorData.json_name_index, jsonTool.GetJsonNameList());
            if (newJsonNameIndex != aiEditorData.json_name_index)
            {
                aiEditorData.json_name_index = newJsonNameIndex;
                jsonConfig = jsonTool.GetTypeConfig(aiEditorData.json_name_index); // ���¼�������
                tabSelected = 0;
                needToUpdateUI = true;
            }
            aiEditorData.SetJsonData(toolName + ":" +
                jsonTool.GetJsonNameList()[aiEditorData.json_name_index]);
            GUILayout.Space(5);
        }

        protected override void DisplayImage(List<Texture2D> images)
        {
            if (images != null)
            {
                remoteImages = images;
                tabSelected = 1;
            }

            ButtonText = "����ͼƬ";
            needToUpdateUI = true;
        }

        protected override void DisplayPreview(byte[] binaryData)
        {
            previewImageData = binaryData;
            tabSelected = 1;
            //needToUpdateUI = true;
        }
    }
}
