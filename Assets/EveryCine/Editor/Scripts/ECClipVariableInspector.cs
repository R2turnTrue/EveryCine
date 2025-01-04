using UnityEditor;
using UnityEngine;

namespace EveryCine.Editor
{
    public class ECClipVariableInspector
    {
        public static ECClipVariable Inspect(ECClipVariable variable)
        {
            var boldText = new GUIStyle (GUI.skin.label);
            boldText.fontStyle = FontStyle.Bold;

            Texture icon;

            switch (variable.varType)
            {
                case ECClipVariable.ECClipVariableType.GameObject:
                    icon = ECResourcesEditor.prefabIcon;
                    break;
                case ECClipVariable.ECClipVariableType.Integer:
                case ECClipVariable.ECClipVariableType.Float:
                default:
                    icon = ECResourcesEditor.integerIcon;
                    break;
            }
            
            EditorGUILayout.Foldout(true, new GUIContent(variable.varName, icon), boldText);

            if (variable.varType == ECClipVariable.ECClipVariableType.GameObject)
            {
                variable.go_defaultPrefab =
                    (GameObject) EditorGUILayout.ObjectField(
                        new GUIContent("Default Prefab"),
                        variable.go_defaultPrefab,
                        typeof(GameObject),
                        false);
                variable.go_instantiateIfNull =
                    EditorGUILayout.Toggle("Instantiate If Null",
                        variable.go_instantiateIfNull);
                variable.go_destroyWhenEnd =
                    EditorGUILayout.Toggle("Destroy When End",
                        variable.go_destroyWhenEnd);
            } else if (variable.varType == ECClipVariable.ECClipVariableType.Integer)
            {
                variable.in_defaultValue = EditorGUILayout.IntField("Default Value",
                    variable.in_defaultValue);
            } else if (variable.varType == ECClipVariable.ECClipVariableType.Float)
            {
                variable.fl_defaultValue = EditorGUILayout.FloatField("Default Value",
                    variable.fl_defaultValue);
            }
            else
            {
                GUILayout.Label("Unknown Var Type: " + variable.varType);
            }
            
            return variable;
        }
    }
}