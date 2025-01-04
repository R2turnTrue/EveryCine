using System.Collections.Generic;
using UnityEngine;

namespace EveryCine
{
    [System.Serializable]
    public class ECClipTrack : ECInspectable
    {
        public ECClip clip;
        public ECTrackType type;
        public bool hasDuration;
        public string trackName;

        public List<Keyframe> keyframes = new();

        public int FindKeyframeIdx(int frame)
        {
            for (int i = keyframes.Count - 1; i >= 0; i--)
            {
                var kf = keyframes[i];
                if (kf.start >= frame && kf.end <= frame)
                {
                    return i;
                }
            }

            return -1;
        }

        public string go_variableName;
        
        public string EveryCineInspectName()
        {
            return trackName;
        }

        public string EveryCineInspectType()
        {
            return "Track";
        }

        public string Seek(double time)
        {
            int f = Mathf.RoundToInt((float)time / ECConstants.SecondPerFrame);

            Keyframe before = null;
            Keyframe after = null;

            for (int i = 0; i < keyframes.Count; i++)
            {
                var keyframe = keyframes[i];
                if (f >= keyframe.start)
                {
                    before = keyframe;
                    if (keyframes.Count - 1 >= i + 1)
                    {
                        after = keyframes[i + 1];
                    }
                }
            }

            if (before == null)
            {
                Debug.Log("(null because before null)");
                return null;
            }

            if (after == null)
            {
                return before.data;
            }

            if (before.type == "transform")
            {
                var distanceInTime = (after.start - before.start) * ECConstants.SecondPerFrame;
                var timeAmount = (float) (time - (before.start * ECConstants.SecondPerFrame));
                
                return ECKeyframeParser.LerpTransform(before.curve, before.data, after.data,
                     timeAmount / distanceInTime);
            }
            
            Debug.Log("(null because unknown type)");

            return null;
        }
    }
}