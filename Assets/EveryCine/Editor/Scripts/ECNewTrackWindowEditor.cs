using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EveryCine.Editor
{
    public class ECNewTrackWindowEditor : EditorWindow
    {
        private int _selectedType = 0;

        private string trackName = "";

        private int _go_variableIdx = 0;

        public ECClip clip;
        
        private void OnGUI()
        {
            _selectedType = EditorGUILayout.Popup(new GUIContent("Track Type"), _selectedType,
                ECTypes.trackTypes.Keys.ToArray());

            GUILayout.BeginVertical(GUI.skin.window);

            trackName = EditorGUILayout.TextField("Track Name", trackName);

            /*
            switch (_selectedType)
            {
                case ECTrackType_Old.Transform:
                    _go_variableIdx = ECEditorUtils.VariablePopup("Ref Variable", _go_variableIdx, clip,
                        ECClipVariable.ECClipVariableType.GameObject);
                    break;
                case ECTrackType_Old.MainCamera:
                default:
                    break;
            }
            */
            
            GUILayout.EndVertical();
            
            if (GUILayout.Button("Create!"))
            {
                var variable = clip.variables
                    .Where((it) => it.varType == ECClipVariable.ECClipVariableType.GameObject)
                    .ToArray()[_go_variableIdx];

                ECClipTrack track = new ECClipTrack();

                track.clip = clip;

                var typeName = ECTypes.trackTypes.Keys.ToArray()[_selectedType];
                var trackType = ECTypes.GetTrackType(typeName);
                track.typeStr = typeName;

                track.hasDuration = trackType.HasDuration();

                track.trackName = trackName;
                
                clip.tracks.Add(track);

                trackName = "";
                _go_variableIdx = 0;
                
                Close();
            }
        }
    }
}