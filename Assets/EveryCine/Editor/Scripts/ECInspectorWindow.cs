using System;
using UnityEditor;
using UnityEngine;

namespace EveryCine.Editor
{
    public class ECInspectorWindow : EditorWindow
    {
        private bool _variableFoldout = true;

        public static ECInspectable currentInspecting;

        public static void InspectSomething(ECInspectable inspectable)
        {
            var inspector = GetWindow<ECInspectorWindow>(false, "");
            inspector.titleContent = new GUIContent("EveryCine Inspector", ECResourcesEditor.ecIcon);
            inspector.minSize = new Vector3(400f, 200f);
            currentInspecting = inspectable;
            inspector.Show();
        }
        
        private void OnGUI()
        {
            if (currentInspecting == null || currentInspecting.ToString() == "null")
            {
                GUILayout.Label("Nothing to inspect :(");
                return;
            }
            
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(ECResourcesEditor.ecIcon, GUILayout.Width(48), GUILayout.Height(48));
            EditorGUILayout.BeginVertical(GUILayout.Height(48));
            var boldText = new GUIStyle (GUI.skin.label);
            boldText.fontStyle = FontStyle.Bold;
            boldText.fontSize = 20;
            GUILayout.Label(currentInspecting.EveryCineInspectName(), boldText);
            GUILayout.Label("Type: " + currentInspecting.EveryCineInspectType());
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();

            if (currentInspecting is ECClip)
            {
                EditorUtility.SetDirty((ECClip)currentInspecting);
                ECClipInspector.OnInspect((ECClip)currentInspecting);
            }

            if (currentInspecting is ECClipTrack)
            {
                var track = (ECClipTrack)currentInspecting;
                var res = ECClipTrackInspector.OnInspect(track);

                if (res == null)
                {
                    track.clip.tracks.Remove(track);
                    InspectSomething(track.clip);
                }
            }

            if (currentInspecting is Keyframe)
            {
                ECKeyframeInspector.Inspect((Keyframe) currentInspecting);
            }
        }
    }
}