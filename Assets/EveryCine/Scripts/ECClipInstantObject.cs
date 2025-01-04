using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ECClipInstantObject : MonoBehaviour
{
#if UNITY_EDITOR
    private static Texture2D iconTexture;
    
    [UnityEditor.InitializeOnLoadMethod]
    private static void ApplyHierarchyIcon()
    {
        if (iconTexture == null)
        {
            iconTexture = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/EveryCine/Editor/Sprites/EC_InstantObj.png", typeof(Texture2D)) as Texture2D;
        }
        
        UnityEditor.EditorApplication.hierarchyWindowItemOnGUI += DrawHierarchyIcon;
    }
    
    static void DrawHierarchyIcon(int instanceID, Rect selectionRect)
    {
        const float Pos =
#if UNITY_2019_3_OR_NEWER      
            32f;
#else
            0f
#endif

        Rect iconRect = new Rect(selectionRect);
        iconRect.x = Pos;
        iconRect.width = 16f;

        GameObject go = UnityEditor.EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (go != null && go.GetComponent<ECClipInstantObject>() != null)
        {
            GUI.DrawTexture(iconRect, iconTexture);
        }
    }
    
#endif
}
