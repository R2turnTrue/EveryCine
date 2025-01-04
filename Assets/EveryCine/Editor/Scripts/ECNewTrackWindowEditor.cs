using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EveryCine.Editor
{
    public class ECNewTrackWindowEditor : EditorWindow
    {
        private ECTrackType _selectedType = ECTrackType.GameObject;

        private string trackName = "";

        private int _go_variableIdx = 0;

        public ECClip clip;
        
        private void OnGUI()
        {
            _selectedType = (ECTrackType)EditorGUILayout.EnumPopup(new GUIContent("Track Type"), _selectedType);

            GUILayout.BeginVertical(GUI.skin.window);

            trackName = EditorGUILayout.TextField("Track Name", trackName);
            switch (_selectedType)
            {
                case ECTrackType.GameObject:
                    _go_variableIdx = ECEditorUtils.VariablePopup("Ref Variable", _go_variableIdx, clip,
                        ECClipVariable.ECClipVariableType.GameObject);
                    break;
                case ECTrackType.MainCamera:
                default:
                    break;
            }
            
            GUILayout.EndVertical();
            
            if (GUILayout.Button("Create!"))
            {
                var variable = clip.variables
                    .Where((it) => it.varType == ECClipVariable.ECClipVariableType.GameObject)
                    .ToArray()[_go_variableIdx];

                ECClipTrack track = new ECClipTrack();

                track.clip = clip;
                
                track.type = _selectedType;

                if (_selectedType == ECTrackType.GameObject)
                {
                    track.go_variableName = variable.varName;
                }

                track.trackName = trackName;
                
                clip.tracks.Add(track);

                trackName = "";
                _go_variableIdx = 0;
                
                Close();
            }
        }
    }
}