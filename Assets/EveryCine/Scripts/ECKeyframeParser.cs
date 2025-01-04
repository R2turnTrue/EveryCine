using System;
using UnityEngine;

namespace EveryCine
{
    public class ECKeyframeParser
    {
        public static (Vector3, Vector3, Vector3) ParseTransform(string data)
        {
            var splited = data.Split('/');
            return (
                new Vector3(float.Parse(splited[0]), float.Parse(splited[1]), float.Parse(splited[2])),
                new Vector3(float.Parse(splited[3]), float.Parse(splited[4]), float.Parse(splited[5])),
                new Vector3(float.Parse(splited[6]), float.Parse(splited[7]), float.Parse(splited[8])));
        }
        
        public static string MakeTransform(Vector3 pos, Vector3 rot, Vector3 sca)
        {
            return $"{pos.x}/{pos.y}/{pos.z}/" +
                   $"{rot.x}/{rot.y}/{rot.z}/" +
                   $"{sca.x}/{sca.y}/{sca.z}";
        }

        public static string LerpTransform(AnimationCurve curve, string before, string after, float a)
        {
            var bef = ParseTransform(before);
            var aft = ParseTransform(after);
            
            //Debug.Log($"Lerping: {bef.Item1} -> {aft.Item1} ({a})");

            var resPos = Vector3.Lerp(bef.Item1, aft.Item1, curve.Evaluate(a));
            var resRot = Vector3.Lerp(bef.Item2, aft.Item2, curve.Evaluate(a));
            var resSca = Vector3.Lerp(bef.Item3, aft.Item3, curve.Evaluate(a));

            return MakeTransform(resPos, resRot, resSca);
        }
    }
}