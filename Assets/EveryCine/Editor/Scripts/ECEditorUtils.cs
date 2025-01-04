using System.Linq;
using UnityEditor;

namespace EveryCine.Editor
{
    public class ECEditorUtils
    {
        public static int VariablePopup(string label, int cur, ECClip clip, ECClipVariable.ECClipVariableType type)
        {
            return EditorGUILayout.Popup(label,
                cur,
                clip.variables
                    .Where((it) => it.varType == type)
                    .Select((it) => it.varName)
                    .ToArray());
        }
    }
}