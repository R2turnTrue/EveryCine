using UnityEngine;

namespace EveryCine
{
    [System.Serializable]
    public class Keyframe : ECInspectable
    {
        public int start;
        public int end;
        public bool startEndTogether;
        public string data;
        public string type;
        public AnimationCurve curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
        
        public string EveryCineInspectName()
        {
            return $"Kf {start}~{end}";
        }

        public string EveryCineInspectType()
        {
            return "Keyframe";
        }
    }
}