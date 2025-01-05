using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EveryCine.Editor.Track
{
    [InitializeOnLoad]
    public class ECTransformTrackTypeInspector : ECTrackTypeInspector
    {
        private static Texture2D icon;
        
        static ECTransformTrackTypeInspector()
        {
            Debug.Log("Registering Inspector Type");
            ECEditorTypes.AddTrackInspector(typeof(ECTransformTrackType), new ECTransformTrackTypeInspector());

            icon = EditorGUIUtility.FindTexture("d_Transform Icon");
        }

        private int _variableId = 0;
        private ECClipTrack _lastInspected = null;
        
        public override void Inspect(ECClipTrack track)
        {
            var arr = track.clip.variables
                .Where((it) => it.varType == ECClipVariable.ECClipVariableType.GameObject)
                .ToArray();
            
            if (_lastInspected != track)
            {
                _variableId = Array.IndexOf(arr
                    .Select(it => it.varName).ToArray(), track.go_variableName);
                _lastInspected = track;
            }
            
            _variableId = ECEditorUtils.VariablePopup("Ref Variable", _variableId, track.clip,
                ECClipVariable.ECClipVariableType.GameObject);
            track.go_variableName = arr[_variableId].varName;
        }

        public override void InspectKeyframe(Keyframe kf)
        {
            var parsed = ECKeyframeParser.ParseTransform(kf.data);
            parsed.Item1 = EditorGUILayout.Vector3Field("Position", parsed.Item1);
            parsed.Item2 = EditorGUILayout.Vector3Field("Rotation", parsed.Item2);
            parsed.Item3 = EditorGUILayout.Vector3Field("Scale", parsed.Item3);
            kf.data = ECKeyframeParser.MakeTransform(parsed.Item1, parsed.Item2, parsed.Item3);
        }

        public override Texture2D Icon()
        {
            return icon;
        }
    }
}