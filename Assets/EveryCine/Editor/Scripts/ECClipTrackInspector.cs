using System;
using System.Linq;
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

            if (track.type == ECTrackType.GameObject)
            {
                _variableId = ECEditorUtils.VariablePopup("Ref Variable", _variableId, track.clip,
                    ECClipVariable.ECClipVariableType.GameObject);
                track.go_variableName = track.clip.variables
                    .Where((it) => it.varType == ECClipVariable.ECClipVariableType.GameObject)
                    .ToArray()[_variableId].varName;
            }
            
            if (GUILayout.Button("Delete Track"))
            {
                return null;
            }
            return track;
        }
    }
}