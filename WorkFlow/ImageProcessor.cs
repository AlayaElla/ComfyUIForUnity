using System;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

namespace ComfyUIForUnity
{
    public class ImageProcessor
    {
        public event Action<float> OnProgressUpdate;
        public event Action<List<Texture2D>> OnImageProcessed;
        public event Action<byte[]> OnImagePreview;

        private WebSocketConnection wsClient;
        private string serverAddress = "127.0.0.1:8188";
        private Guid clientId = Guid.NewGuid();

        private bool isQueueing = false;
        private JsonTool jsonTool;
        private JsonConfig jsonConfig;
        private string prompt_id = "";
        private List<Texture2D> remoteImage;

        private List<SaveDepthRendererFeature> savefeatures;
        private Texture2D depthImage;
        private Texture2D screenImage;
        private Texture2D ToolInputImage;

        public class AfterModifyData
        {
            public Dictionary<string, string> uploadedImages = new Dictionary<string, string>();
        }

        private AfterModifyData afterModifyData = new AfterModifyData();

        private class UploadedImagesCache
        {
            public string path;
            public long size;
        }
        private Dictionary<string, UploadedImagesCache> imagesCacheList = new Dictionary<string, UploadedImagesCache>();

        public ImageProcessor(string serverAddress)
        {
            this.serverAddress = serverAddress;
            afterModifyData = new AfterModifyData();
            imagesCacheList = new Dictionary<string, UploadedImagesCache>();
            remoteImage = new List<Texture2D>();

            GetRendererFeatures();
        }

        void GetRendererFeatures()
        {
            if (savefeatures == null)
            {
                savefeatures = new List<SaveDepthRendererFeature>();
                var renderer = (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset).GetRenderer(0);
                var property = typeof(ScriptableRenderer).GetProperty("rendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);

                List<ScriptableRendererFeature> features = property.GetValue(renderer) as List<ScriptableRendererFeature>;

                foreach (var feature in features)
                {
                    if (feature is SaveDepthRendererFeature && feature.isActive)
                    {
                        savefeatures.Add(feature as SaveDepthRendererFeature);
                    }
                }
            }
        }

        public void SetToolImageTextures(Texture2D texture2D)
        {
            ToolInputImage = texture2D;
        }

        public void StartImageProcessingOperation(JsonTool _jsonTool, JsonConfig _jsonConfig)
        {
            if (isQueueing)
            {
                Debug.Log("���ڵȴ����..");
                return;
            }
            isQueueing = true;

            jsonTool = _jsonTool;
            jsonConfig = _jsonConfig;

            EditorCoroutineUtility.StartCoroutineOwnerless(ExecuteOperation());
        }

        private IEnumerator ExecuteOperation()
        {
            remoteImage.Clear();

            //��ȡ��Ҫ�ϴ���ͼƬ
            var needUploadImage = jsonTool.GetNeedUploadImage(jsonConfig);

            //��ȡ��Ⱥ���ĻͼƬ
            needUploadImage = SetDepthAndColorImage(needUploadImage);

            //���ù�������Ҫ�ϴ���ͼƬ
            needUploadImage = SetToolImage(needUploadImage);

            //��ʼ�ϴ�ͼƬ
            if (needUploadImage != null)
            {
                yield return UploadImageCoroutine(needUploadImage);
            }

            //����Json����
            var promptWorkflow = jsonTool.SetJsonVaule(afterModifyData);

            // ��ʼ��WebSocket����
            yield return InitializeWebSocketConnection();

            if (promptWorkflow == null)
            {
                isQueueing = false;
                yield break;
            }

            // ������ʾ��������
            yield return QueuePromptToServer(promptWorkflow);
        }

        private IEnumerator InitializeWebSocketConnection()
        {
            var wsUrl = $"ws://{serverAddress}/ws?clientId={clientId}";
            if (wsClient == null)
            {
                wsClient = new WebSocketConnection(wsUrl);
                wsClient.OnMessage += HandleProgressUpdate;
                wsClient.OnBinaryMessage += HandleBinaryMessage;
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                if (!wsClient.CheckConnect())
                {
                    wsClient = new WebSocketConnection(wsUrl);
                    wsClient.OnMessage -= HandleProgressUpdate;
                    wsClient.OnMessage += HandleProgressUpdate;
                    wsClient.OnBinaryMessage -= HandleBinaryMessage;
                    wsClient.OnBinaryMessage += HandleBinaryMessage;
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        private IEnumerator QueuePromptToServer(JObject promptWorkflow)
        {
            var jsonObject = new JObject
            {
                ["prompt"] = promptWorkflow,
                ["client_id"] = clientId.ToString()
            };

            var json = jsonObject.ToString(Formatting.None);

            using (UnityWebRequest webRequest = new UnityWebRequest($"http://{serverAddress}/prompt", "POST"))
            {
                byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
                webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("��������Ӧ����: " + webRequest.error);
                    isQueueing = false;
                    OnImageProcessed?.Invoke(null);
                    yield break;
                }
                else
                {
                    var responseString = webRequest.downloadHandler.text;
                    if (string.IsNullOrEmpty(responseString))
                    {
                        Debug.LogError("��������ӦΪ��");
                        isQueueing = false;
                        OnImageProcessed?.Invoke(null);
                        yield break;
                    }

                    prompt_id = JObject.Parse(responseString)["prompt_id"].ToString();
                }
            }
        }

        private IEnumerator FetchAndProcessHistory(string promptId)
        {
            IEnumerator historyCoroutine = GetHistory(promptId);
            yield return historyCoroutine;

            if (historyCoroutine.Current is List<ImageInfo> images)
            {
                DownloadImage(images);
            }
        }
        private IEnumerator GetHistory(string promptId)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get($"http://{serverAddress}/history/{promptId}"))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("��ȡ��ʷ��¼����: " + webRequest.error);
                    yield return null;
                }
                else
                {
                    var responseString = webRequest.downloadHandler.text;
                    if (string.IsNullOrEmpty(responseString))
                    {
                        Debug.LogError("��ʷ��¼��ӦΪ��");
                        yield return null;
                    }
                    else
                    {
                        var history = JObject.Parse(responseString)[prompt_id];
                        var image = history["outputs"][jsonConfig.OutputNode]["images"];
                        yield return JsonConvert.DeserializeObject<List<ImageInfo>>(image.ToString());
                    }
                }
            }
        }

        private void DownloadImage(List<ImageInfo> images)
        {
            if (images != null || images.Count > 0)
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(DownloadImagesCoroutine(images));
            }
        }

        private IEnumerator DownloadImagesCoroutine(List<ImageInfo> images)
        {
            var imageUrl = "";
            foreach (var image in images)
            {
                imageUrl = $"http://{serverAddress}/view?filename={image.filename}&subfolder={image.subfolder}&type={image.type}";
                //Debug.Log(imageUrl);
                using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
                {
                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.LogError("ͼƬ���ش���: " + request.error);
                    }
                    else
                    {
                        remoteImage.Add(DownloadHandlerTexture.GetContent(request));
                    }
                }
            }
            OnImageProcessed?.Invoke(remoteImage);
            isQueueing = false;
        }

        Dictionary<string, Texture2D> SetDepthAndColorImage(Dictionary<string, Texture2D> needUploadImage)
        {
            if (jsonConfig.NodesForDepthImage == null || jsonConfig.NodesForColorImage == null)
                return needUploadImage;

            foreach (var feature in savefeatures)
            {
                if (feature.isActive)
                {
                    if (feature.saveType == SaveDepthRendererFeature.SaveType.Depth)
                    {
                        depthImage = feature.CaptureTexture();
                        depthImage.name = "depth";
                    }
                    else if (feature.saveType == SaveDepthRendererFeature.SaveType.Color)
                    {
                        screenImage = feature.CaptureTexture();
                        screenImage.name = "screen";
                    }
                }
            }
            foreach (string nodekey in jsonConfig.NodesForDepthImage)
            {
                if (needUploadImage.ContainsKey(nodekey))
                    needUploadImage[nodekey] = depthImage;
                else
                    needUploadImage.Add(nodekey, depthImage);
            }
            foreach (string nodekey in jsonConfig.NodesForColorImage)
            {
                if (needUploadImage.ContainsKey(nodekey))
                    needUploadImage[nodekey] = screenImage;
                else
                    needUploadImage.Add(nodekey, screenImage);
            }

            return needUploadImage;
        }

        Dictionary<string, Texture2D> SetToolImage(Dictionary<string, Texture2D> needUploadImage)
        {
            if (ToolInputImage == null || jsonConfig.NodesForToolImage == null)
                return needUploadImage;

            foreach (string nodekey in jsonConfig.NodesForToolImage)
            {
                needUploadImage[nodekey] = ToolInputImage;
            }
            return needUploadImage;
        }

        public IEnumerator UploadImageCoroutine(Dictionary<string, Texture2D> images)
        {
            afterModifyData.uploadedImages = new Dictionary<string, string>();
            foreach (var image in images)
            {
                yield return StartUploadImageCoroutine(image.Key, image.Value);
            }
        }

        private IEnumerator StartUploadImageCoroutine(string key, Texture2D texture2D, bool overwrite = true)
        {
            var imageBytes = texture2D.EncodeToPNG();
            if (imageBytes == null)
            {
                Debug.LogError("ͼƬ����ʧ��");
                isQueueing = false;
                OnImageProcessed?.Invoke(null);
                yield break;
            }

            long imageSize = imageBytes.Length;
            if (imagesCacheList.TryGetValue(key, out UploadedImagesCache imagesCache))
            {
                if (imageSize == imagesCache.size)
                {
                    //Debug.Log($"ͼƬ {key} ���ϴ��������ϴ�����,��ַΪ:{imagesCache.path}");
                    afterModifyData.uploadedImages[key] = imagesCache.path;
                    yield break;
                }
            }

            WWWForm form = new WWWForm();
            form.AddBinaryData("image", imageBytes, $"{texture2D.name}.png");
            form.AddField("overwrite", overwrite ? "1" : "0");

            using (UnityWebRequest www = UnityWebRequest.Post($"http://{serverAddress}/upload/image", form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("ͼƬ�ϴ�ʧ��: " + www.error);
                    isQueueing = false;
                    OnImageProcessed?.Invoke(null);
                    yield break;
                }
                else
                {
                    var responseString = www.downloadHandler.text;
                    //Debug.Log("ͼƬ�ϴ��ɹ�:" + responseString);
                    if (!string.IsNullOrEmpty(responseString))
                    {
                        var response = JObject.Parse(responseString);
                        //var type = response["type"].ToString();
                        var imagePath = "";

                        if (response["subfolder"].ToString() != "")
                            imagePath = $"{response["subfolder"]}/{response["name"]}";
                        else
                            imagePath = $"{response["name"]}";

                        afterModifyData.uploadedImages[key] = imagePath;
                        imagesCacheList[key] = new UploadedImagesCache
                        {
                            path = imagePath,
                            size = imageSize
                        };
                    }
                }
            }
        }

        private void HandleProgressUpdate(object sender, string message)
        {
            var progressUpdate = JObject.Parse(message);
            JToken data = null;
            //Debug.Log(progressUpdate["type"].ToString());

            switch (progressUpdate["type"].ToString())
            {
                case "progress":
                    data = progressUpdate["data"];
                    float _p = float.Parse(data["value"].ToString());
                    float max = float.Parse(data["max"].ToString());
                    float progress = (_p / max) * 100;
                    OnProgressUpdate?.Invoke(progress);
                    break;
                case "status":
                    break;
                case "execution_cached":
                    data = progressUpdate["data"];
                    if (data["nodes"].ToArray().Contains(jsonConfig.OutputNode))
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(FetchAndProcessHistory(prompt_id));
                    }
                    break;
                case "executed":
                    data = progressUpdate["data"];
                    if (data["node"].ToString() == jsonConfig.OutputNode && data["prompt_id"].ToString() == prompt_id)
                    {
                        var image = JsonConvert.DeserializeObject<List<ImageInfo>>(data["output"]["images"].ToString());
                        DownloadImage(image);
                    }
                    break;
            }
        }
        private void HandleBinaryMessage(object sender, byte[] binaryData)
        {
            OnImagePreview?.Invoke(binaryData);
        }

        public bool CheckQueueing()
        {
            return isQueueing;
        }
    }
}