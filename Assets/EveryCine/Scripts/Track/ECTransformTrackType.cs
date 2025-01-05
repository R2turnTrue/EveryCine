using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EveryCine
{
    #if UNITY_EDITOR
    [InitializeOnLoad]
    #endif
    public class ECTransformTrackType : ECTrackType
    {
        #if UNITY_EDITOR
        static ECTransformTrackType()
        {
            Debug.Log("Registering Type");
            ECTypes.AddTrackType("ECTransformTrackType", new ECTransformTrackType());
        }
        #endif
        
        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            Debug.Log("Registering Type");
            ECTypes.AddTrackType("ECTransformTrackType", new ECTransformTrackType());
        }
        
        public class TransformWatchState : ECTrackWatchState
        {
            public GameObject target;
            public Vector3 lastPos;
            public Vector3 lastRot;
            public Vector3 lastSca;
        }

        public override bool CompareWatchTrack(ECTrackWatchState previous, ECClipTrack track)
        {
            var state = (TransformWatchState)previous;
            var target = state.target;
            var targetTrans = target.transform;
            
            if (targetTrans.localPosition == state.lastPos && 
                targetTrans.localRotation.eulerAngles == state.lastRot &&
                targetTrans.localScale == state.lastSca)
            {
                return false;
            }

            return true;
        }

        public override ECTrackWatchState CreateNewWatchState(ECClipTrack track, ECShowable show)
        {
            var target = show.GetGameObject(track.go_variableName);
            return new TransformWatchState
            {
                lastPos = target.transform.localPosition,
                lastRot = target.transform.localRotation.eulerAngles,
                lastSca = target.transform.localScale,
                target = target,
                track = track
            };
        }

        public override string BuildWatchKeyframeData(ECClipTrack track, ECTrackWatchState state)
        {
            var st = (TransformWatchState)state;
            
            return ECKeyframeParser.MakeTransform(
                st.lastPos,
                st.lastRot,
                st.lastSca);
        }

        public override string Interpolate(ECCinemable cinema, Keyframe before, Keyframe after, double time)
        {
            var distanceInTime = (after.start - before.start) * ECConstants.SecondPerFrame;
            var timeAmount = (float) (time - (before.start * ECConstants.SecondPerFrame));
                
            return ECKeyframeParser.LerpTransform(before.curve, before.data, after.data,
                timeAmount / distanceInTime);
        }

        public override bool RuntimePlay(ECCinemable cinema, ECClipTrack track, ECShowable show, string interpolated)
        {
            var obj = show.GetGameObject(track.go_variableName);
            
            if (obj != null)
            {
                return ECCinema.TransformPlayFromData(obj, interpolated);
            }
            else
            {
                throw new NullReferenceException($"GameObject Variable {track.go_variableName} is not set!");
            }
        }

        public override Keyframe CreateEmptyKeyframe(ECClipTrack track, int start, int end)
        {
            return new Keyframe
            {
                start = start,
                end = end,
                data = "0.0/0.0/0.0/0.0/0.0/0.0/0.0/0.0/0.0",
                track = track,
                startEndTogether = !track.hasDuration
            };
        }
    }
}