using UnityEngine;

namespace EveryCine.Editor
{
    public struct ECObjectWatchState
    {
        public GameObject target;
        public Vector3 lastPos;
        public Vector3 lastRot;
        public Vector3 lastSca;
        public string varName;
    }
}