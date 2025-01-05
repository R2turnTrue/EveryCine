using UnityEngine;

namespace EveryCine.Editor
{
    public abstract class ECTrackTypeInspector
    {
        public abstract void Inspect(ECClipTrack track);

        public abstract void InspectKeyframe(Keyframe kf);

        public abstract Texture2D Icon();
    }
}