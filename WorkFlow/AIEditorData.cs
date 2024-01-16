using System;
using System.Collections.Generic;
using UnityEditor;

namespace ComfyUIForUnity
{
    public class AIEditorData
    {
        private JsonTool jsonTool;
        private ServerInfo serverInfo;
        private string nowJsonName = "";
        private string nowToolName = "";

        public int json_name_index
        {
            get => LoadJsonIndex($"{nowToolName}:{GetKey(ValueType.json_name_index)}");
            set => SaveJsonIndex($"{nowToolName}:{GetKey(ValueType.json_name_index)}", value);
        }

        class cacheValue
        {
            public object value;
            public bool isCache = false;
        }
        private Dictionary<string, cacheValue> cache = new Dictionary<string, cacheValue>();

        public AIEditorData(JsonTool _jsonTool, ServerInfo _serverInfo)
        {
            jsonTool = _jsonTool;
            serverInfo = _serverInfo;
            //初始化缓存
            cache = new Dictionary<string, cacheValue>();
        }
        public void SetToolName(string _nowToolName)
        {
            nowToolName = _nowToolName;
        }
        public void SetJsonData(string _nowJsonName)
        {
            nowJsonName = _nowJsonName;
        }

        public enum ValueType
        {
            json_name_index,
            ckpt_name_index,
            vae_name_index,
            seed,
            width,
            height,
            batch,
            denoise,
            prompt,
            color,
            lora_count,
            lora_name_index,
            lora_weight,
            imagePath
        }
        private string GetKey(ValueType type)
        {
            switch (type)
            {
                case ValueType.json_name_index:
                    return "JsonTypeSelected";
                case ValueType.ckpt_name_index:
                    return "CkptNameIndex";
                case ValueType.vae_name_index:
                    return "VaeNameIndex";
                case ValueType.seed:
                    return "Seed";
                case ValueType.denoise:
                    return "Denoise";
                case ValueType.width:
                    return "Width";
                case ValueType.height:
                    return "Hight";
                case ValueType.batch:
                    return "Batch";
                case ValueType.prompt:
                    return "Prompt";
                case ValueType.color:
                    return "Color";
                case ValueType.lora_count:
                    return "lora_count";
                case ValueType.lora_name_index:
                    return "lora_name_";
                case ValueType.lora_weight:
                    return "lora_wt_";
                case ValueType.imagePath:
                    return "ImagePath";
                default:
                    return "";
            }
        }

        private void SaveJsonIndex(string key, int value)
        {
            if (value == LoadJsonIndex(key))
                return;
            EditorPrefs.SetInt(key, value);

            if (cache.ContainsKey(key))
            {
                cache[key].value = value;
                cache[key].isCache = true;
            }
            else
                cache.Add(key, new cacheValue() { value = value, isCache = true });
        }

        private int LoadJsonIndex(string key)
        {
            if (cache.ContainsKey(key) && cache[key].isCache)
                return (int)cache[key].value;

            return EditorPrefs.GetInt(key, 0);
        }

        private string GetPrefKey(ValueType type, string _node, int _index)
        {
            return _index != -1
                ? $"{nowJsonName}:{_node}:{GetKey(type)}{_index}"
                : $"{nowJsonName}:{_node}:{GetKey(type)}";
        }

        private void UpdateCache<T>(string key, T value)
        {
            if (cache.ContainsKey(key))
            {
                cache[key].value = value;
                cache[key].isCache = true;
            }
            else
            {
                cache.Add(key, new cacheValue() { value = value, isCache = true });
            }
        }

        public void Save<T>(T value, ValueType type, string node, int index = -1)
        {
            var key = GetPrefKey(type, node, index);
            if (Equals(value, Load<T>(type, node, index)))
                return;

            if (typeof(T) == typeof(int))
            {
                EditorPrefs.SetInt(key, Convert.ToInt32(value));
            }
            else if (typeof(T) == typeof(string))
            {
                EditorPrefs.SetString(key, Convert.ToString(value));
            }
            else if (typeof(T) == typeof(long))
            {
                var _value = Convert.ToInt64(value);
                EditorPrefs.SetFloat(key, Convert.ToInt64(value));
            }
            else if (typeof(T) == typeof(float))
            {
                EditorPrefs.SetFloat(key, Convert.ToSingle(value));
            }
            else
            {
                UnityEngine.Debug.Log($"保存失败，未知的类型:{typeof(T)}");
            }

            UpdateCache(key, value);
        }

        public T Load<T>(ValueType type, string node, int index = -1)
        {
            var key = GetPrefKey(type, node, index);
            object value;
            if (cache.ContainsKey(key) && cache[key].isCache)
                return (T)cache[key].value;

            if (typeof(T) == typeof(int))
            {
                value = EditorPrefs.GetInt(key, -1);
                return (T)(object)value;
            }
            else if (typeof(T) == typeof(string))
            {
                value = EditorPrefs.GetString(key, "");
                return (T)(object)value;
            }
            else if (typeof(T) == typeof(long))
            {
                value = (long)EditorPrefs.GetFloat(key, -1);
                return (T)(object)value;
            }
            else if (typeof(T) == typeof(float))
            {
                value = EditorPrefs.GetFloat(key, -1);
                return (T)(object)value;
            }
            else
            {
                UnityEngine.Debug.Log($"读取失败，未知的类型:{typeof(T)}");
                return default;
            }
        }
    }
}
