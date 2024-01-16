using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AIToolConfig", menuName = "ScriptableObjects/AIToolConfig")]
[Serializable]
public class AIToolConfig : ScriptableObject
{
    public string Name = "ComfyUIForUnity";
    public string Address = "127.0.0.1:8188";
}