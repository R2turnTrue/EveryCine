using System;
using System.Collections.Generic;

namespace EveryCine
{
    public static class ECTypes
    {
        public static Dictionary<string, ECTrackType> trackTypes = new();

        public static void AddTrackType(string trackTypeT, ECTrackType inspector)
        {
            trackTypes[trackTypeT] = inspector;
        }

        public static ECTrackType GetTrackType(string trackTypeT)
        {
            return trackTypes[trackTypeT];
        }
    }
}