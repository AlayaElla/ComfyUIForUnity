using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ComfyUIForUnity
{
    [Serializable]
    public class JsonConfig
    {
        public string Json = "";
        public string[] NodesToModify = new string[0];
        public string[] NodesForToolImage = new string[0];
        public string[] NodesForDepthImage = new string[0];
        public string[] NodesForColorImage = new string[0];
        public string OutputNode { get; set; }
    }
    [Serializable]
    public class JsonConfigs
    {
        public Dictionary<string, JsonConfig> Configs = new Dictionary<string, JsonConfig>();
    }

    public class JsonTool
    {
        //public JsonType jsonType { get; set; }
        private JsonConfigs configs;
        //private JObject json;
        private Dictionary<string, Node> nodes;
        private string[] jsonNameList;
        private string toolName;

        public JsonTool(AIToolConfig _config, string configsName)
        {
            toolName = _config.Name;
            LoadConfig(configsName);
        }

        private void LoadConfig(string configsName)
        {
            string jsonPath = Application.dataPath + $"/{toolName}/Json/" + configsName;
            try
            {
                if (!File.Exists(jsonPath))
                {
                    Debug.LogError("配置文件未找到: " + jsonPath);
                    return;
                }

                string _json = File.ReadAllText(jsonPath);

                configs = JsonConvert.DeserializeObject<JsonConfigs>(_json);
                jsonNameList = configs.Configs.Keys.ToArray();

                if (configs == null)
                {
                    Debug.LogError("JSON解析失败: " + jsonPath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("读取配置文件时出错: " + ex.Message);
            }
        }

        public JsonConfig GetTypeConfig(int index)
        {
            var jsonName = jsonNameList[Math.Min(index, jsonNameList.Length - 1)];
            JsonConfig config;
            if (configs.Configs.TryGetValue(jsonName, out config))
            {
                LoadJson(config.Json);
                return config;
            }

            Debug.LogError("未找到配置: " + jsonName);
            return null;
        }

        public void LoadJson(string _path)
        {
            var jsonPath = Application.dataPath + "/" + toolName + _path;
            if (!File.Exists(jsonPath))
            {
                Debug.LogError("JSON文件未找到: " + jsonPath);
                return;
            }

            var _json = File.ReadAllText(jsonPath);
            //json = JObject.Parse(_json);

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new NodeConverter());
            nodes = JsonConvert.DeserializeObject<Dictionary<string, Node>>(_json, settings);
        }

        public JObject SetJsonVaule(ImageProcessor.AfterModifyData afterModifyData)
        {
            if (afterModifyData.uploadedImages.Count != 0)
            {
                foreach (var image in afterModifyData.uploadedImages)
                {
                    var node = nodes[image.Key];
                    if (node is LoadImage loadImage)
                        loadImage.Image = image.Value;
                }
            }

            //设置参数
            foreach (Node node in nodes.Values)
            {
                switch (node)
                {
                    case SamplerBase sampler:
                        sampler.Seed = sampler.GetSeed();
                        break;
                }
            }
            return JObject.FromObject(nodes);
        }

        public Dictionary<string, Texture2D> GetNeedUploadImage(JsonConfig config)
        {
            Dictionary<string, Texture2D> images = new Dictionary<string, Texture2D>();
            foreach (var node in nodes)
            {
                string nodeKey = node.Key;
                Node value = node.Value;

                if (value is LoadImage loadImage
                    && !config.NodesForDepthImage.Contains(nodeKey)
                    && !config.NodesForColorImage.Contains(nodeKey)
                    && !config.NodesForToolImage.Contains(nodeKey))
                {
                    if (loadImage.NeedUploadImage != null)
                    {
                        images[nodeKey] = loadImage.NeedUploadImage;
                    }
                    else//使用一个默认的图片
                    {
                        Debug.Log("没有找到上传的图片！Node: " + nodeKey);
                        images[nodeKey] = loadImage.DefaultImage;
                    }
                }
            }
            return images;
        }

        public JObject GetJson()
        {
            return JObject.FromObject(nodes);
            //return json;
        }

        public Dictionary<string, Node> GetNodes()
        {
            return nodes;
        }

        public string[] GetJsonNameList()
        {
            return jsonNameList;
        }
    }
}