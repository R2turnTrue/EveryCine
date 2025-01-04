using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EveryCine
{
    [CreateAssetMenu(fileName = "Clip", menuName = "EveryCine/Clip", order = 1)]
    public class ECClip : ScriptableObject, ECInspectable
    {
        [SerializeField] public List<ECClipVariable> variables;
        [SerializeField] public List<ECClipTrack> tracks;
        
        public string EveryCineInspectName()
        {
            return name;
        }

        public string EveryCineInspectType()
        {
            return "Clip";
        }

        public ECClipVariable GetVariableDefinitionByName(string name)
        {
            foreach (var variable in variables)
            {
                if (variable.varName == name)
                {
                    return variable;
                }
            }

            return null;
        }
    }
}