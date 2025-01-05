using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace EveryCine.Editor
{
    public class ECClipTrackInspector
    {
        private static ECClipTrack lastTrack;
        private static int _variableId;
        
        public static ECClipTrack OnInspect(ECClipTrack track)
        {
            if (lastTrack != track)
            {
                //Debug.Log("New Track Inspected");
                _variableId = Array.IndexOf(track.clip.variables
                    .Where((it) => it.varType == ECClipVariable.ECClipVariableType.GameObject)
                    .Select((it) => it.varName)
                    .ToArray(), track.go_variableName);
                lastTrack = track;
            }
            
            track.trackName = EditorGUILayout.TextField("Track Name", track.trackName);

            var trackType = ECTypes.GetTrackType(track.typeStr);
            var inspector = ECEditorTypes.GetTrackInspector(trackType.GetType());
            if (inspector != null)
            {
                inspector.Inspect(track);
            }
            
            if (GUILayout.Button("Delete Track"))
            {
                return null;
            }
            
            if (GUILayout.Button("Add keyframe at current frame"))
            {
                var type = ECTypes.GetTrackType(track.typeStr);
                var f = Mathf.RoundToInt((float)ECClipEditor.Instance.RunningTime / ECConstants.SecondPerFrame);
                var f_end = Mathf.RoundToInt((float)ECClipEditor.Instance.RunningTime / ECConstants.SecondPerFrame);

                if (track.hasDuration)
                {
                    f_end += 50;
                }
                
                var kf = type.CreateEmptyKeyframe(track, f, f_end);
                track.keyframes.Add(kf);
                
                track.keyframes =
                    track.keyframes.OrderBy(it => it.start)
                        .ToList();
            }
            return track;
        }
    }
}