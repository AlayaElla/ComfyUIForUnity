using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ComfyUIForUnity
{
    [InitializeOnLoad]
    public class UIAIGeneratorEditor
    {
        static UIAIGeneratorEditor()
        {
            Selection.selectionChanged += OnSelectionChanged;
        }

        private static void OnSelectionChanged()
        {
            GameObject selectedObject = Selection.activeGameObject;

            if (selectedObject != null && UIAIGenerator.IsWindowOpen())
            {
                Image imageComponent = selectedObject.GetComponent<Image>();
                if (imageComponent != null)
                {
                    UIAIGenerator window = (UIAIGenerator)EditorWindow.GetWindow(typeof(UIAIGenerator), false, "UIÉú³ÉÆ÷");
                    if (window != null)
                    {
                        window.SetOriginalUI(imageComponent);
                    }
                }
            }
        }
    }
}
