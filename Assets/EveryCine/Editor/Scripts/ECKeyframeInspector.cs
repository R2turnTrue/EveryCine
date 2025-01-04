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

            if (keyframe.type == "transform")
            {
                var parsed = ECKeyframeParser.ParseTransform(keyframe.data);
                parsed.Item1 = EditorGUILayout.Vector3Field("Position", parsed.Item1);
                parsed.Item2 = EditorGUILayout.Vector3Field("Rotation", parsed.Item2);
                parsed.Item3 = EditorGUILayout.Vector3Field("Scale", parsed.Item3);
                keyframe.data = ECKeyframeParser.MakeTransform(parsed.Item1, parsed.Item2, parsed.Item3);
            }
            else
            {
                GUILayout.Label("Unknown Type: " + keyframe.type);
                keyframe.data = EditorGUILayout.TextField("Raw Data", keyframe.data);
            }
        }
    }
}