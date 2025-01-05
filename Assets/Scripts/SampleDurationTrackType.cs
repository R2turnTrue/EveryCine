using EveryCine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Keyframe = EveryCine.Keyframe;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class SampleDurationTrackType : ECTrackType
{
#if UNITY_EDITOR
    static SampleDurationTrackType()
    {
        ECTypes.AddTrackType("SampleDurationTrackType", new SampleDurationTrackType());
    }
#endif
    
    [RuntimeInitializeOnLoadMethod]
    public static void Initialize()
    {
        ECTypes.AddTrackType("SampleDurationTrackType", new SampleDurationTrackType());
    }
    
    public override bool CompareWatchTrack(ECTrackWatchState previous, ECClipTrack track)
    {
        return false;
    }

    public override ECTrackWatchState CreateNewWatchState(ECClipTrack track, ECShowable show)
    {
        return new ECTrackWatchState();
    }

    public override string BuildWatchKeyframeData(ECClipTrack track, ECTrackWatchState state)
    {
        return "";
    }

    public override string Interpolate(ECCinemable cinema, Keyframe before, Keyframe after, double time)
    {
        var distanceInTime = (before.end - before.start) * ECConstants.SecondPerFrame;
        var timeAmount = (float) (time - (before.start * ECConstants.SecondPerFrame));
        var factor = timeAmount / distanceInTime;

        var start = float.Parse(before.data.Split('/')[0]);
        var end = float.Parse(before.data.Split('/')[1]);

        return $"{start}/{end}/{start + ((end - start) * factor)}";
    }

    public override bool RuntimePlay(ECCinemable cinema, ECClipTrack track, ECShowable show, string interpolated)
    {
        //Debug.Log($"Im playgin!! --> {interpolated}");
        var go = show.GetGameObject(track.go_variableName);
        var data = float.Parse(interpolated.Split('/')[2]);
        go.transform.localScale = new Vector3(data, data, data);
        return true;
    }

    public override Keyframe CreateEmptyKeyframe(ECClipTrack track, int start, int end)
    {
        return new Keyframe
        {
            start = start,
            end = end,
            data = "1.0/0.0/1.0",
            track = track,
            startEndTogether = !track.hasDuration
        };
    }

    public override bool HasDuration()
    {
        return true;
    }
}