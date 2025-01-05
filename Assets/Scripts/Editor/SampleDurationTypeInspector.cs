using System;
using System.Linq;
using EveryCine;
using EveryCine.Editor;
using UnityEditor;
using UnityEngine;
using Keyframe = EveryCine.Keyframe;

namespace Editor
{
    [InitializeOnLoad]
    public class SampleDurationTypeInspector : ECTrackTypeInspector
    {
        private static Texture2D icon;
        
        static SampleDurationTypeInspector()
        {
            ECEditorTypes.AddTrackInspector(typeof(SampleDurationTrackType), new SampleDurationTypeInspector());

            icon = EditorGUIUtility.FindTexture("d_Transform Icon");
        }
        
        private int _variableId = 0;
        private ECClipTrack _lastInspected = null;
        
        public override void Inspect(ECClipTrack track)
        {
            GUILayout.Label("Hello, Track! (Duration)");
            
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
            var start = float.Parse(kf.data.Split('/')[0]);
            var end = float.Parse(kf.data.Split('/')[1]);

            start = EditorGUILayout.FloatField("Start Scale", start);
            end = EditorGUILayout.FloatField("End Scale", end);

            kf.data = $"{start}/{end}/{start}";
        }

        public override Texture2D Icon()
        {
            return icon;
        }
    }
}