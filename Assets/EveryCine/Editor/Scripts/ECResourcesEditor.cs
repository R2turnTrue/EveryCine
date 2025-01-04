using UnityEditor;
using UnityEngine;

namespace EveryCine.Editor
{
    [InitializeOnLoad]
    public class ECResourcesEditor
    {
        public static Texture ecIcon;

        public static Texture2D prefabIcon;
        public static Texture2D cameraIcon;
        public static Texture2D recordIcon;
        
        public static Texture2D integerIcon;
        
        static ECResourcesEditor()
        {
            Debug.Log("Loading assets...");
            ecIcon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/EveryCine/Editor/Sprites/EC_Icon.png");
            prefabIcon = EditorGUIUtility.FindTexture("Prefab Icon");
            cameraIcon = EditorGUIUtility.FindTexture("Camera Icon");
            integerIcon = EditorGUIUtility.FindTexture("GridAxisX");
            recordIcon = EditorGUIUtility.FindTexture("Animation.Record");
        }
    }
}