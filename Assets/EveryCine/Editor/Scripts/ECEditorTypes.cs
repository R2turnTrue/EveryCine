using System;
using System.Collections.Generic;
using UnityEditor;

namespace EveryCine.Editor
{
    public static class ECEditorTypes
    {
        private static Dictionary<Type, ECTrackTypeInspector> trackInspectors = new();

        public static void AddTrackInspector(Type trackTypeT, ECTrackTypeInspector inspector)
        {
            trackInspectors[trackTypeT] = inspector;
        }

        public static ECTrackTypeInspector GetTrackInspector(Type trackTypeT)
        {
            return trackInspectors[trackTypeT];
        }
    }
}