using UnityEditor;
using UnityEngine;

namespace EveryCine.Editor
{
    public class ECClipInspector
    {
        private static int lastClip = -1;
        public static bool _variableFoldout = true;
        public static string varName = "New Variable Name";

        private struct CreateClipCallbackParam
        {
            public ECClip Clip;
            public ECClipVariable.ECClipVariableType Type;
        }

        private static void CreateClipCallback(object param)
        {
            if (!(param is CreateClipCallbackParam))
            {
                return;
            }

            var par = (CreateClipCallbackParam)param;
            ECClipVariable variable = new ECClipVariable();

            variable.varName = varName;
            variable.varType = par.Type;
            
            par.Clip.variables.Add(variable);
            varName = "New Variable Name";
        }
        
        public static void OnInspect(ECClip clip)
        {
            if (clip.GetInstanceID() != lastClip)
            {
                _variableFoldout = true;
                varName = "New Variable Name";
                lastClip = clip.GetInstanceID();
            }
            
            _variableFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_variableFoldout, "Variables");

            if (_variableFoldout)
            {
                var boldText = new GUIStyle (GUI.skin.label);
                boldText.fontStyle = FontStyle.Bold;
                
                /*
                EditorGUILayout.Foldout(true, new GUIContent("GameObject 1", ECResourcesEditor.prefabIcon), boldText);
                EditorGUILayout.ObjectField(new GUIContent("Prefab"), null, typeof(GameObject), false);
                EditorGUILayout.Toggle("Instantiate if null", true);
                
                EditorGUILayout.Separator();
                
                EditorGUILayout.Foldout(true, new GUIContent("Integer 1", ECResourcesEditor.integerIcon), boldText);
                EditorGUILayout.IntField("Default Value", 0);
                
                EditorGUILayout.Separator();
                */

                for (int i = clip.variables.Count - 1; i >= 0; i--)
                {
                    var variable = clip.variables[i];
                    clip.variables[i] =
                        ECClipVariableInspector.Inspect(variable);
                    EditorGUILayout.Separator();
                }
                
                GUILayout.Label("Create New Variable!", boldText);
                GUILayout.BeginHorizontal();
                varName = GUILayout.TextField(varName);
                if (GUILayout.Button("Create Variable"))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("GameObject"), false, CreateClipCallback, 
                        new CreateClipCallbackParam
                        {
                            Clip = clip,
                            Type = ECClipVariable.ECClipVariableType.GameObject
                        });
                    menu.AddItem(new GUIContent("Integer"), false, CreateClipCallback, new CreateClipCallbackParam
                    {
                        Clip = clip,
                        Type = ECClipVariable.ECClipVariableType.Integer
                    });
                    menu.AddItem(new GUIContent("Float"), false, CreateClipCallback, new CreateClipCallbackParam
                    {
                        Clip = clip,
                        Type = ECClipVariable.ECClipVariableType.Float
                    });
                    
                    menu.ShowAsContext();
                }
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}