using EveryCine;
using EveryCine.Editor;
using UnityEditor;
using UnityEngine;
using Keyframe = EveryCine.Keyframe;

namespace Editor
{
    [InitializeOnLoad]
    public class SampleTrackTypeInspector : ECTrackTypeInspector
    {
        private static Texture2D icon;
        
        static SampleTrackTypeInspector()
        {
            Debug.Log("Registering Inspector Type");
            ECEditorTypes.AddTrackInspector(typeof(SampleTrackType), new SampleTrackTypeInspector());

            icon = EditorGUIUtility.FindTexture("d_Transform Icon");
        }
        
        public override void Inspect(ECClipTrack track)
        {
            GUILayout.Label("Hello, World from Sample Track Type!");
        }

        public override void InspectKeyframe(Keyframe kf)
        {
            GUILayout.Label("Hello, World! Keyframe!");
        }

        public override Texture2D Icon()
        {
            return icon;
        }
    }
}