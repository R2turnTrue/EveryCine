using UnityEngine;

namespace EveryCine
{
    [System.Serializable]
    public class ECClipVariable
    {
        public ECClipVariableType varType;
        public string varName;

        public GameObject go_defaultPrefab;
        public bool go_instantiateIfNull;
        public bool go_destroyWhenEnd;

        public int in_defaultValue;
        
        public float fl_defaultValue;
        
        public enum ECClipVariableType
        {
            GameObject,
            Integer,
            Float
        }
    }
}