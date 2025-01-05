using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace EveryCine
{
    [System.Serializable]
    public class ECClipTrack : ECInspectable
    {
        public ECClip clip;
        public string typeStr;
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
        
        public int FindKeyframeIdxWithStart(int start)
        {
            for (int i = keyframes.Count - 1; i >= 0; i--)
            {
                var kf = keyframes[i];
                if (kf.start == start)
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

        public string Seek(ECCinemable cinema, double time)
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
                //Debug.Log("(null because before null)");
                return null;
            }
            
            var trackType = ECTypes.GetTrackType(before.track.typeStr);
            
            if (trackType.IsSingleFrame() && f != before.start)
            {
                return null;
            }

            if (trackType.HasDuration() && f >= before.end)
            {
                return null;
            }

            if (before.start == before.end && after == null)
            {
                return before.data;
            }
            
            return trackType.Interpolate(cinema, before, after, time);
        }
    }
}