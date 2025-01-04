using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace EveryCine.Editor
{
    [CustomEditor(typeof(ECClip))]
    public class ECClipSOEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Open in Clip Editor"))
            {
                ECClipEditor.OpenClipEditor((ECClip)target);
            }
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            Object target = EditorUtility.InstanceIDToObject(instanceID);

            if (target is ECClip)
            {
                ECClipEditor.OpenClipEditor((ECClip)target);
                return true;
            }

            return false;
        }
    }
}