using System;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Net;

public class ServerInfo
{
    private string serverAddress = "127.0.0.1:8188";
    private bool isGetInfo = false;
    private string[] ckpt_list;
    private string[] vae_list;
    private string[] lora_list;

    private string ResponseString = "";

    public ServerInfo(string serverAddress)
    {
        this.serverAddress = serverAddress;
    }

    public bool IsGetInfo()
    {
        return isGetInfo;
    }

    public void GetServerInfo()
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(StartGetServerInfo());
    }

    public string[] GetCkptList()
    {
        return ckpt_list;
    }

    public int GetCkptIndex(string name) {
        for (int i = 0; i < ckpt_list.Length; i++)
        {
            if (ckpt_list[i] == name)
            {
                return i;
            }
        }
        return 0;
    }

    public string[] GetVaeList()
    {
        return vae_list;
    }
    public int GetVaeIndex(string name)
    {
        for (int i = 0; i < vae_list.Length; i++)
        {
            if (vae_list[i] == name)
            {
                return i;
            }
        }
        return 0;
    }

    public string[] GetLoraList()
    {
        return lora_list;
    }

    public int GetLoraIndex(string name)
    {
        for (int i = 0; i < lora_list.Length; i++)
        {
            if (lora_list[i] == name)
            {
                return i;
            }
        }
        return 0;
    }

    private IEnumerator StartGetServerInfo()
    {
        isGetInfo = false;

        //读取Checkpoint信息
        yield return GetObjectInfo("CheckpointLoaderSimple");
        var list = JObject.Parse(ResponseString)["CheckpointLoaderSimple"]["input"]["required"]["ckpt_name"];
        ckpt_list = list[0].ToObject<string[]>();

        //读取Vae信息
        yield return GetObjectInfo("VAELoader");
        list = JObject.Parse(ResponseString)["VAELoader"]["input"]["required"]["vae_name"];
        vae_list = list[0].ToObject<string[]>();

        //读取Lora信息
        yield return GetObjectInfo("LoraLoader");
        list = JObject.Parse(ResponseString)["LoraLoader"]["input"]["required"]["lora_name"];
        lora_list = list[0].ToObject<string[]>();

        isGetInfo = true;
    }

    private IEnumerator GetObjectInfo(string infopath)
    {
        //获取Checkpoint：http://127.0.0.1:8188/object_info/CheckpointLoaderSimple
        //返回：{"CheckpointLoaderSimple": {"input": {"required": {"ckpt_name": [["AnythingV5Ink_ink.safetensors", "CounterfeitV25_25.safetensors", "chilloutmix_NiPrunedFp32Fix.safetensors", "dreamshaperXL_turboDpmppSDE.safetensors", "ghostmix_v20Bakedvae.safetensors", "hassakuHentaiModel_v12.safetensors", "helloyoung25d_V10f.safetensors", "henmixReal_v30.safetensors", "landscapesupermix_v21.safetensors", "majicmixFantasy_v30Vae.safetensors", "model.safetensors", "notRealAnimeStyle_v141.safetensors", "pastelMixStylizedAnime_pastelMixPrunedFP16.safetensors", "perfectdeliberate_v5.safetensors", "toonify_v10.safetensors"]]}}, "output": ["MODEL", "CLIP", "VAE"], "output_is_list": [false, false, false], "output_name": ["MODEL", "CLIP", "VAE"], "name": "CheckpointLoaderSimple", "display_name": "Load Checkpoint", "description": "", "category": "loaders", "output_node": false}}

        //获取Checkpoint
        var url = $"http://{serverAddress}/object_info/{infopath}";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(url + "获取信息错误: " + webRequest.error);
                yield return null;
            }
            else
            {
                var responseString = webRequest.downloadHandler.text;
                if (string.IsNullOrEmpty(responseString))
                {
                    Debug.LogError(url + "响应为空");
                    yield return null;
                }
                else
                {
                    ResponseString = responseString;
                }
            }
        }
    }
}