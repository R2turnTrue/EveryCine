using EveryCine;
using UnityEditor;
using UnityEngine;
using Keyframe = EveryCine.Keyframe;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class SampleTrackType : ECTrackType
{
#if UNITY_EDITOR
    static SampleTrackType()
    {
        ECTypes.AddTrackType("SampleTrackType", new SampleTrackType());
    }
#endif

    [RuntimeInitializeOnLoadMethod]
    public static void Initialize()
    {
        ECTypes.AddTrackType("SampleTrackType", new SampleTrackType());
    }

    public override bool CompareWatchTrack(ECTrackWatchState previous, ECClipTrack track)
    {
        return false;
    }

    public override ECTrackWatchState CreateNewWatchState(ECClipTrack track, ECShowable show)
    {
        return new ECTrackWatchState
        {
            track = track
        };
    }

    public override string BuildWatchKeyframeData(ECClipTrack track, ECTrackWatchState state)
    {
        return "";
    }

    public override string Interpolate(ECCinemable cinema, Keyframe before, Keyframe after, double time)
    {
        return $"";
    }

    public override bool RuntimePlay(ECCinemable cinema, ECClipTrack track, ECShowable show, string interpolated)
    {
        Debug.Log("Playing");
        
        if (Application.isPlaying)
        {
            cinema.Pause();

            TestScript.Instance.ResumeAfter(cinema, 2.0f);
        }
        
        cinema.AddTime(ECConstants.SecondPerFrame);

        return true;
    }

    public override Keyframe CreateEmptyKeyframe(ECClipTrack track, int start, int end)
    {
        return new Keyframe
        {
            start = start,
            end = end,
            data = "",
            track = track,
            startEndTogether = !track.hasDuration
        };
    }

    public override bool IsSingleFrame()
    {
        return true;
    }
}