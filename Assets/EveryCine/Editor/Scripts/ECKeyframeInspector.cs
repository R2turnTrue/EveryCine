using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EveryCine.Editor
{
    public class ECKeyframeInspector
    {
        public static void Inspect(Keyframe keyframe)
        {
            if (keyframe.startEndTogether)
            {
                keyframe.start = keyframe.end = EditorGUILayout.IntField("Frame", keyframe.start);
            }
            else
            {
                keyframe.start = EditorGUILayout.IntField("Start Frame", keyframe.start);
                keyframe.end = EditorGUILayout.IntField("End Frame", keyframe.end);

                if (keyframe.end <= keyframe.start)
                {
                    keyframe.end = keyframe.start + 1;
                }
            }

            keyframe.curve = EditorGUILayout.CurveField("Curve", keyframe.curve);

            var trackType = ECTypes.GetTrackType(keyframe.track.typeStr);
            var trackInspector = ECEditorTypes.GetTrackInspector(trackType.GetType());

            if (trackInspector != null)
            {
                trackInspector.InspectKeyframe(keyframe);
            } else
            {
                GUILayout.Label("Unknown Type: " + keyframe.track.typeStr);
                keyframe.data = EditorGUILayout.TextField("Raw Data", keyframe.data);
            }

            if (GUILayout.Button("Delete Keyframe"))
            {
                Debug.Log($"Bef: {keyframe.track.keyframes.Count}");
                var track = keyframe.track;
                EditorUtility.SetDirty(track.clip);
                var idx = track.FindKeyframeIdxWithStart(keyframe.start);
                track.keyframes.RemoveAt(idx);
                Debug.Log($"Aft: {keyframe.track.keyframes.Count}");
                ECInspectorWindow.InspectSomething(track.clip);
            }
        }
    }
}