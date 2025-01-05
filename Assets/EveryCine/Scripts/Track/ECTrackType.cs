namespace EveryCine
{
    public abstract class ECTrackType
    {
        public abstract bool CompareWatchTrack(ECTrackWatchState previous, ECClipTrack track);

        public abstract ECTrackWatchState CreateNewWatchState(ECClipTrack track, ECShowable show);

        public abstract string BuildWatchKeyframeData(ECClipTrack track, ECTrackWatchState state);
        
        public abstract string Interpolate(ECCinemable cinema, Keyframe before, Keyframe after, double time);

        public abstract bool RuntimePlay(ECCinemable cinema, ECClipTrack track, ECShowable show, string interpolated);

        public abstract Keyframe CreateEmptyKeyframe(ECClipTrack track, int start, int end);

        public virtual bool IsSingleFrame()
        {
            return false;
        }
        
        public virtual bool HasDuration()
        {
            return false;
        }
    }
}