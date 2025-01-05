using UnityEngine;

namespace EveryCine
{
    public interface ECShowable
    {
        public GameObject GetGameObject(string varName);

        public int GetInt(string varName);

        public float GetFloat(string varName);
    }
}